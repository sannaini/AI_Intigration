using System.Text.Json;
using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Core.Interfaces.AI;
using LearningAIIntegrations.Core.Models;
using Microsoft.AspNetCore.Mvc;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace LearningAIIntegrations.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MortgageAIController : ControllerBase
    {
        private readonly IMortgageToolsService _mortgageTools;
        private readonly IMortgageToolAIService _mortgageToolAiService;
        private readonly ILogger<MortgageAIController> _logger;

        public MortgageAIController(
            IMortgageToolsService mortgageTools,
            IMortgageToolAIService mortgageToolAiService,
            ILogger<MortgageAIController> logger)
        {
            _mortgageTools = mortgageTools;
            _mortgageToolAiService = mortgageToolAiService;
            _logger = logger;
        }

        // POST /api/mortgageai/ask
        [HttpPost("ask")]
        public async Task<ActionResult<MortgageAIResponse>> Ask(
            [FromBody] MortgageAIRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question cannot be empty.");

            _logger.LogInformation("Question: {Question}", request.Question);

            var aiResponse = await _mortgageToolAiService.AskAsync(request.Question);
        
            return Ok(new MortgageAIResponse
            {
                Answer = aiResponse.Answer,
                ToolCalled = aiResponse.ToolCalled,
                ToolArguments = aiResponse.ToolArguments,
                ToolResult = aiResponse.ToolResult
            });
        }
    }
}