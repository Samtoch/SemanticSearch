using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace SemanticSearch.Services
{
    public class SearchService : ISearchService
    {
        public async Task<float[]> SemanticSearch(string inputText)
        {
            string model = "all-minilm"; // Ensure this model exists in Ollama
            List<string> blogPostTitles = new List<string>
            {
                "AI is transforming the world.",
                "Machine learning enables computers to learn from data.",
                "Quantum computing is the future of technology.",
                "Deep learning is a subset of machine learning."
            };

            // Generate embeddings for each blog post title
            Console.WriteLine("Generating embeddings for blog post titles...");
            var candidateEmbeddings = await GenerateEmbeddings(blogPostTitles, model);
            Console.WriteLine("Embeddings generated successfully.");
            Console.WriteLine("\n");

            Console.WriteLine("Time of Generation: " + DateTime.Now );

            int cnt = 0;
            foreach (var kvp in candidateEmbeddings)
            {
                cnt++;
                Console.WriteLine("Embeding Key: " + kvp.Key);
                Console.WriteLine("Embeding Value: " + string.Join(", ", kvp.Value));
                Console.WriteLine("Embeding string length: " + kvp.Value.Length);
                Console.WriteLine("\n");
            }

            Console.WriteLine("TOTAL TOKEN: " + cnt);

            // User input query
            string userQuery = "Tell me about AI and machine learning.";
            float[] userEmbedding = await GetOllamaEmbedding(userQuery, model);

            Console.WriteLine("\n");
            Console.WriteLine("Below is the Content of the Embeddings Query\n" + string.Join(", ", userEmbedding));
            Console.WriteLine("TOTAL TOKEN2: " + userEmbedding.Length);

            // Compute cosine similarities and get the top three matches.
            var topMatches = candidateEmbeddings
                .Select(candidate => new
                {
                    Text = candidate.Key,
                    Similarity = CosineSimilarity(candidate.Value, userEmbedding)
                })
                .OrderByDescending(match => match.Similarity)
                .Take(3);

            Console.WriteLine("\n");

            // Display results
            Console.WriteLine("\nTop matching blog post titles:");
            foreach (var match in topMatches)
            {
                Console.WriteLine($"Similarity: {match.Similarity:F4} - {match.Text}");
            }

            return userEmbedding;
        }

        // Function to compute Cosine Similarity
        static float CosineSimilarity(float[] vecA, float[] vecB)
        {
            float dotProduct = vecA.Zip(vecB, (a, b) => a * b).Sum();
            float magnitudeA = (float)Math.Sqrt(vecA.Sum(a => a * a));
            float magnitudeB = (float)Math.Sqrt(vecB.Sum(b => b * b));
            return dotProduct / (magnitudeA * magnitudeB);
        }

        // Function to generate embeddings for multiple texts
        static async Task<Dictionary<string, float[]>> GenerateEmbeddings(List<string> texts, string model)
        {
            Dictionary<string, float[]> embeddings = new Dictionary<string, float[]>();

            foreach (var text in texts)
            {
                float[] embedding = await GetOllamaEmbedding(text, model);
                if (embedding != null)
                {
                    embeddings[text] = embedding;
                }
            }
            return embeddings;
        }

        // Function to generate embeddings for a single text using Ollama API
        static async Task<float[]> GetOllamaEmbedding(string text, string model)
        {
            using HttpClient client = new HttpClient();
            var requestBody = new
            {
                model = model,
                prompt = text,
                options = new { }
            };

            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:11434/api/embeddings", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseJson);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("embedding", out JsonElement embeddingElement))
                {
                    return JsonSerializer.Deserialize<float[]>(embeddingElement.GetRawText());
                }
            }

            Console.WriteLine("Failed to get embeddings.");
            return null;
        }

    }
}
