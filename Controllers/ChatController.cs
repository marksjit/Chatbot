using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace AIChatbot.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly FAQBot _bot;

        public ChatController(FAQBot bot)
        {
            _bot = bot;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Message))
                {
                    return BadRequest(new { error = "Message cannot be empty" });
                }

                // Generate conversationId if not provided
                string conversationId = request.ConversationId ?? Guid.NewGuid().ToString();

                // Get response from bot
                string response = await _bot.GetResponseAsync(request.Message, conversationId);

                return Ok(new ChatResponse
                {
                    Response = response,
                    ConversationId = conversationId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
        public string? ConversationId { get; set; }  
    }

    public class ChatResponse
    {
        public string Response { get; set; }
        public string ConversationId { get; set; }
    }
}