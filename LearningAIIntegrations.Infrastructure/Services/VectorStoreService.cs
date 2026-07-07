using System.Drawing;
using System.Text.Json;
using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Core.Models;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace LearningAIIntegrations.Infrastructure.Services
{
    // ── Stores and searches vectors in Qdrant ─────────────────────
    public class VectorStoreService : IVectorStoreService
    {
        private readonly QdrantClient _client;
        private readonly ILogger<VectorStoreService> _logger;


        // Must match nomic-embed-text output dimensions exactly
        private const int VectorSize = 768;

        public VectorStoreService(QdrantClient client, ILogger<VectorStoreService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task CreateCollectionIfNotExistsAsync(
            string collectionName,
            int vectorSize = VectorSize)
        {
            // Check if collection already exists
            var collections = await _client.ListCollectionsAsync();
            var exists = collections.Any(c => c == collectionName);

            if (exists)
            {
                _logger.LogInformation(
                    "Collection '{Name}' already exists — skipping creation",
                    collectionName);
                return;
            }

            // Create the collection with cosine similarity
            // Cosine = measures angle between vectors (best for text similarity)
            // Other options: Dot product, Euclidean distance
            await _client.CreateCollectionAsync(
                collectionName: collectionName,
                vectorsConfig: new VectorParams
                {
                    Size = (ulong)vectorSize,
                    Distance = Distance.Cosine
                }
            );

            _logger.LogInformation(
                "Created Qdrant collection '{Name}' " +
                "with vector size {Size} and Cosine distance",
                collectionName, vectorSize);
        }



        // ── Store Chunks ──────────────────────────────────────────
        // Saves document chunks + their vectors into Qdrant
        // chunks[i] pairs with vectors[i] — parallel lists
        public async Task StoreChunksAsync<TPayload>(string collectionName,
            List<VectorRecord<TPayload>> records)
        {
            // Make sure collection exists before storing
            await CreateCollectionIfNotExistsAsync(collectionName);


            // Build Qdrant "points" — each point = one chunk + its vector
            var points = records.Select((record, index) =>
            {
                var point = new PointStruct
                {
                    Id = new PointId { Uuid = record.Id.ToString() },
                    Vectors = record.Vector
                };

                // Automatically convert any generic class object directly to Qdrant Payload
                var json = JsonSerializer.Serialize(record.Payload);
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                foreach (var kvp in dict!)
                {
                    switch (kvp.Value.ValueKind)
                    {
                        case JsonValueKind.String:
                            point.Payload[kvp.Key] = kvp.Value.GetString()!;
                            break;

                        case JsonValueKind.Number:
                            if (kvp.Value.TryGetInt32(out var i))
                                point.Payload[kvp.Key] = i;
                            else if (kvp.Value.TryGetInt64(out var l))
                                point.Payload[kvp.Key] = l;
                            else
                                point.Payload[kvp.Key] = kvp.Value.GetDouble();
                            break;

                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            point.Payload[kvp.Key] = kvp.Value.GetBoolean();
                            break;
                    }
                }

                return point;
            }).ToList();

            // Upsert = insert OR update
            // Safe to call multiple times with same IDs
            await _client.UpsertAsync(collectionName, points);

            _logger.LogInformation(
                "Stored {Count} chunks in Qdrant collection '{Name}'",
                points.Count, collectionName);
        }

        // ── Search ────────────────────────────────────────────────
        // Finds the most similar chunks to the question vector
        // Returns top K chunks sorted by similarity score
        public async Task<List<VectorRecord<TPayload>>> SearchAsync<TPayload>(string collectionName, float[] questionVector, int topK)
        {

            var results = await _client.QueryAsync(
                collectionName: collectionName,
                query: questionVector,
                limit: (ulong)topK,
                payloadSelector: true
            );



            return results.Select(result =>
            {
                // Reconstruct the generic object structure from Qdrant values safely
                var dict = result.Payload.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.KindCase switch
                    {
                        Value.KindOneofCase.StringValue => (object)kvp.Value.StringValue,
                        Value.KindOneofCase.IntegerValue => kvp.Value.IntegerValue,
                        Value.KindOneofCase.DoubleValue => kvp.Value.DoubleValue,
                        Value.KindOneofCase.BoolValue => kvp.Value.BoolValue,
                        Value.KindOneofCase.NullValue => null!,
                        _ => kvp.Value.ToString()
                    });

                var json = JsonSerializer.Serialize(dict);
                var payload = JsonSerializer.Deserialize<TPayload>(json);

                return new VectorRecord<TPayload>
                {
                    Id = Guid.Parse(result.Id.Uuid),
                    Payload = payload
                };
            }).ToList();
        }
    }
}