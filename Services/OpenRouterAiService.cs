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

        public async Task<string> AnalyzeAsync(string code, string language, string mode)
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
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var result = doc.RootElement.
                GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            return result ?? "No response";

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

                    Return ONLY valid JSON in this format:
                    {{
                    ""summary"": ""short explanation"",
                    ""issues"": [""issue1"", ""issue2""],
                    ""improvements"": [""improvement1"", ""improvement2""],
                    ""refactoredCode"": ""improved version of code""
                    }}

                    Code:
           {code}
            ";
        }

    }
}
