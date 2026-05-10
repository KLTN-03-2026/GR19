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

        // Bổ sung thêm Anthropic vào danh sách hỗ trợ
        private enum AiProvider { Gemini, OpenAI, Ollama, Anthropic }

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

        // Tự động nhận diện Provider dựa vào URL
        private AiProvider DetectProvider(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return AiProvider.Gemini;

            string lowerEndpoint = endpoint.ToLower();
            if (lowerEndpoint.Contains("googleapis.com") || lowerEndpoint.Contains("gemini"))
                return AiProvider.Gemini;

            if (lowerEndpoint.Contains("anthropic.com") || lowerEndpoint.EndsWith("/v1/messages"))
                return AiProvider.Anthropic;

            if (lowerEndpoint.Contains("localhost") || lowerEndpoint.Contains("127.0.0.1") || lowerEndpoint.Contains("ollama"))
                return AiProvider.Ollama;

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
                    if (provider == AiProvider.Ollama) modelName = "qwen2.5";
                    else if (provider == AiProvider.OpenAI) modelName = "gpt-3.5-turbo";
                    else if (provider == AiProvider.Anthropic) modelName = "claude-3-haiku-20240307";
                }

                return (apiKey, apiEndpoint, modelName, provider);
            }
        }

        public async Task<string?> GetAnswerAsync(string userQuestion, int? idKhachHang, List<object> chatHistory)
        {
            try
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
                        if (!requestUrl.Contains("?key=")) requestUrl = $"{apiEndpoint}?key={apiKey}";
                        break;
                    case AiProvider.Anthropic:
                        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                        break;
                    case AiProvider.OpenAI:
                    case AiProvider.Ollama:
                        if (!string.IsNullOrEmpty(apiKey) && apiKey.ToLower() != "ollama")
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                        break;
                }

                string systemPrompt = await BuildSystemPromptAsync(idKhachHang);
                var toolsDefinition = GetToolDefinitions();

                object payload;
                if (provider == AiProvider.Gemini)
                    payload = BuildGeminiPayload(systemPrompt, chatHistory, userQuestion, toolsDefinition);
                else if (provider == AiProvider.Anthropic)
                    payload = BuildAnthropicPayload(systemPrompt, chatHistory, userQuestion, toolsDefinition, modelName);
                else
                    payload = BuildOpenAIPayload(systemPrompt, chatHistory, userQuestion, toolsDefinition, modelName);

                var aiResponse = await CallApiAsync(client, requestUrl, payload);

                if (aiResponse == null)
                {
                    return await FallbackLocalChatAsync(userQuestion, idKhachHang);
                }

                string? finalResultText = null;

                switch (provider)
                {
                    case AiProvider.Gemini:
                        finalResultText = await HandleGeminiFlow(client, requestUrl, payload, aiResponse.Value, idKhachHang);
                        break;
                    case AiProvider.Anthropic:
                        finalResultText = await HandleAnthropicFlow(client, requestUrl, payload, aiResponse.Value, idKhachHang, provider);
                        break;
                    case AiProvider.OpenAI:
                    case AiProvider.Ollama:
                        finalResultText = await HandleOpenAIFlow(client, requestUrl, payload, aiResponse.Value, idKhachHang, provider);
                        break;
                }

                if (!string.IsNullOrEmpty(finalResultText))
                {
                    string toolNamesPattern = "GET_THONG_TIN_CHUNG|GET_KHUYEN_MAI|HUONG_DAN_HE_THONG|HUONG_DAN_DAT_BAN|KIEM_TRA_SAN_PHAM|TIM_MON_THEO_LOAI|GET_GOI_Y_SAN_PHAM|KIEM_TRA_SACH|TIM_SACH_MO_RONG|GET_GOI_Y_SACH|GET_GOI_Y_COMBO|GET_DIEM_TICH_LUY|GET_THONG_TIN_CA_NHAN|GET_TONG_QUAN_TAI_KHOAN|GET_LICH_SU_DAT_BAN|HUY_DAT_BAN|GET_LICH_SU_THUE_SACH|GET_LICH_SU_DON_HANG|THEO_DOI_DON_HANG|KET_NOI_NHAN_VIEN|GET_BAN_CHAY|QUERY_DATABASE";
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
            catch (Exception)
            {
                return await FallbackLocalChatAsync(userQuestion, idKhachHang);
            }
        }

        // --- KHU VỰC BUILD PAYLOAD ---

        private object BuildAnthropicPayload(string systemPrompt, List<object> history, string userMsg, List<object> tools, string modelName)
        {
            var messages = new List<object>();

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
                            role = (roleEl.GetString() == "model" || roleEl.GetString() == "assistant") ? "assistant" : "user";

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

            var anthropicTools = new List<object>();
            var toolsJson = JsonSerializer.Serialize(tools, _jsonOptions);
            var toolsDoc = JsonDocument.Parse(toolsJson);

            foreach (var item in toolsDoc.RootElement.EnumerateArray())
            {
                anthropicTools.Add(new
                {
                    name = item.GetProperty("name").GetString(),
                    description = item.GetProperty("description").GetString(),
                    input_schema = item.GetProperty("parameters")
                });
            }

            return new
            {
                model = modelName,
                system = systemPrompt, 
                messages = messages,
                tools = anthropicTools.Any() ? anthropicTools : null,
                max_tokens = 2048
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

        private object BuildGeminiPayload(string systemPrompt, List<object> history, string userMsg, List<object> tools)
        {
            var contents = new List<object>
            {
                new { role = "user", parts = new[] { new { text = systemPrompt } } },
                new { role = "model", parts = new[] { new { text = "Dạ vâng, em đã hiểu rõ nhiệm vụ và sẽ giao tiếp tự nhiên nhất ạ." } } }
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

        // --- KHU VỰC PARSE VÀ XỬ LÝ FLOW ---

        private async Task<string?> HandleAnthropicFlow(HttpClient client, string url, object payload, JsonElement response, int? idKhachHang, AiProvider provider)
        {
            var (text, toolCallObj, toolId) = ParseAnthropicResponse(response);

            if (toolCallObj != null)
            {
                var (toolResult, _) = await ExecuteToolCallAsync(toolCallObj, idKhachHang, provider);
                string toolMsg = ExtractMessageFromTool(toolResult);
                return string.IsNullOrEmpty(text) ? toolMsg : $"{text}\n\n{toolMsg}";
            }
            return text;
        }

        private (string? text, object? toolCallObj, string? toolId) ParseAnthropicResponse(JsonElement aiResponse)
        {
            try
            {
                string textResult = "";
                object? toolCall = null;
                string? toolId = null;   

                if (aiResponse.TryGetProperty("content", out var contentArr))
                {
                    foreach (var item in contentArr.EnumerateArray())
                    {
                        if (item.TryGetProperty("type", out var typeEl))
                        {
                            string type = typeEl.GetString() ?? "";
                            if (type == "text" && item.TryGetProperty("text", out var textEl))
                            {
                                textResult += textEl.GetString() ?? ""; 
                            }
                            else if (type == "tool_use")
                            {
                                toolId = item.GetProperty("id").GetString();
                                string name = item.GetProperty("name").GetString() ?? ""; 
                                string inputArgs = item.GetProperty("input").GetRawText();
                                toolCall = new { name = name, arguments = inputArgs };
                            }
                        }
                    }
                    return (textResult.Trim(), toolCall, toolId);
                }
            }
            catch { }
            return (null, null, null);
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

        // --- CÁC PHƯƠNG THỨC TIỆN ÍCH KHÁC (GIỮ NGUYÊN HOẶC TINH CHỈNH NHẸ) ---

        private async Task<(object? toolResult, string toolName)> ExecuteToolCallAsync(object functionCall, int? idKhachHang, AiProvider provider)
        {
            var callJson = JsonSerializer.SerializeToElement(functionCall);
            string toolName = callJson.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? "" : "";

            JsonElement args;
            if (provider == AiProvider.OpenAI || provider == AiProvider.Ollama || provider == AiProvider.Anthropic)
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
                    case "GET_BAN_CHAY": return (await _toolService.GetBanChayAsync(), toolName);
                    case "QUERY_DATABASE": return (await _toolService.QueryDatabaseAsync(GetArg("sqlQuery", "")), toolName);
                    case "GET_THONG_TIN_CHUNG": return (await _toolService.GetThongTinChungAsync(), toolName);
                    case "GET_KHUYEN_MAI": return (await _toolService.GetKhuyenMaiAsync(), toolName);
                    case "HUONG_DAN_HE_THONG": return (await _toolService.GetHuongDanHeThongAsync(GetArg("chuDe", "")), toolName);
                    case "HUONG_DAN_DAT_BAN": return (await _toolService.HuongDanDatBanAsync(idKhachHang), toolName);
                    case "KIEM_TRA_SAN_PHAM": return (await _toolService.KiemTraSanPhamAsync(GetArg("tenSanPham", "")), toolName);
                    case "TIM_MON_THEO_LOAI": return (await _toolService.TimMonTheoLoaiAsync(GetArg("loaiMon", "")), toolName);
                    case "GET_GOI_Y_SAN_PHAM": return (await _toolService.GetGoiYSanPhamAsync(), toolName);
                    case "KIEM_TRA_SACH": return (await _toolService.KiemTraSachAsync(GetArg("tenSach", "")), toolName);
                    case "GET_GOI_Y_SACH": return (await _toolService.GetGoiYSachAsync(), toolName);
                    case "GET_GOI_Y_COMBO": return (await _toolService.GetGoiYComboAsync(), toolName);
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
                        if (!idKhachHang.HasValue || idKhachHang <= 0)
                            return (await _toolService.GetYeuCauDangNhapAsync("kết nối trực tiếp với nhân viên hỗ trợ"), toolName);
                        return (await _toolService.KetNoiNhanVienAsync(), toolName);

                    case "GET_TRANG_THAI_TAI_KHOAN":
                        if (!idKhachHang.HasValue || idKhachHang <= 0)
                            return (await _toolService.GetYeuCauDangNhapAsync("kiểm tra trạng thái tài khoản"), toolName);
                        return (await _toolService.GetTrangThaiTaiKhoanAsync(idKhachHang.Value), toolName);

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
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                var response = await client.PostAsJsonAsync(url, payload, _jsonOptions, cts.Token);
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cts.Token);
            }
            catch { return null; }
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
                promptTemplate = @"Bạn là Trợ lý AI của quán cà phê Cafebook, tên là 'Cafebook AI'. Quán CHỈ CÓ 1 CHI NHÁNH DUY NHẤT.
                                Phong cách giao tiếp: Tự nhiên, thân thiện, ấm áp và giống con người nhất có thể. Hãy trả lời như một người bạn, sử dụng từ ngữ mềm mỏng, thấu hiểu cảm xúc của khách hàng.

                                QUY TẮC BẮT BUỘC (TUYỆT ĐỐI TUÂN THỦ):
                                1. GỌI TOOL KHI CẦN THIẾT. KHÔNG ĐƯỢC TỰ BỊA DỮ LIỆU.
                                2. TRUY VẤN LINH HOẠT BẰNG DATABASE bằng tool QUERY_DATABASE.
                                3. TỰ ĐỘNG LẤY SỐ (ID) điền vào tham số của Tool.
                                4. CÚ PHÁP GỌI TOOL: [CALL: TEN_TOOL(tham_so=""gia_tri"")]
                                5. TRÁNH LẶP LẠI.
                                6. LỖI KÝ TỰ / CHAT NHÂN VIÊN: Nếu khách muốn chat với nhân viên, gọi [CALL: KET_NOI_NHAN_VIEN()].

                                Thời gian hiện tại: {ThoiGianHienTai}";
            }

            return promptTemplate
                .Replace("{trangThaiKhach}", trangThaiKhach)
                .Replace("{ThoiGianHienTai}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        }

        private async Task<string> FallbackLocalChatAsync(string input, int? idKhachHang)
        {
            input = input.ToLower();
            int customerId = idKhachHang ?? 0;
            bool daDangNhap = customerId > 0;

            if (input.Contains("điểm tích lũy") || input.Contains("điểm của tôi") || input.Contains("thông tin cá nhân") ||
                input.Contains("lịch sử đặt bàn") || input.Contains("lịch sử thuê sách") || input.Contains("lịch sử đơn hàng"))
            {
                if (!daDangNhap)
                    return "Dạ, để xem thông tin cá nhân và lịch sử, bạn vui lòng đăng nhập trước nhé.\n[BUTTON: Đăng nhập | /dang-nhap]";

                if (input.Contains("điểm tích lũy") || input.Contains("điểm của tôi")) return ExtractMessageFromTool(await _toolService.GetDiemTichLuyAsync(customerId));
                if (input.Contains("thông tin cá nhân")) return ExtractMessageFromTool(await _toolService.GetThongTinCaNhanAsync(customerId));
                if (input.Contains("lịch sử đặt bàn")) return ExtractMessageFromTool(await _toolService.GetLichSuDatBanAsync(customerId));
                if (input.Contains("lịch sử thuê sách") || input.Contains("đang mượn") || input.Contains("đang thuê")) return ExtractMessageFromTool(await _toolService.GetLichSuThueSachAsync(customerId));
                if (input.Contains("lịch sử đơn hàng")) return ExtractMessageFromTool(await _toolService.GetLichSuDonHangAsync(customerId));
            }

            if (input.Contains("mã đơn") || input.Contains("kiểm tra đơn"))
                return "Dạ, AI thông minh đang bảo trì nên mình không tự động kiểm tra mã số được. Bạn hãy bấm vào nút sau để xem nhé:\n[BUTTON: Lịch sử đơn hàng | /tai-khoan/lich-su-don-hang]";

            if (input.Contains("khuyến mãi") || input.Contains("sale") || input.Contains("ưu đãi")) return ExtractMessageFromTool(await _toolService.GetKhuyenMaiAsync());
            if (input.Contains("wifi") || input.Contains("pass") || input.Contains("mật khẩu wifi")) return ExtractMessageFromTool(await _toolService.GetThongTinChungAsync("wifi"));
            if (input.Contains("địa chỉ") || input.Contains("ở đâu")) return ExtractMessageFromTool(await _toolService.GetThongTinChungAsync("diachi"));
            if (input.Contains("mở cửa") || input.Contains("đóng cửa") || input.Contains("mấy giờ")) return ExtractMessageFromTool(await _toolService.GetThongTinChungAsync("giogiac"));

            if (input.StartsWith("kiểm tra: ") || input.StartsWith("kiểm tra:"))
            {
                string identifier = input.Replace("kiểm tra:", "").Trim();
                if (string.IsNullOrEmpty(identifier)) return "Vui lòng nhập SĐT hoặc Email. VD: Kiểm tra: 0987654321";
                return ExtractMessageFromTool(await _toolService.KiemTraKhoaTaiKhoanNhanhAsync(identifier));
            }

            if (input.Contains("khóa") || input.Contains("mở tài khoản") || input.Contains("tại sao bị khóa"))
                return "Dạ, để kiểm tra nick bị khóa, bạn gõ y hệt cú pháp sau nha:\n👉 **Kiểm tra: [SĐT hoặc Tên đăng nhập]**";

            if (input.Contains("mật khẩu") || input.Contains("quên pass") || input.Contains("lấy lại")) return ExtractMessageFromTool(await _toolService.GetHuongDanHeThongAsync("quên mật khẩu"));
            if (input.Contains("góp ý") || input.Contains("đánh giá")) return ExtractMessageFromTool(await _toolService.GetHuongDanHeThongAsync("góp ý"));

            if (input.Contains("nhân viên") || input.Contains("hỗ trợ") || input.Contains("người thật"))
            {
                if (!daDangNhap) return "Dạ, để chat với nhân viên, bạn vui lòng đăng nhập trước nhé.\n[BUTTON: Đăng nhập | /dang-nhap]";
                return "Dạ mình đã ghi nhận yêu cầu. Nhân viên Cafebook sẽ liên hệ lại với bạn ngay ạ! [NEEDS_SUPPORT]";
            }

            string menuChung = "🔹 **Wifi**\n🔹 **Địa chỉ**\n🔹 **Khuyến mãi**\n🔹 **Lấy lại mật khẩu**\n🔹 **Góp ý**\n🔹 **Kiểm tra: [SĐT]** (Tra cứu khóa nick)\n";
            string menuDangNhap = daDangNhap ? "\n*Tính năng cho thành viên:*\n🔹 **Điểm tích lũy**\n🔹 **Lịch sử đặt bàn**\n🔹 **Lịch sử thuê sách**\n🔹 **Lịch sử đơn hàng**\n🔹 **Nhân viên**" : "";

            return "Cafebook chào bạn! Hiện tại hệ thống AI đang bảo trì đột xuất.\n" +
                   "Mình là trợ lý dự phòng, bạn vui lòng gõ đúng các từ khóa sau để tra cứu nhé:\n\n" +
                   menuChung + menuDangNhap;
        }

        private List<object> GetToolDefinitions()
        {
            return new List<object>
            {
                new { 
                    name = "GET_BAN_CHAY", 
                    description = "BẮT BUỘC GỌI KHI khách hỏi về đồ uống ngon, nước bán chạy, sách hot, sách được yêu thích nhất.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "QUERY_DATABASE", 
                    escription = "BẮT BUỘC GỌI KHI khách hỏi thông tin linh hoạt về sản phẩm, tác giả, thể loại, nhà xuất bản, sách mà chưa có tool cụ thể.", 
                    parameters = new { type = "object", properties = new { sqlQuery = new { type = "string" } }, required = new[] { "sqlQuery" } } 
                },
                new { 
                    name = "GET_THONG_TIN_CHUNG", 
                    description = "BẮT BUỘC GỌI KHI khách hỏi: quán ở đâu, địa chỉ quán, giờ mở cửa, pass wifi, số điện thoại. KHÔNG HỎI LẠI.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "GET_KHUYEN_MAI", 
                    description = "BẮT BUỘC GỌI KHI khách hỏi về khuyến mãi. KHÔNG HỎI LẠI.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "HUONG_DAN_HE_THONG", 
                    description = "BẮT BUỘC GỌI KHI khách muốn biết cách: đổi mật khẩu, lấy lại/quên mật khẩu, xem thông tin cá nhân, mua hàng, góp ý, chính sách.", 
                    parameters = new { type = "object", properties = new { chuDe = new { type = "string" } }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "HUONG_DAN_DAT_BAN", 
                    description = "BẮT BUỘC GỌI NGAY KHI khách nhắc đến 'đặt bàn', 'muốn đặt bàn'. TUYỆT ĐỐI KHÔNG HỎI SỐ NGƯỜI, KHÔNG HỎI THỜI GIAN.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "KIEM_TRA_SAN_PHAM", 
                    description = "BẮT BUỘC GỌI KHI khách hỏi CÓ BÁN MÓN NÀY KHÔNG.", 
                    parameters = new { type = "object", properties = new { tenSanPham = new { type = "string" } }, required = new[] { "tenSanPham" } } 
                },
                new { 
                    name = "TIM_MON_THEO_LOAI", 
                    description = "BẮT BUỘC GỌI KHI khách yêu cầu gợi ý loại đồ uống ĐÃ CÓ TÊN CỤ THỂ.", 
                    parameters = new { type = "object", properties = new { loaiMon = new { type = "string" } }, required = new[] { "loaiMon" } } 
                },
                new { 
                    name = "GET_GOI_Y_SAN_PHAM", 
                    description = "BẮT BUỘC GỌI KHI khách KHÔNG NÓI RÕ LOẠI GÌ, chỉ nói gợi ý đồ uống/gợi ý món. TUYỆT ĐỐI KHÔNG HỎI LẠI.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "KIEM_TRA_SACH", 
                    description = "BẮT BUỘC GỌI KHI khách tìm TÊN 1 CUỐN SÁCH CỤ THỂ.", 
                    parameters = new { type = "object", properties = new { tenSach = new { type = "string" } }, required = new[] { "tenSach" } } 
                },
                new { 
                    name = "TIM_SACH_MO_RONG", 
                    description = "BẮT BUỘC GỌI KHI khách tìm sách theo Tác Giả, Thể Loại.", 
                    parameters = new { type = "object", properties = new { loaiTimKiem = new { type = "string", description = "TacGia | TheLoai | NhaXuatBan" }, tuKhoa = new { type = "string" } }, required = new[] { "loaiTimKiem", "tuKhoa" } } 
                },
                new { 
                    name = "GET_GOI_Y_SACH", 
                    description = "Gọi khi khách muốn gợi ý đọc sách nói chung.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "GET_GOI_Y_COMBO", 
                    description = "Gọi NGAY LẬP TỨC khi khách yêu cầu gợi ý CẢ NƯỚC UỐNG VÀ SÁCH cùng lúc.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "GET_DIEM_TICH_LUY", 
                    description = "BẮT BUỘC GỌI KHI khách xem điểm tích lũy.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "GET_THONG_TIN_CA_NHAN", 
                    description = "BẮT BUỘC GỌI KHI khách xem thông tin cá nhân của họ.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "GET_TONG_QUAN_TAI_KHOAN", 
                    description = "BẮT BUỘC GỌI KHI tra cứu tổng quan tài khoản.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "GET_LICH_SU_DAT_BAN", 
                    description = "BẮT BUỘC GỌI KHI xem lịch sử đặt bàn.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "HUY_DAT_BAN", 
                    description = "BẮT BUỘC GỌI KHI khách dùng từ 'hủy bàn'.", 
                    parameters = new { type = "object", properties = new { idPhieuDat = new { type = "integer" }, lyDo = new { type = "string" } }, required = new[] { "idPhieuDat" } } 
                },
                new { 
                    name = "GET_LICH_SU_THUE_SACH", 
                    description = "BẮT BUỘC GỌI KHI xem lịch sử thuê sách.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "GET_LICH_SU_DON_HANG", 
                    description = "BẮT BUỘC GỌI KHI xem lịch sử mua hàng, lịch sử đơn hàng.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "THEO_DOI_DON_HANG", 
                    description = "BẮT BUỘC GỌI KHI khách kiểm tra chi tiết một đơn hàng.", 
                    parameters = new { type = "object", properties = new { idHoaDon = new { type = "integer" } }, required = new[] { "idHoaDon" } } 
                },
                new { 
                    name = "KET_NOI_NHAN_VIEN", 
                    description = "BẮT BUỘC GỌI KHI khách nói: 'nhân viên', 'chat với nhân viên', 'người thật', 'gặp admin'.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
                new { 
                    name = "GET_TRANG_THAI_TAI_KHOAN", 
                    description = "GỌI KHI khách hỏi tại sao tài khoản bị khóa, khi nào tài khoản được mở lại, hoặc kiểm tra tình trạng tài khoản.", 
                    parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() } 
                },
            };
        }
    }
}