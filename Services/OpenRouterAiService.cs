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
            using var doc = JsonDocument.Parse(json);

            var content = doc.RootElement.
                GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            content = CleanJson(content!);

            try
            {
                var structured = JsonSerializer.Deserialize<AiStructuredResponse>(content!);
                if (structured != null && structured.Summary.StartsWith("{"))
                {
                    var inner = JsonSerializer.Deserialize<AiStructuredResponse>(structured.Summary);
                    if (inner != null) { return inner; }
                }
                return structured ?? new AiStructuredResponse();

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
            return $@"
                You are a senior software engineer.

                        Analyze the following {language} code.
                        Mode: {mode}
                    
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

    }
}
