using CafebookApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CafebookApi.Services
{
    public class AiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly AiToolService _toolService;
        private readonly IWebHostEnvironment _env;
        private static readonly JsonSerializerOptions _jsonOptions;

        private enum AiProvider { Gemini, OpenAI, Ollama }

        static AiService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public AiService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, AiToolService toolService, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
            _toolService = toolService;
            _env = env;
        }

        // ============================================================
        // 1. CẤU HÌNH & TỰ ĐỘNG PHÁT HIỆN API
        // ============================================================

        private AiProvider DetectProvider(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return AiProvider.Gemini;
            if (endpoint.Contains("googleapis.com")) return AiProvider.Gemini;
            if (endpoint.Contains("localhost") || endpoint.Contains("127.0.0.1")) return AiProvider.Ollama;
            return AiProvider.OpenAI;
        }

        private async Task<(string ApiKey, string ApiEndpoint, string ModelName, AiProvider Provider)> GetAiSettingsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CafebookDbContext>();

                // TỐI ƯU: Chỉ Select đúng 3 record cần thiết từ SQL Server, tránh load rác vào RAM
                var aiKeys = new[] { "AI_Chat_API_Key", "AI_Chat_Endpoint", "AI_Chat_API_model" };
                var settings = await context.CaiDats.AsNoTracking()
                    .Where(c => aiKeys.Contains(c.TenCaiDat))
                    .ToListAsync();

                string apiKey = settings.FirstOrDefault(c => c.TenCaiDat == "AI_Chat_API_Key")?.GiaTri ?? "";
                string apiEndpoint = settings.FirstOrDefault(c => c.TenCaiDat == "AI_Chat_Endpoint")?.GiaTri ?? "";
                string modelName = settings.FirstOrDefault(c => c.TenCaiDat == "AI_Chat_API_model")?.GiaTri ?? "";

                var provider = DetectProvider(apiEndpoint);

                if (string.IsNullOrEmpty(modelName))
                {
                    if (provider == AiProvider.Ollama) modelName = "qwen3:1.7b";
                    else if (provider == AiProvider.OpenAI) modelName = "gpt-3.5-turbo";
                }

                return (apiKey, apiEndpoint, modelName, provider);
            }
        }

        // ============================================================
        // 2. LUỒNG XỬ LÝ CHÍNH
        // ============================================================

        public async Task<string?> GetAnswerAsync(string userQuestion, int? idKhachHang, List<object> chatHistory)
        {
            var (apiKey, apiEndpoint, modelName, provider) = await GetAiSettingsAsync();

            if (string.IsNullOrEmpty(apiEndpoint))
            {
                return "Hệ thống AI chưa được cấu hình Endpoint. Vui lòng liên hệ Admin.";
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(120);

            string requestUrl = apiEndpoint;

            switch (provider)
            {
                case AiProvider.Gemini:
                    requestUrl = $"{apiEndpoint}?key={apiKey}";
                    break;
                case AiProvider.OpenAI:
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    break;
                case AiProvider.Ollama:
                    if (!string.IsNullOrEmpty(apiKey) && apiKey != "ollama")
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    break;
            }

            string systemPrompt = await BuildSystemPromptAsync(idKhachHang);
            var toolsDefinition = GetToolDefinitions(idKhachHang);

            object payload;
            if (provider == AiProvider.Gemini)
                payload = BuildGeminiPayload(systemPrompt, chatHistory, userQuestion, toolsDefinition);
            else
                payload = BuildOpenAIPayload(systemPrompt, chatHistory, userQuestion, toolsDefinition, modelName);

            var aiResponse = await CallApiAsync(client, requestUrl, payload);
            if (aiResponse == null) return "Xin lỗi, hiện tại tôi không thể kết nối đến máy chủ AI (Lỗi kết nối).";

            if (provider == AiProvider.Gemini)
                return await HandleGeminiFlow(client, requestUrl, (dynamic)payload, aiResponse.Value, idKhachHang);
            else
                return await HandleOpenAIFlow(client, requestUrl, (dynamic)payload, aiResponse.Value, idKhachHang, provider);
        }

        // ============================================================
        // 3. XỬ LÝ LUỒNG
        // ============================================================

        private async Task<string?> HandleGeminiFlow(HttpClient client, string url, dynamic payload, JsonElement response, int? idKhachHang)
        {
            var (text, functionCall) = ParseGeminiResponse(response);

            if (!string.IsNullOrEmpty(text) && functionCall == null) return text;

            if (functionCall != null)
            {
                var (toolResult, toolName) = await ExecuteToolCallAsync(functionCall, idKhachHang, AiProvider.Gemini);

                var contents = (List<object>)payload.contents;
                contents.Add(new { role = "model", parts = new[] { new { functionCall = functionCall } } });
                contents.Add(new
                {
                    role = "function",
                    parts = new[] { new { functionResponse = new { name = toolName, response = toolResult } } }
                });

                var finalResponse = await CallApiAsync(client, url, (object)payload);
                if (finalResponse == null) return "Có lỗi khi xử lý thông tin từ hệ thống.";

                var (finalText, _) = ParseGeminiResponse(finalResponse.Value);
                return finalText;
            }
            return null;
        }

        private async Task<string?> HandleOpenAIFlow(HttpClient client, string url, dynamic payload, JsonElement response, int? idKhachHang, AiProvider provider)
        {
            var (text, toolCallObj, toolCallId) = ParseOpenAIResponse(response);

            if (!string.IsNullOrEmpty(text) && toolCallObj == null) return text;

            if (toolCallObj != null && !string.IsNullOrEmpty(toolCallId))
            {
                var (toolResult, toolName) = await ExecuteToolCallAsync(toolCallObj, idKhachHang, provider);

                var messages = (List<object>)payload.messages;
                messages.Add(new
                {
                    role = "assistant",
                    tool_calls = new[] {
                        new {
                            id = toolCallId,
                            type = "function",
                            function = toolCallObj
                        }
                    }
                });

                messages.Add(new
                {
                    role = "tool",
                    tool_call_id = toolCallId,
                    name = toolName,
                    content = JsonSerializer.Serialize(toolResult, _jsonOptions)
                });

                var finalResponse = await CallApiAsync(client, url, (object)payload);
                if (finalResponse == null) return "Có lỗi khi xử lý thông tin từ hệ thống.";

                var (finalText, _, _) = ParseOpenAIResponse(finalResponse.Value);
                return finalText;
            }
            return null;
        }

        // ============================================================
        // 4. PAYLOAD BUILDERS & PARSERS
        // ============================================================

        private object BuildGeminiPayload(string systemPrompt, List<object> history, string userMsg, List<object> tools)
        {
            var contents = new List<object>
            {
                new { role = "user", parts = new[] { new { text = systemPrompt } } },
                new { role = "model", parts = new[] { new { text = "OK. Em đã hiểu nhiệm vụ và sẽ tuân thủ tuyệt đối." } } }
            };

            if (history != null) contents.AddRange(history);
            contents.Add(new { role = "user", parts = new[] { new { text = userMsg } } });

            return new
            {
                contents = contents,
                tools = new[] { new { functionDeclarations = tools } },
                toolConfig = new { functionCallingConfig = new { mode = "AUTO" } }
            };
        }

        private object BuildOpenAIPayload(string systemPrompt, List<object> history, string userMsg, List<object> tools, string modelName)
        {
            var messages = new List<object> { new { role = "system", content = systemPrompt } };

            if (history != null)
            {
                foreach (dynamic msg in history)
                {
                    try
                    {
                        string json = JsonSerializer.Serialize((object)msg);
                        var element = JsonDocument.Parse(json).RootElement;
                        string role = "user";
                        string text = "";

                        if (element.TryGetProperty("role", out var roleEl))
                            role = roleEl.GetString() == "model" ? "assistant" : "user";

                        if (element.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                            text = parts[0].GetProperty("text").GetString() ?? "";
                        else if (element.TryGetProperty("content", out var contentEl))
                            text = contentEl.GetString() ?? "";

                        if (!string.IsNullOrEmpty(text)) messages.Add(new { role = role, content = text });
                    }
                    catch { }
                }
            }
            messages.Add(new { role = "user", content = userMsg });

            var openAiTools = tools.Select(t => new { type = "function", function = t }).ToList();

            return new
            {
                model = modelName,
                messages = messages,
                tools = openAiTools,
                temperature = 0.7,
                stream = false
            };
        }

        private (string? text, object? functionCall) ParseGeminiResponse(JsonElement aiResponse)
        {
            try
            {
                if (aiResponse.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var content = candidates[0].GetProperty("content");
                    if (content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var part = parts[0];
                        if (part.TryGetProperty("functionCall", out var fc)) return (null, fc);
                        if (part.TryGetProperty("text", out var txt)) return (txt.GetString(), null);
                    }
                }
            }
            catch { }
            return (null, null);
        }

        private (string? text, object? toolCallObj, string? toolId) ParseOpenAIResponse(JsonElement aiResponse)
        {
            try
            {
                if (aiResponse.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var message = choices[0].GetProperty("message");

                    if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.GetArrayLength() > 0)
                    {
                        var firstTool = toolCalls[0];
                        string id = firstTool.GetProperty("id").GetString() ?? "";
                        var function = firstTool.GetProperty("function");
                        return (null, function, id);
                    }

                    if (message.TryGetProperty("content", out var content))
                    {
                        return (content.GetString(), null, null);
                    }
                }
            }
            catch { }
            return (null, null, null);
        }

        // ============================================================
        // 5. THỰC THI TOOL
        // ============================================================

        private async Task<(object? toolResult, string toolName)> ExecuteToolCallAsync(object functionCall, int? idKhachHang, AiProvider provider)
        {
            var callJson = JsonSerializer.SerializeToElement(functionCall);
            string toolName = callJson.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? "" : "";

            JsonElement args;
            if (provider == AiProvider.OpenAI || provider == AiProvider.Ollama)
            {
                string argsString = callJson.TryGetProperty("arguments", out var argEl) ? (argEl.GetString() ?? "{}") : "{}";
                try { args = JsonDocument.Parse(argsString).RootElement; } catch { args = JsonDocument.Parse("{}").RootElement; }
            }
            else
            {
                if (!callJson.TryGetProperty("args", out args)) args = JsonDocument.Parse("{}").RootElement;
            }

            T GetArg<T>(string key, T defaultValue)
            {
                if (args.TryGetProperty(key, out var el))
                {
                    try
                    {
                        var type = typeof(T);
                        if (type == typeof(int)) return (T)(object)el.GetInt32();
                        if (type == typeof(double)) return (T)(object)el.GetDouble();
                        if (type == typeof(string)) return (T)(object)(el.GetString() ?? "");
                    }
                    catch { }
                }
                return defaultValue;
            }

            try
            {
                switch (toolName)
                {
                    case "GET_THONG_TIN_CHUNG": return (await _toolService.GetThongTinChungAsync(), toolName);
                    case "GET_KHUYEN_MAI": return (await _toolService.GetKhuyenMaiAsync(), toolName);
                    case "KIEM_TRA_SAN_PHAM": return (await _toolService.KiemTraSanPhamAsync(GetArg("tenSanPham", "")), toolName);
                    case "TIM_MON_THEO_LOAI": return (await _toolService.TimMonTheoLoaiAsync(GetArg("loaiMon", "")), toolName);
                    case "GET_GOI_Y_SAN_PHAM": return (await _toolService.GetGoiYSanPhamAsync(), toolName);
                    case "KIEM_TRA_SACH": return (await _toolService.KiemTraSachAsync(GetArg("tenSach", "")), toolName);
                    case "TIM_SACH_THEO_TAC_GIA": return (await _toolService.TimSachTheoTacGiaAsync(GetArg("tenTacGia", "")), toolName);
                    case "GET_GOI_Y_SACH": return (await _toolService.GetGoiYSachAsync(), toolName);
                    case "KIEM_TRA_BAN": return (await _toolService.KiemTraBanTrongAsync(GetArg("soNguoi", 2)), toolName);

                    case "DAT_BAN_THUC_SU":
                        string timeStr = GetArg("thoiGianDat", "");
                        if (!DateTime.TryParse(timeStr, out DateTime thoiGianDat)) thoiGianDat = DateTime.Now;
                        return (await _toolService.DatBanThucSuAsync(GetArg("tenBan", ""), GetArg("soNguoi", 2), thoiGianDat, GetArg("hoTen", ""), GetArg("sdt", ""), GetArg("email", ""), GetArg("ghiChu", ""), idKhachHang), toolName);

                    case "GET_TONG_QUAN_TAI_KHOAN":
                        if (!idKhachHang.HasValue) return ("Yêu cầu đăng nhập để xem thông tin.", toolName);
                        return (await _toolService.GetTongQuanTaiKhoanAsync(idKhachHang.Value), toolName);

                    case "GET_DIEM_TICH_LUY":
                        if (!idKhachHang.HasValue) return ("Yêu cầu đăng nhập để xem điểm.", toolName);
                        return (await _toolService.GetDiemTichLuyAsync(idKhachHang.Value), toolName);

                    case "GET_THONG_TIN_CA_NHAN":
                        if (!idKhachHang.HasValue) return ("Yêu cầu đăng nhập để xem thông tin.", toolName);
                        return (await _toolService.GetThongTinCaNhanAsync(idKhachHang.Value), toolName);

                    case "GET_LICH_SU_DAT_BAN":
                        if (!idKhachHang.HasValue) return ("Yêu cầu đăng nhập để xem lịch sử.", toolName);
                        return (await _toolService.GetLichSuDatBanAsync(idKhachHang.Value), toolName);

                    case "HUY_DAT_BAN":
                        if (!idKhachHang.HasValue) return ("Yêu cầu đăng nhập để thực hiện.", toolName);
                        return (await _toolService.HuyDatBanAsync(GetArg("idPhieuDat", 0), GetArg("lyDo", ""), idKhachHang.Value), toolName);

                    case "GET_LICH_SU_THUE_SACH":
                        if (!idKhachHang.HasValue) return ("Yêu cầu đăng nhập để xem sách đang thuê.", toolName);
                        return (await _toolService.GetLichSuThueSachAsync(idKhachHang.Value), toolName);

                    case "GET_LICH_SU_DON_HANG":
                        if (!idKhachHang.HasValue) return ("Yêu cầu đăng nhập để xem đơn hàng.", toolName);
                        return (await _toolService.GetLichSuDonHangAsync(idKhachHang.Value), toolName);

                    case "THEO_DOI_DON_HANG":
                        if (!idKhachHang.HasValue) return ("Yêu cầu đăng nhập để theo dõi đơn.", toolName);
                        return (await _toolService.TheoDoiDonHangAsync(GetArg("idHoaDon", 0), idKhachHang.Value), toolName);

                    default:
                        return (new { Error = $"Tool '{toolName}' không được hỗ trợ trong hệ thống." }, toolName);
                }
            }
            catch (Exception ex)
            {
                return (new { Error = $"Lỗi hệ thống khi gọi tool: {ex.Message}" }, toolName);
            }
        }

        private async Task<JsonElement?> CallApiAsync(HttpClient client, string url, object payload)
        {
            try
            {
                var response = await client.PostAsJsonAsync(url, payload, _jsonOptions);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {err}");
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<JsonElement>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        // ============================================================
        // 6. SYSTEM PROMPT (TÍCH HỢP FILE NGOÀI & FALLBACK)
        // ============================================================

        private async Task<string> BuildSystemPromptAsync(int? idKhachHang)
        {
            string trangThaiKhach = idKhachHang.HasValue && idKhachHang > 0
                ? $"KHÁCH HÀNG: THÀNH VIÊN (ID: {idKhachHang}).\n" +
                  $"   -> QUYỀN HẠN: Tra cứu toàn bộ lịch sử cá nhân (Đặt bàn, Đơn hàng, Thuê sách) và Hủy đặt bàn.\n" +
                  $"   -> ĐẶC QUYỀN GỌI NHÂN VIÊN: NẾU khách yêu cầu gặp nhân viên hỗ trợ thực tế, BẠN ĐƯỢC PHÉP thêm chuỗi [NEEDS_SUPPORT] vào câu trả lời để hệ thống tự động chuyển tín hiệu cho nhân viên."
                : "KHÁCH HÀNG: VÃNG LAI (Chưa đăng nhập).\n" +
                  "   -> QUYỀN HẠN CƠ BẢN: CÔNG KHAI trả lời mọi thông tin chung về QUÁN (Địa chỉ, giờ mở cửa, wifi, menu món ăn, sách thư viện, khuyến mãi).\n" +
                  "   -> HẠN CHẾ 1: TUYỆT ĐỐI KHÔNG tra cứu thông tin cá nhân, điểm tích lũy hay lịch sử tài khoản của bất kỳ khách hàng nào. Nếu bị hỏi, yêu cầu đăng nhập.\n" +
                  "   -> HẠN CHẾ 2: Khách vãng lai KHÔNG CÓ QUYỀN gọi nhân viên. TUYỆT ĐỐI KHÔNG xuất ra chuỗi [NEEDS_SUPPORT] dưới mọi hình thức.";

            string promptTemplate = "";
            string filePath = Path.Combine(_env.ContentRootPath, "SettingCafebook", "BuildSystemPromptAI.txt");

            if (File.Exists(filePath))
            {
                promptTemplate = await File.ReadAllTextAsync(filePath);
            }
            else
            {
                promptTemplate = @"Bạn là Trợ lý AI của quán cà phê & thư viện Cafebook. Phong cách: Thân thiện, tự nhiên, chuyên nghiệp, xưng 'Em/Mình' và gọi khách là 'Anh/Chị/Bạn'.

QUY TẮC CỐT LÕI (TUÂN THỦ TUYỆT ĐỐI):
1. BẢO MẬT HỆ THỐNG & DỮ LIỆU: Chỉ trả lời dựa trên thông tin thu được từ TOOLS. TUYỆT ĐỐI KHÔNG tự bịa dữ liệu, không tiết lộ tên Tool hay giải thích về token mã hóa.

2. QUY TẮC TRA CỨU & GỢI Ý (RẤT QUAN TRỌNG):
   - Khi khách hỏi 'Quán có món X không?' -> BẮT BUỘC gọi Tool KIEM_TRA_SAN_PHAM / TIM_MON_THEO_LOAI.
   - Khi khách nhờ gợi ý (Ví dụ: 'Có gì ngon không?', 'Gợi ý đi', 'Ok') -> BẮT BUỘC gọi Tool GET_GOI_Y_SAN_PHAM hoặc GET_GOI_Y_SACH. TUYỆT ĐỐI KHÔNG tự bịa ra tên món ăn/sách khi chưa gọi Tool.

3. TRẢ LỜI VỀ TÌNH TRẠNG:
   - Món ăn: Nếu Status là 'Đang kinh doanh' -> có sẵn. Nếu 'Ngừng kinh doanh' -> báo tạm hết.
   - Sách: Nếu Status 'Có sẵn' -> còn trên kệ. Nếu 'Đã hết' -> đã có người mượn. (TUYỆT ĐỐI KHÔNG gợi ý hay cung cấp tính năng 'Thuê sách trực tuyến').

4. ĐỊNH DẠNG: Tiền tệ 50.000đ. Ngày tháng dd/MM/yyyy HH:mm.

5. QUY TẮC NÚT BẤM (BUTTON - ÉP BUỘC):
   - NẾU kết quả từ Tool trả về một mảng `Actions` (gồm Label và Link), BẠN BẮT BUỘC PHẢI TẠO NÚT CHO TỪNG PHẦN TỬ TRONG MẢNG ĐÓ.
   - Cú pháp chuẩn: [BUTTON: Label | Link]. (Ví dụ: [BUTTON: Xem chi tiết | /chi-tiet/abc...])
   - Đặt tất cả các nút ở cuối cùng của câu trả lời. Giữ nguyên 100% đường dẫn Link mã hóa.

6. LUỒNG ĐẶT BÀN: Hỏi số người và thời gian -> Dùng KIEM_TRA_BAN -> Khách chốt -> Dùng DAT_BAN_THUC_SU.

NGỮ CẢNH HIỆN TẠI:
- {trangThaiKhach}
- Thời gian: {ThoiGianHienTai}";
            }

            return promptTemplate
                .Replace("{trangThaiKhach}", trangThaiKhach)
                .Replace("{ThoiGianHienTai}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        }

        // ============================================================
        // 7. TOOL DEFINITIONS
        // ============================================================

        private List<object> GetToolDefinitions(int? idKhachHang)
        {
            var tools = new List<object>
            {
                new { name = "GET_THONG_TIN_CHUNG", description = "Lấy thông tin chung của quán: Giờ mở cửa, Wifi, Địa chỉ, Hotline, Quy định.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "GET_KHUYEN_MAI", description = "Tra cứu khuyến mãi đang diễn ra.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },

                new { name = "KIEM_TRA_SAN_PHAM", description = "BẮT BUỘC gọi khi khách hỏi món cụ thể. LƯU Ý QUAN TRỌNG: Chỉ truyền vào TỪ KHÓA LÕI ngắn gọn nhất (VD: 'bạc xỉu', 'đen đá', 'trà đào'), TUYỆT ĐỐI KHÔNG truyền cả câu dài.", parameters = new { type = "object", properties = new { tenSanPham = new { type = "string" } }, required = new[] { "tenSanPham" } } },
                new { name = "TIM_MON_THEO_LOAI", description = "BẮT BUỘC gọi khi khách hỏi về một nhóm danh mục chung (VD: có trà sữa không, có cafe không, có trà không, có bánh không).", parameters = new { type = "object", properties = new { loaiMon = new { type = "string" } }, required = new[] { "loaiMon" } } },

                new { name = "GET_GOI_Y_SAN_PHAM", description = "BẮT BUỘC gọi khi khách nhờ tư vấn món ngon, hoặc khách nói 'có gì ngon không', 'gợi ý đi', hoặc khách đồng ý cho bạn gợi ý (VD: 'ok', 'ừ').", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "KIEM_TRA_SACH", description = "BẮT BUỘC gọi khi khách hỏi quán có cuốn sách cụ thể nào không. LƯU Ý: Chỉ trích xuất tên chính của sách, bỏ các từ thừa thãi.", parameters = new { type = "object", properties = new { tenSach = new { type = "string" } }, required = new[] { "tenSach" } } },
                new { name = "TIM_SACH_THEO_TAC_GIA", description = "Tìm sách theo tên tác giả.", parameters = new { type = "object", properties = new { tenTacGia = new { type = "string" } }, required = new[] { "tenTacGia" } } },
                new { name = "GET_GOI_Y_SACH", description = "BẮT BUỘC gọi khi khách nhờ tư vấn sách hay, hoặc đồng ý cho bạn gợi ý sách.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "KIEM_TRA_BAN", description = "Kiểm tra danh sách bàn trống.", parameters = new { type = "object", properties = new { soNguoi = new { type = "integer" } }, required = new[] { "soNguoi" } } },
                new { name = "DAT_BAN_THUC_SU", description = "Thực hiện đặt bàn (Chỉ gọi khi khách đã chốt).", parameters = new { type = "object", properties = new { tenBan = new { type = "string" }, soNguoi = new { type = "integer" }, thoiGianDat = new { type = "string", description = "Format: yyyy-MM-dd HH:mm" }, hoTen = new { type = "string" }, sdt = new { type = "string" }, email = new { type = "string" }, ghiChu = new { type = "string" } }, required = new[] { "tenBan", "soNguoi", "thoiGianDat", "hoTen", "sdt" } } }
            };

            if (idKhachHang.HasValue && idKhachHang > 0)
            {
                tools.Add(new { name = "GET_DIEM_TICH_LUY", description = "Lấy điểm tích lũy.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } });
                tools.Add(new { name = "GET_THONG_TIN_CA_NHAN", description = "Lấy thông tin profile khách hàng.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } });
                tools.Add(new { name = "GET_TONG_QUAN_TAI_KHOAN", description = "Tra cứu tổng quan cá nhân và điểm.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } });
                tools.Add(new { name = "GET_LICH_SU_DAT_BAN", description = "Xem lịch sử đặt bàn.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } });
                tools.Add(new { name = "HUY_DAT_BAN", description = "Hủy phiếu đặt bàn.", parameters = new { type = "object", properties = new { idPhieuDat = new { type = "integer" }, lyDo = new { type = "string" } }, required = new[] { "idPhieuDat" } } });
                tools.Add(new { name = "GET_LICH_SU_THUE_SACH", description = "Xem sách đang thuê.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } });
                tools.Add(new { name = "GET_LICH_SU_DON_HANG", description = "Xem lịch sử đơn hàng.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } });
                tools.Add(new { name = "THEO_DOI_DON_HANG", description = "Theo dõi đơn hàng chi tiết.", parameters = new { type = "object", properties = new { idHoaDon = new { type = "integer" } }, required = new[] { "idHoaDon" } } });
            }
            return tools;
        }
    }
}