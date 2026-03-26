using CodeCheck.Models;

namespace CodeCheck.Services
{
    public interface IAIService
    {
        Task<AiStructuredResponse> AnalyzeAsync(string code, string language, string mode);


    }
}
