using System.Text;
using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Core.Interfaces.AI;
using LearningAIIntegrations.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearningAIIntegrations.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AiController(IChatAiService aiService, IChatHistoryService chatHistoryService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
          var result = await aiService.AskQuestionAsync("who are you");
          return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatHistoryRequest request)
        {
            // 1. Load existing history for this session
            var history = chatHistoryService.GetHistory(request.SessionId);

            // 2. Call Gemini with full history + new message
            var aiResponse = await aiService.ChatWithHistoryAsync(history, request.Message);

            // 3. Save BOTH the user message and AI reply to history
            //    This grows the history for next time
            chatHistoryService.AddMessage(request.SessionId, "user", request.Message);
            chatHistoryService.AddMessage(request.SessionId, "model", aiResponse.Reply);

            // 4. Return the reply + useful metadata
            return Ok(new ChatHistoryResponse
            {
                SessionId = request.SessionId,
                Reply = aiResponse.Reply,
                InputTokens = aiResponse.InputTokens,
                OutputTokens = aiResponse.OutputTokens,
                TotalMessagesInHistory = chatHistoryService.GetHistory(request.SessionId).Count
            });

        }

        [HttpGet("stream")]
        public async Task GetStreamAsync(CancellationToken cancellationToken)
        {
            // 1. Tell the browser: "this is a stream, keep the connection open"
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("X-Accel-Buffering", "no"); // disables proxy buffering

            await foreach (var chunk in aiService.AskQuestionAsync("give me any kids story with 10 sentences", cancellationToken))
            {
                // 3. Format as SSE and write immediately to the response
                //    SSE format: "data: YOUR_TEXT\n\n"
                //    The double newline \n\n signals end of one event to the browser
                var sseMessage = $"data: {chunk}\n\n";
                await Response.WriteAsync(sseMessage, Encoding.UTF8);

                // 4. Flush = actually SEND this chunk to the client right now
                //    Without flush, .NET buffers it and sends everything at once
                //    (which defeats the whole purpose of streaming!)
                await Response.Body.FlushAsync();
            }

            // 5. Send a [DONE] signal so the client knows the stream is finished
            await Response.WriteAsync("data: [DONE]\n\n", Encoding.UTF8);
            await Response.Body.FlushAsync();
        }
    }
}