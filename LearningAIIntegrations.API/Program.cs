using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Core.Interfaces.AI;
using LearningAIIntegrations.Core.Models;
using LearningAIIntegrations.Infrastructure.Services;
using LearningAIIntegrations.Infrastructure.Services.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);



// Add this 👇
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings")
);



// ═══════════════════════════════════════════════
//  3. OLLAMA (Module 4 — chat + embeddings)
// ═══════════════════════════════════════════════

// Ollama runs locally — no API key needed!
var ollamaUri = new Uri(
    builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434");
var ollamaModel = builder.Configuration["Ollama:Model"] ?? "llama3.2";

// ── OllamaApiClient ───────────────────────────────────────────
// Used in DocumentController for chat (llama3.2)
// IOllamaApiClient is the interface OllamaSharp provides
builder.Services.AddSingleton<IOllamaApiClient>(
    new OllamaApiClient(ollamaUri,ollamaModel));

// ── IEmbeddingGenerator ───────────────────────────────────────
// Used in EmbeddingService to convert text → vectors
// We cast OllamaApiClient to IEmbeddingGenerator
// and set the embedding model to nomic-embed-text
//
// WHY cast? OllamaApiClient implements BOTH:
//   → IOllamaApiClient (for chat)
//   → IEmbeddingGenerator<string, Embedding<float>> (for embeddings)
// We register them separately so each service gets what it needs
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var client = new OllamaApiClient(ollamaUri)
    {
        // Tell Ollama which model to use for embeddings
        SelectedModel = "nomic-embed-text"
    };
    return (IEmbeddingGenerator<string, Embedding<float>>)client;
});

// ═══════════════════════════════════════════════
//  4. QDRANT (Module 4 — vector storage)
// ═══════════════════════════════════════════════

// Qdrant runs in Docker on port 6334 (gRPC — faster than REST)
// Note: 6334 is gRPC port, 6333 is REST/dashboard port
// QdrantClient uses gRPC internally for performance
var qdrantHost = builder.Configuration["Qdrant:Host"] ?? "localhost";
var qdrantPort = int.Parse(builder.Configuration["Qdrant:Port"] ?? "6334");

builder.Services.AddSingleton(
    new QdrantClient(qdrantHost, qdrantPort));


// ═══════════════════════════════════════════════
//  5. MODULE 4 SERVICES
// ═══════════════════════════════════════════════

builder.Services.AddScoped<IDocumentVectorService, DocumentVectorService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IVectorStoreService, VectorStoreService>();


builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IChatAiService, OllamaChatService>();
builder.Services.AddSingleton<IDocumentAiService, DocumentAIService>();
builder.Services.AddSingleton<IMortgageToolAIService, MortgageToolAIService>();
builder.Services.AddSingleton<IMortgageToolsService, MortgageToolsService>();
builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>();

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // Serves the generated OpenAPI spec as a JSON endpoint
    app.UseSwaggerUI(); // Serves the interactive web UI
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
