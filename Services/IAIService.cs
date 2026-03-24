namespace CodeCheck.Services
{
    public interface IAIService
    {
        Task<string> AnalyzeAsync(string code, string language, string mode);

    }
}
