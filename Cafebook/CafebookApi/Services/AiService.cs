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
using System.Net.Sockets;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
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

        public async Task<string?> GetAnswerAsync(string userQuestion, int? idKhachHang, List<object> chatHistory)
        {
            var (apiKey, apiEndpoint, modelName, provider) = await GetAiSettingsAsync();

            if (string.IsNullOrEmpty(apiEndpoint))
            {
                return "Hệ thống AI chưa được cấu hình Endpoint. Vui lòng liên hệ Admin.";
            }

            var client = _httpClientFactory.CreateClient();
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
            var toolsDefinition = GetToolDefinitions();

            object payload;
            if (provider == AiProvider.Gemini)
                payload = BuildGeminiPayload(systemPrompt, chatHistory, userQuestion, toolsDefinition);
            else
                payload = BuildOpenAIPayload(systemPrompt, chatHistory, userQuestion, toolsDefinition, modelName);

            var aiResponse = await CallApiAsync(client, requestUrl, payload);

            if (aiResponse == null)
            {
                return "Hiện tại hệ thống AI đang quá tải hoặc tạm thời gián đoạn. Để mình kết nối bạn với nhân viên tư vấn trực tiếp của Cafebook nhé. [NEEDS_SUPPORT]";
            }

            string? finalResultText = null;

            if (provider == AiProvider.Gemini)
                finalResultText = await HandleGeminiFlow(client, requestUrl, (dynamic)payload, aiResponse.Value, idKhachHang);
            else
                finalResultText = await HandleOpenAIFlow(client, requestUrl, (dynamic)payload, aiResponse.Value, idKhachHang, provider);

            if (!string.IsNullOrEmpty(finalResultText))
            {
                string toolNamesPattern = "GET_THONG_TIN_CHUNG|GET_KHUYEN_MAI|HUONG_DAN_HE_THONG|HUONG_DAN_DAT_BAN|KIEM_TRA_SAN_PHAM|TIM_MON_THEO_LOAI|GET_GOI_Y_SAN_PHAM|KIEM_TRA_SACH|TIM_SACH_MO_RONG|GET_GOI_Y_SACH|GET_GOI_Y_COMBO|GET_DIEM_TICH_LUY|GET_THONG_TIN_CA_NHAN|GET_TONG_QUAN_TAI_KHOAN|GET_LICH_SU_DAT_BAN|HUY_DAT_BAN|GET_LICH_SU_THUE_SACH|GET_LICH_SU_DON_HANG|THEO_DOI_DON_HANG|KET_NOI_NHAN_VIEN";
                var matches = Regex.Matches(finalResultText, $@"(?:\[|\\\[).*?({toolNamesPattern})(?:\s*\(\s*(.*?)\s*\))?.*?(?:\]|\\\])", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    string fullMatch = match.Value;
                    string funcName = match.Groups[1].Value.ToUpper();
                    string argsStr = match.Groups[2].Value;

                    var dict = new Dictionary<string, string>();
                    if (!string.IsNullOrWhiteSpace(argsStr))
                    {
                        var argMatches = Regex.Matches(argsStr, @"([a-zA-Z0-9_]+)\s*[:=]\s*""?([^,""\}\)]+?)""?(?=\s*(?:,|$))");
                        foreach (Match m in argMatches)
                        {
                            dict[m.Groups[1].Value.Trim()] = m.Groups[2].Value.Trim();
                        }
                    }

                    var fakeFunctionCall = new { name = funcName, arguments = JsonSerializer.Serialize(dict) };

                    var (toolResult, _) = await ExecuteToolCallAsync(fakeFunctionCall, idKhachHang, provider);
                    string toolMsg = ExtractMessageFromTool(toolResult);

                    finalResultText = finalResultText.Replace(fullMatch, "\n\n" + toolMsg);
                }
            }

            if (!string.IsNullOrEmpty(finalResultText))
            {
                finalResultText = Regex.Replace(finalResultText, @"<think>.*?</think>", "", RegexOptions.Singleline);
                finalResultText = finalResultText.Replace("*", "").Trim();
            }

            return finalResultText;
        }

        private string ExtractMessageFromTool(object? toolResult)
        {
            if (toolResult == null) return "Đã xử lý xong yêu cầu.";
            try
            {
                var jsonDoc = JsonSerializer.SerializeToDocument(toolResult, _jsonOptions);
                if (jsonDoc.RootElement.TryGetProperty("message", out var msgEl) ||
                    jsonDoc.RootElement.TryGetProperty("Message", out msgEl))
                    return msgEl.GetString() ?? "";

                if (jsonDoc.RootElement.TryGetProperty("error", out var errEl) ||
                    jsonDoc.RootElement.TryGetProperty("Error", out errEl))
                    return errEl.GetString() ?? "";
            }
            catch { }
            return "Đã thực thi tác vụ thành công.";
        }

        private async Task<string?> HandleGeminiFlow(HttpClient client, string url, dynamic payload, JsonElement response, int? idKhachHang)
        {
            var (text, functionCall) = ParseGeminiResponse(response);

            if (functionCall != null)
            {
                var (toolResult, _) = await ExecuteToolCallAsync(functionCall, idKhachHang, AiProvider.Gemini);
                string toolMsg = ExtractMessageFromTool(toolResult);
                return string.IsNullOrEmpty(text) ? toolMsg : $"{text}\n\n{toolMsg}";
            }
            return text;
        }

        private async Task<string?> HandleOpenAIFlow(HttpClient client, string url, dynamic payload, JsonElement response, int? idKhachHang, AiProvider provider)
        {
            var (text, toolCallObj, toolCallId) = ParseOpenAIResponse(response);

            if (toolCallObj != null && !string.IsNullOrEmpty(toolCallId))
            {
                var (toolResult, _) = await ExecuteToolCallAsync(toolCallObj, idKhachHang, provider);
                string toolMsg = ExtractMessageFromTool(toolResult);
                return string.IsNullOrEmpty(text) ? toolMsg : $"{text}\n\n{toolMsg}";
            }
            return text;
        }

        private object BuildGeminiPayload(string systemPrompt, List<object> history, string userMsg, List<object> tools)
        {
            var contents = new List<object>
            {
                new { role = "user", parts = new[] { new { text = systemPrompt } } },
                new { role = "model", parts = new[] { new { text = "OK. Em đã hiểu nhiệm vụ." } } }
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
                        if (type == typeof(int))
                        {
                            if (el.ValueKind == JsonValueKind.Number) return (T)(object)el.GetInt32();
                            if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out int parsedInt)) return (T)(object)parsedInt;
                        }
                        if (type == typeof(double))
                        {
                            if (el.ValueKind == JsonValueKind.Number) return (T)(object)el.GetDouble();
                            if (el.ValueKind == JsonValueKind.String && double.TryParse(el.GetString(), out double parsedDouble)) return (T)(object)parsedDouble;
                        }
                        if (type == typeof(string))
                        {
                            if (el.ValueKind == JsonValueKind.String) return (T)(object)(el.GetString() ?? "");
                            if (el.ValueKind == JsonValueKind.Number) return (T)(object)el.GetRawText();
                        }
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

                    case "HUONG_DAN_HE_THONG": return (await _toolService.GetHuongDanHeThongAsync(GetArg("chuDe", "")), toolName);
                    case "HUONG_DAN_DAT_BAN": return (await _toolService.HuongDanDatBanAsync(idKhachHang), toolName);

                    case "KIEM_TRA_SAN_PHAM": return (await _toolService.KiemTraSanPhamAsync(GetArg("tenSanPham", "")), toolName);
                    case "TIM_MON_THEO_LOAI": return (await _toolService.TimMonTheoLoaiAsync(GetArg("loaiMon", "")), toolName);
                    case "GET_GOI_Y_SAN_PHAM": return (await _toolService.GetGoiYSanPhamAsync(), toolName);

                    case "KIEM_TRA_SACH": return (await _toolService.KiemTraSachAsync(GetArg("tenSach", "")), toolName);
                    case "GET_GOI_Y_SACH": return (await _toolService.GetGoiYSachAsync(), toolName);
                    case "GET_GOI_Y_COMBO": return (await _toolService.GetGoiYComboAsync(), toolName); // <-- THÊM DÒNG NÀY
                    case "TIM_SACH_MO_RONG": return (await _toolService.TimSachMoRongAsync(GetArg("loaiTimKiem", ""), GetArg("tuKhoa", "")), toolName);

                    case "GET_TONG_QUAN_TAI_KHOAN":
                        if (!idKhachHang.HasValue) return (await _toolService.GetYeuCauDangNhapAsync("xem thông tin tài khoản"), toolName);
                        return (await _toolService.GetTongQuanTaiKhoanAsync(idKhachHang.Value), toolName);

                    case "GET_DIEM_TICH_LUY":
                        if (!idKhachHang.HasValue) return (await _toolService.GetYeuCauDangNhapAsync("xem điểm tích lũy"), toolName);
                        return (await _toolService.GetDiemTichLuyAsync(idKhachHang.Value), toolName);

                    case "GET_THONG_TIN_CA_NHAN":
                        if (!idKhachHang.HasValue) return (await _toolService.GetYeuCauDangNhapAsync("xem thông tin cá nhân"), toolName);
                        return (await _toolService.GetThongTinCaNhanAsync(idKhachHang.Value), toolName);

                    case "GET_LICH_SU_DAT_BAN":
                        if (!idKhachHang.HasValue) return (await _toolService.GetYeuCauDangNhapAsync("xem lịch sử đặt bàn"), toolName);
                        return (await _toolService.GetLichSuDatBanAsync(idKhachHang.Value), toolName);

                    case "HUY_DAT_BAN":
                        if (!idKhachHang.HasValue) return (await _toolService.GetYeuCauDangNhapAsync("thực hiện hủy bàn"), toolName);
                        return (await _toolService.HuyDatBanAsync(GetArg("idPhieuDat", 0), GetArg("lyDo", ""), idKhachHang.Value), toolName);

                    case "GET_LICH_SU_THUE_SACH":
                        if (!idKhachHang.HasValue) return (await _toolService.GetYeuCauDangNhapAsync("xem sách đang thuê"), toolName);
                        return (await _toolService.GetLichSuThueSachAsync(idKhachHang.Value), toolName);

                    case "GET_LICH_SU_DON_HANG":
                        if (!idKhachHang.HasValue) return (await _toolService.GetYeuCauDangNhapAsync("xem lịch sử đơn hàng"), toolName);
                        return (await _toolService.GetLichSuDonHangAsync(idKhachHang.Value), toolName);

                    case "THEO_DOI_DON_HANG":
                        if (!idKhachHang.HasValue) return (await _toolService.GetYeuCauDangNhapAsync("theo dõi đơn hàng"), toolName);
                        return (await _toolService.TheoDoiDonHangAsync(GetArg("idHoaDon", 0), idKhachHang.Value), toolName);

                    case "KET_NOI_NHAN_VIEN":
                        return (await _toolService.KetNoiNhanVienAsync(), toolName);

                    default:
                        return (new { Error = $"Tool '{toolName}' không được hỗ trợ." }, toolName);
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
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                var response = await client.PostAsJsonAsync(url, payload, _jsonOptions, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cts.Token);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException || ex is HttpRequestException || ex is SocketException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<string> BuildSystemPromptAsync(int? idKhachHang)
        {
            string trangThaiKhach = idKhachHang.HasValue && idKhachHang > 0
                ? $"THÀNH VIÊN ĐĂNG NHẬP."
                : "KHÁCH VÃNG LAI.";

            string promptTemplate = "";
            string filePath = Path.Combine(_env.ContentRootPath, "SettingCafebook", "BuildSystemPromptAI.txt");

            if (File.Exists(filePath))
            {
                promptTemplate = await File.ReadAllTextAsync(filePath);
            }
            else
            {
                promptTemplate = @"Bạn là Trợ lý AI của quán cà phê Cafebook. Quán CHỈ CÓ 1 CHI NHÁNH DUY NHẤT.

                                QUY TẮC HIỂU NGỮ CẢNH & GỌI TOOL:
                                1. LẮNG NGHE NGỮ CẢNH TỰ NHIÊN: Khách hàng không phải lúc nào cũng nói rập khuôn. 
                                   - Nếu khách nói 'cho 1 cuốn sách ngày mới', 'đang chán quá', hãy gọi Tool gợi ý sách.
                                   - Nếu khách nói 'nay uống gì và đọc gì', 'tư vấn combo', hãy gọi Tool gợi ý combo (nước + sách).
                                2. KHÔNG HỎI NGƯỢC LẠI: Khi nhận được yêu cầu gợi ý (bất kể nước, sách hay combo), NGAY LẬP TỨC xuất ra cú pháp gọi Tool tương ứng. KHÔNG HỎI 'Bạn thích thể loại nào?', 'Bạn muốn đi mấy người?'.
                                3. TỰ ĐỘNG LẤY SỐ (ID): Nếu khách nhắc đến một con số (VD: 'đơn hàng 45', 'hủy bàn 26'), phải tự động đưa số đó vào tham số Tool.
                                4. CÚ PHÁP GỌI TOOL CHUẨN: [CALL: TEN_TOOL(tham_so=""gia_tri"")]
                                5. CHỈ DÙNG DỮ LIỆU TỪ TOOL: Không tự bịa tên sách, tên đồ uống hay giá tiền.

                                Thời gian: {ThoiGianHienTai}";
            }

            return promptTemplate
                .Replace("{trangThaiKhach}", trangThaiKhach)
                .Replace("{ThoiGianHienTai}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        }

        private List<object> GetToolDefinitions()
        {
            var tools = new List<object>
            {
                new { name = "GET_THONG_TIN_CHUNG", description = "BẮT BUỘC GỌI KHI khách hỏi: quán ở đâu, địa chỉ quán, giờ mở cửa, pass wifi, số điện thoại. KHÔNG HỎI LẠI.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "GET_KHUYEN_MAI", description = "BẮT BUỘC GỌI KHI khách hỏi về khuyến mãi. KHÔNG HỎI LẠI.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },

                new { name = "HUONG_DAN_HE_THONG", description = "BẮT BUỘC GỌI KHI khách muốn biết cách: đổi mật khẩu, lấy lại/quên mật khẩu, xem thông tin cá nhân, mua hàng, góp ý, chính sách. AI TỰ ĐỘNG truyền chủ đề, KHÔNG ĐƯỢC HỎI LẠI.", parameters = new { type = "object", properties = new { chuDe = new { type = "string" } }, required = Array.Empty<string>() } },

                new { name = "HUONG_DAN_DAT_BAN", description = "BẮT BUỘC GỌI NGAY KHI khách nhắc đến 'đặt bàn', 'muốn đặt bàn'. TUYỆT ĐỐI KHÔNG HỎI SỐ NGƯỜI, KHÔNG HỎI THỜI GIAN.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },

                new { name = "KIEM_TRA_SAN_PHAM", description = "BẮT BUỘC GỌI KHI khách hỏi CÓ BÁN MÓN NÀY KHÔNG.", parameters = new { type = "object", properties = new { tenSanPham = new { type = "string" } }, required = new[] { "tenSanPham" } } },
                // ĐÃ SỬA: Bắt buộc gọi thay vì hỏi ngược
                new { name = "TIM_MON_THEO_LOAI", description = "BẮT BUỘC GỌI KHI khách yêu cầu gợi ý loại đồ uống ĐÃ CÓ TÊN CỤ THỂ (trà sữa, cafe...). TUYỆT ĐỐI KHÔNG hỏi khách thích vị gì.", parameters = new { type = "object", properties = new { loaiMon = new { type = "string" } }, required = new[] { "loaiMon" } } },
                new { name = "GET_GOI_Y_SAN_PHAM", description = "BẮT BUỘC GỌI KHI khách KHÔNG NÓI RÕ LOẠI GÌ, chỉ nói gợi ý đồ uống/gợi ý món. TUYỆT ĐỐI KHÔNG HỎI LẠI.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },

                new { name = "KIEM_TRA_SACH", description = "BẮT BUỘC GỌI KHI khách tìm TÊN 1 CUỐN SÁCH CỤ THỂ.", parameters = new { type = "object", properties = new { tenSach = new { type = "string" } }, required = new[] { "tenSach" } } },
                // ĐÃ SỬA: Bắt buộc tự điền tham số để không hỏi ngược khách
                new { name = "TIM_SACH_MO_RONG", description = "BẮT BUỘC GỌI KHI khách tìm sách theo Tác Giả, Thể Loại. AI PHẢI TỰ ĐỘNG nhận diện 1 trong 3 loại: TacGia, TheLoai, NhaXuatBan, TUYỆT ĐỐI KHÔNG HỎI KHÁCH.", parameters = new { type = "object", properties = new { loaiTimKiem = new { type = "string", description = "TacGia | TheLoai | NhaXuatBan" }, tuKhoa = new { type = "string" } }, required = new[] { "loaiTimKiem", "tuKhoa" } } },
                new { name = "GET_GOI_Y_SACH", description = "Gọi khi khách muốn gợi ý đọc sách nói chung, hoặc dùng các ngữ cảnh tâm trạng (VD: 'cho cuốn sách hay cho ngày mới', 'đang buồn đọc gì', 'gợi ý sách hay').", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "GET_GOI_Y_COMBO", description = "Gọi NGAY LẬP TỨC khi khách yêu cầu gợi ý CẢ NƯỚC UỐNG VÀ SÁCH cùng lúc (VD: 'nay nên uống gì và đọc gì', 'tư vấn cho 1 combo đi', 'gợi ý combo').", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "GET_DIEM_TICH_LUY", description = "BẮT BUỘC GỌI KHI khách xem điểm tích lũy.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "GET_THONG_TIN_CA_NHAN", description = "BẮT BUỘC GỌI KHI khách xem thông tin cá nhân của họ.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "GET_TONG_QUAN_TAI_KHOAN", description = "BẮT BUỘC GỌI KHI tra cứu tổng quan tài khoản.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "GET_LICH_SU_DAT_BAN", description = "BẮT BUỘC GỌI KHI xem lịch sử đặt bàn.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "HUY_DAT_BAN", description = "BẮT BUỘC GỌI KHI khách dùng từ 'hủy bàn'. AI BẮT BUỘC tự lấy con số trong câu để điền vào tham số.", parameters = new { type = "object", properties = new { idPhieuDat = new { type = "integer" }, lyDo = new { type = "string" } }, required = new[] { "idPhieuDat" } } },
                new { name = "GET_LICH_SU_THUE_SACH", description = "BẮT BUỘC GỌI KHI xem lịch sử thuê sách.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "GET_LICH_SU_DON_HANG", description = "BẮT BUỘC GỌI KHI xem lịch sử mua hàng, lịch sử đơn hàng.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } },
                new { name = "THEO_DOI_DON_HANG", description = "BẮT BUỘC GỌI KHI khách kiểm tra chi tiết một đơn hàng. AI BẮT BUỘC tự lấy con số trong câu để điền vào tham số.", parameters = new { type = "object", properties = new { idHoaDon = new { type = "integer" } }, required = new[] { "idHoaDon" } } },
                new { name = "KET_NOI_NHAN_VIEN", description = "BẮT BUỘC GỌI KHI khách nói: 'nhân viên', 'chat với nhân viên', 'người thật', 'gặp admin'.", parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } }
            };

            return tools;
        }
    }
}