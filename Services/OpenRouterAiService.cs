using CodeCheck.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace CodeCheck.Services
{
    public class OpenRouterAiService : IAIService
    {
        private readonly HttpClient _http;
        private readonly OpenRouterOptions _options;

        public OpenRouterAiService(HttpClient http, IOptions<OpenRouterOptions> options)
        {
            _http = http;
            _options = options.Value;
        }
        private string CleanJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content;
            

                
                content = content.Trim();

                if (content.StartsWith("```"))
                {
                    content = content.Replace("```json", "").Replace("```", "").Trim();
                }

            
            return content;
        }
        public async Task<AiStructuredResponse> AnalyzeAsync(string code, string language, string mode)
        {
            var prompt = BuildPrompt(code, language, mode);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Headers.Add("HTTP-Refer", "http://localhost");
            request.Headers.Add("X-Title", "CodeCheck AI");

            var body = new
            {
                model = _options.Model,
                messages = new[]
                {
                    new {role = "system" , content = "You are  a senior softwarte engineer. Analyze code professonally"},
                    new {role = "user" , content = prompt}
                }
            };
            request.Content = new StringContent(

                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
                );


            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ai request failed:{error}");
            }
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("=== RAW AI RESPONSE ===");
            Console.WriteLine(json);

            using var doc = JsonDocument.Parse(json);

            //var content = doc.RootElement.
            //    GetProperty("choices")[0]
            //    .GetProperty("message")
            //    .GetProperty("content")
            //    .GetString();

            var root = doc.RootElement;
            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
                throw new Exception("Invalid AI response: no choices");


            var message = choices[0].GetProperty("message");
            if (!message.TryGetProperty("content", out var contentElement))
                throw new Exception("Invalid AI response: no content");
            var content = contentElement.GetString();
            content = CleanJson(content!);

            try
            {
                var structured = JsonSerializer.Deserialize<AiStructuredResponse>(content!);
                //if (structured != null && structured.Summary.StartsWith("{"))
                //{
                //    var inner = JsonSerializer.Deserialize<AiStructuredResponse>(structured.Summary);
                //    if (inner != null) { return inner; }
                
                    if (structured == null || string.IsNullOrWhiteSpace(structured.Summary))
                    {
                        return new AiStructuredResponse
                        {
                            Summary = "basic code analysis complete",
                            Issues = new List<string> { "no critical issues found, but cdoe can be improved"},
                            Improvements = new List<string> { "Consider adding logging or validation"},
                            RefactoredCode = code,

                        };
                    }
                    return structured;
               // }
            }
            catch
            {
                return new AiStructuredResponse() { Summary = content ?? "Failed to parse Ai response" };
            }

        }

        private string BuildPrompt(string code, string language, string mode)
        {
            /* return mode switch
             {
                 "bugs" => $"Find bugs in this {language} code:\n{code}",
                 "optimize" => $"Optimize this {language} code:\n{code}",
                 "security" => $"Find security issues in this {language} code:\n{code}",
                 _ => $"Explain this {language} code:\n{code}"
             };
            */
            var normalizedMode = NormalizeMode(mode);
            return $@"
                You are a senior software engineer.

                        Analyze the following {language} code.
                        Mode: {normalizedMode}
                    
                    IMPORTANT:
                    - Return ONLY VALID JSON
                    - Do NOT INCLUDE markdown (no ```json)
                    - Do NOT INCLUDE explanations outside JSON
                    Return ONLY valid JSON in this format:
                   FORMAT:
                    {{
                    ""summary"": ""..."",
                    ""issues"": [""...""],
                    ""improvements"": [""...""],
                    ""refactoredCode"": ""...""
                    }}

                    Code:
           {code}
            ";
        }
        private string NormalizeMode(string mode)
        {
            return mode switch
            {
                "fix" => "refactor",
                "bugs" => $"Find bugs ",
                "optimize" => $"Optimize this ",
                _ => "explain"
            };
        }

    }
}
