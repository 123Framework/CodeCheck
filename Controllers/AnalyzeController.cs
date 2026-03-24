using CodeCheck.Models;
using CodeCheck.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodeCheck.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyzeController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AnalyzeController(IAIService aiService)
        {
            _aiService = aiService;
        }
        [HttpPost] public async Task<ActionResult<AnalyzeResponse>> Analyze([FromBody] AnalyzeRequest request)
        {
            var result = await _aiService.AnalyzeAsync(

                request.Code,
                request.Language,
                request.Mode);
            return Ok(new AnalyzeResponse
            {
                Result = result,
            });
        }
    }
}
