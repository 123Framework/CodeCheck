namespace CodeCheck.Models
{
    public class AnalyzeRequest
    {
        public string Code { get; set; } = "";
        public string Language { get; set; } = "csharp";
        public string Mode { get; set; } = "";


    }
}
