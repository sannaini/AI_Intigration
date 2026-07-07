using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Core.Interfaces.AI;
using LearningAIIntegrations.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearningAIIntegrations.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IDocumentVectorService _documentVectorService;
        private readonly IDocumentAiService _documentAIService;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(
            IDocumentService documentService,
            IEmbeddingService embeddingService,
            IDocumentVectorService documentVectorService,
            IDocumentAiService documentAIService,
            ILogger<DocumentController> logger)
        {
            _documentService = documentService;
            _embeddingService = embeddingService;
            _documentVectorService = documentVectorService;
            _documentAIService = documentAIService;
            _logger = logger;
        }

        // ── PHASE 1: Upload + Index ───────────────────────────────
        // POST /api/document/upload
        // Frontend sends multipart/form-data with a .txt file
        [HttpPost("upload")]
        public async Task<ActionResult<DocumentUploadResponse>> Upload(IFormFile file)
        {
            // Basic validations
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .txt files are supported in this module.");

            _logger.LogInformation(
                "Received file '{FileName}' ({Size} bytes)",
                file.FileName, file.Length);

            // 1. Read the file text content on the server
            //    Frontend sends binary file — we read it here
            string text;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                text = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("File is empty.");

            // 2. PHASE 1a — Split text into chunks
            //    e.g. 1 big document → 20 small chunks
            var chunks = _documentService.SplitIntoChunks(text, file.FileName);

            _logger.LogInformation(
                "Split into {Count} chunks", chunks.Count);

            // 3. PHASE 1b — Convert all chunks to vectors (embeddings)
            //    Calls Ollama nomic-embed-text for each chunk
            //    Returns parallel list of float[] vectors
            var chunkTexts = chunks.Select(c => c.Text).ToList();
            var vectors = await _embeddingService.EmbedBatchAsync(chunkTexts);

            _logger.LogInformation(
                "Generated {Count} embeddings", vectors.Count);

            // 4. PHASE 1c — Store chunks + vectors in Qdrant
            //    Each chunk saved with its vector + metadata
            var vectorRecords = chunks.Select(c => new VectorRecord<DocumentChunk>
            {
                
                Id = c.Id, 
                Payload = c,
                Vector = vectors[chunks.IndexOf(c)]
            }).ToList();
            await _documentVectorService.StoreDocumentsAsync(vectorRecords);

            _logger.LogInformation(
                "Stored {Count} chunks in Qdrant", chunks.Count);

            return Ok(new DocumentUploadResponse
            {
                FileName = file.FileName,
                TotalChunks = chunks.Count,
                Message = $"Successfully indexed {chunks.Count} chunks from '{file.FileName}'"
            });
        }

        // ── PHASE 2 + 3: Query ────────────────────────────────────
        // POST /api/document/query
        // Frontend sends { "question": "What is the refund period?" }
        [HttpPost("query")]
        public async Task<ActionResult<DocumentQueryResponse>> Query(
            [FromBody] DocumentQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question cannot be empty.");

            _logger.LogInformation(
                "Received question: '{Question}'", request.Question);

            // 1. PHASE 2a — Convert question to vector
            //    Same model as indexing — MUST use same model!
            //    Different models produce incompatible vector spaces
            var questionVector = await _embeddingService.EmbedAsync(request.Question);

            // 2. PHASE 2b — Search Qdrant for similar chunks
            //    Compares question vector against all stored chunk vectors
            //    Returns top K most similar chunks
            var relevantChunks = await _documentVectorService.SearchDocumentsAsync(questionVector, request.TopChunks);

            if (!relevantChunks.Any())
            {
                return Ok(new DocumentQueryResponse
                {
                    Question = request.Question,
                    Answer = "No relevant content found. Please upload a document first.",
                    RelevantChunks = new List<string>()
                });
            }

            _logger.LogInformation(
                "Found {Count} relevant chunks", relevantChunks.Count);

            // 3. PHASE 3 — Build RAG prompt
            //    Combine retrieved chunks into context
            //    Tell the AI to ONLY use this context to answer
            //    This is called "grounding" — prevents AI from making things up
            var context = string.Join("\n\n", relevantChunks.Select(c => c.Payload.Text));

            var answer = await _documentAIService.GetAnswerAsync(request.Question, relevantChunks.Select(c => c.Payload.Text));
            _logger.LogInformation("Generated answer: '{Answer}'", answer);

            return Ok(new DocumentQueryResponse
            {
                Question = request.Question,
                Answer = answer,
                // Return chunk texts so frontend can show sources
                RelevantChunks = relevantChunks.Select(c => c.Payload.Text).ToList()
            });
        }
    }
}