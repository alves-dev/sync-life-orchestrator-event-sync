using System.Text.Json.Serialization;
using System.Text.Json;

namespace Orchestrator
{
    public class EventBaseV3
    {
        [JsonPropertyName("specversion")]
        public string SpecVersion { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        [JsonPropertyName("datacontenttype")]
        public string DataContentType { get; set; }

        [JsonPropertyName("dataschema")]
        public string DataSchema { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }

        [JsonPropertyName("extensions")]
        public JsonElement Extensions { get; set; }

        public EventBaseV3(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeProp))
                throw new ArgumentException("JSON precisa conter o campo 'type'");

            // Copia o restante dos campos sem o 'type'
            using var outputDoc = JsonDocument.Parse(RemoveProperty(json, "type"));

            SpecVersion = "1.0";
            Type = typeProp.GetString();
            Source = "services/home-assistant";
            Id = Guid.NewGuid().ToString();
            Time = new DateTimeOffset(DateTime.UtcNow).ToOffset(TimeSpan.FromHours(-3));
            DataContentType = "application/json";
            DataSchema = "https://github.com/alves-dev/SyncLife/tree/main/evens/schema/health/schema_v1.json";
            Data = outputDoc.RootElement.Clone();
            Extensions = GetExtensions();
        }

        private static string RemoveProperty(string json, string propToRemove)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var dict = new Dictionary<string, JsonElement>();

            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name != propToRemove)
                    dict[prop.Name] = prop.Value;
            }

            return JsonSerializer.Serialize(dict);
        }

        private JsonElement GetExtensions()
        {
            var dict = new Dictionary<string, object>
                {
                    { "created_by", "event-sync" },
                    { "origin", "home-assistant" }
                };

            // Serializa o dicionário e converte pra JsonElement
            string json = JsonSerializer.Serialize(dict);
            var jsonDoc = JsonDocument.Parse(json);
            return jsonDoc.RootElement;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
    }
}