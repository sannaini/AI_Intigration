using System.Text;
using System.Text.Json;
using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Core.Interfaces.AI;
using LearningAIIntegrations.Core.Models;
using LearningAIIntegrations.Shared.Extensions;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace LearningAIIntegrations.Infrastructure.Services.AI
{
    public class MortgageToolAIService : IMortgageToolAIService
    {
        private readonly IMortgageToolsService _mortgageTools;
        private readonly IOllamaApiClient _ollamaClient;
        private readonly ILogger<MortgageToolAIService> _logger;

        public MortgageToolAIService(
            IMortgageToolsService mortgageTools,
            IOllamaApiClient ollamaClient,
            ILogger<MortgageToolAIService> logger)
        {
            _mortgageTools = mortgageTools;
            _ollamaClient = ollamaClient;
            _logger = logger;
        }

        public async Task<MortgageAIResponse> AskAsync(string question)
        {
            _logger.LogInformation("Mortgage AI Question: {Question}", question);

            var tools = BuildTools();

            var messages = new List<Message>
            {
                new Message
                {
                    Role = ChatRole.System,
                    Content =
                        "You are a mortgage advisor assistant. " +
                        "Always use tools for calculations. Never guess numbers."
                },
                new Message
                {
                    Role = ChatRole.User,
                    Content = question
                }
            };

            var request = new ChatRequest
            {
                Messages = messages,
                Tools = tools,
                Stream = false
            };

            var firstResponse = await GetFullResponse(request);

            var toolCalls = firstResponse.Message?.ToolCalls?.ToList();

            if (toolCalls == null || !toolCalls.Any())
            {
                return new MortgageAIResponse
                {
                    Answer = firstResponse.Message?.Content ?? "No response"
                };
            }
            var toolNames = new List<string>();
            var toolArguments = new List<IDictionary<string, object>>();
            var toolResults = new List<string>();

            var secondMessages = new List<Message>(messages)
            {
                firstResponse.Message!
            };

            foreach (var toolCall in toolCalls)
            {
                var toolName = toolCall.Function.Name;

                toolNames.Add(toolName);

                var rawArgs = toolCall.Function.Arguments;

                toolArguments.Add(rawArgs);

                var normalizedArgs =
                    ToolArgumentNormalizer.Normalize(rawArgs);

                var result = ExecuteTool(
                    toolName,
                    normalizedArgs);

                toolResults.Add(result);
                secondMessages.Add(new Message
                {
                    Role = ChatRole.Tool,
                    ToolName = toolName,
                    Content = result
                });
            }

            var secondRequest = new ChatRequest
            {
                Messages = secondMessages,
                Stream = false
            };


            var finalResponse = await GetFullResponse(secondRequest);


            return new MortgageAIResponse
            {
                Answer = finalResponse.Message?.Content ?? "No response",

                ToolCalled = string.Join(",", toolNames),

                ToolArguments = toolArguments
                    .Select(x => x.ToDictionary(
                        k => k.Key,
                        v => (object)v.Value))
                    .ToList(),
            };
        }

        #region Tool Definitions

        private List<Tool> BuildTools()
        {
            return new List<Tool>
            {
                CreateMortgagePaymentTool(),
                CreateAffordabilityTool(),
                CreateLoanTypesTool()
            };
        }

        private Tool CreateMortgagePaymentTool()
        {
            return new Tool
            {
                Function = new Function
                {
                    Name = "CalculateMortgagePayment",
                    Description = "Calculates monthly mortgage payment and total cost.",
                    Parameters = new Parameters
                    {
                        Properties = new Dictionary<string, Property>
                        {
                            ["loanAmount"] = new Property { Type = "number" },
                            ["annualRate"] = new Property { Type = "number" },
                            ["termYears"] = new Property { Type = "number" }
                        },
                        Required = new List<string>
                        {
                            "loanAmount", "annualRate", "termYears"
                        }
                    }
                }
            };
        }

        private Tool CreateAffordabilityTool()
        {
            return new Tool
            {
                Function = new Function
                {
                    Name = "CalculateAffordability",
                    Description = "Calculates home affordability based on income and debts.",
                    Parameters = new Parameters
                    {
                        Properties = new Dictionary<string, Property>
                        {
                            ["annualIncome"] = new Property { Type = "number" },
                            ["monthlyDebts"] = new Property { Type = "number" },
                            ["downPayment"] = new Property { Type = "number" }
                        },
                        Required = new List<string>
                        {
                            "annualIncome", "monthlyDebts", "downPayment"
                        }
                    }
                }
            };
        }

        private Tool CreateLoanTypesTool()
        {
            return new Tool
            {
                Function = new Function
                {
                    Name = "GetQualifiedLoanTypes",
                    Description = "Returns eligible loan types based on credit score.",
                    Parameters = new Parameters
                    {
                        Properties = new Dictionary<string, Property>
                        {
                            ["creditScore"] = new Property { Type = "number" },
                            ["downPaymentPercent"] = new Property { Type = "number" }
                        },
                        Required = new List<string>
                        {
                            "creditScore", "downPaymentPercent"
                        }
                    }
                }
            };
        }

        #endregion

        #region Tool Execution

        private string ExecuteTool(string toolName, IDictionary<string, object>? args)
        {
            if (args == null)
                throw new ArgumentException("Tool arguments missing");

            return toolName switch
            {
                "CalculateMortgagePayment" =>
                    JsonSerializer.Serialize(
                        _mortgageTools.CalculateMortgagePayment(
                            args.GetDecimal("loanAmount"),
                            args.GetDouble("annualRate"),
                            args.GetInt("termYears"))),

                "CalculateAffordability" =>
                    JsonSerializer.Serialize(
                        _mortgageTools.CalculateAffordability(
                            args.GetDecimal("annualIncome"),
                            args.GetDecimal("monthlyDebts"),
                            args.GetDecimal("downPayment"))),

                "GetQualifiedLoanTypes" =>
                    JsonSerializer.Serialize(
                        _mortgageTools.GetQualifiedLoanTypes(
                            args.GetInt("creditScore"),
                            args.GetDouble("downPaymentPercent"))),

                _ => throw new ArgumentException($"Unknown tool: {toolName}")
            };
        }

        #endregion

        #region Ollama Helper

        private async Task<ChatResponseStream> GetFullResponse(ChatRequest request)
        {
            ChatResponseStream? last = null;

            await foreach (var chunk in _ollamaClient.ChatAsync(request))
            {
                last = chunk;
            }

            return last ?? throw new InvalidOperationException("No response from Ollama");
        }



        public static class ToolArgumentNormalizer
        {
            public static IDictionary<string, object> Normalize(
                IDictionary<string, object> args)
            {
                return args.ToDictionary(
                    x => x.Key,
                    x => NormalizeValue(x.Value));
            }

            private static object NormalizeValue(object value)
            {
                if (value is not JsonElement element)
                    return value;

                return element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString()!,

                    JsonValueKind.Number =>
                        element.TryGetInt32(out var i)
                            ? i
                            : element.GetDouble(),

                    JsonValueKind.True => true,

                    JsonValueKind.False => false,

                    JsonValueKind.Null => null!,

                    _ => element.ToString()
                };
            }
        }

        #endregion
}
}
