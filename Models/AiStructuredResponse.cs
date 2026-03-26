namespace CodeCheck.Models
{
    public class AiStructuredResponse
    {
        public string Summary { get; set; } = "";
        public List<string> Issues { get; set; } = new();
        public List<string> Improvements { get; set; } = new();
        public string RefactoredCode { get; set; } = "";


    }
}
