using Microsoft.AspNetCore.Mvc;
using backend.Services;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly GroqService _groqService;

        public ChatbotController(GroqService groqService)
        {
            _groqService = groqService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest(new { error = "Message is required" });
            }

            try
            {
                var systemContext = await _groqService.GetDatabaseContextAsync();
                var response = await _groqService.ChatCompletionAsync(request.Message, systemContext);
                
                return Ok(new { message = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("context")]
        public async Task<IActionResult> GetContext()
        {
            try
            {
                var context = await _groqService.GetDatabaseContextAsync();
                return Ok(new { context });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
