namespace CodeCheck.Services
{
    public class FakeService : IAIService
    {
        public Task<string> AnalyzeAsync(string code, string language, string mode)
        {
            var result = $"[FAKE AI]\nMode:{mode}\nLanguage: {language}\n\nCode length:{code.Length}";
            return Task.FromResult(result);
        }
    }
}
