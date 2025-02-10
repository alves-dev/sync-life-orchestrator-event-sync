using System.Diagnostics.Tracing;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Orchestrator
{
    public class Device
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("identifiers")]
        public string[] Identifiers { get; set; }
    }

    public class MenssageTransform
    {
        public static String GetLiquidSummaryHealthyPayload(string eventRabbit)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(eventRabbit);
                int healthy = jsonDoc.RootElement.GetProperty("total_liquid").GetProperty("healthy").GetInt16();
                return healthy.ToString();
            }
            catch (Exception)
            {
                Console.WriteLine("[MenssageTransform] 'healthy' não encontrado.");
                return "-1";
            }
        }

        public static String GetLiquidSummaryUnhealthyPayload(string eventRabbit)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(eventRabbit);
                int unhealthy = jsonDoc.RootElement.GetProperty("total_liquid").GetProperty("unhealthy").GetInt16();
                return unhealthy.ToString();
            }
            catch (Exception)
            {
                Console.WriteLine("[MenssageTransform] 'unhealthy' não encontrado.");
                return "-1";
            }
        }

        public static String GetLiquidSummaryAcceptablePayload(string eventRabbit)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(eventRabbit);
                JsonElement liquidsArray = jsonDoc.RootElement.GetProperty("accepted_liquids");
                string acceptedLiquids = string.Join(",", liquidsArray.EnumerateArray().Select(liquid => liquid.GetString()));
                return acceptedLiquids;
            }
            catch (Exception)
            {
                Console.WriteLine("[MenssageTransform] 'accepted_liquids' não encontrado.");
                return "-1";
            }
        }

        public static String? GetEntityLiquidSummaryHealthyPayload()
        {

            var json = new Dictionary<string, object>
            {
                { "name", "Liquid Summary Healthy" },
                { "object_id", "health.nutri.track.liquid.summary.healthy.state" },
                { "unique_id", "health.nutri.track.liquid.summary.healthy.state" },
                { "device_class", "volume"},
                { "unit_of_measurement", "mL"},
                { "state_topic", "health/nutri/track/liquid/summary/healthy/state"},
                { "device", GetHealthDevice() }
            };

            try
            {
                return JsonSerializer.Serialize(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Deu ruim em GetEntityLiquidSummaryHealthyPayload!");
                Console.WriteLine(e);
            }
            return null;
        }

        public static String? GetEntityLiquidSummaryUnhealthyPayload()
        {

            var json = new Dictionary<string, object>
            {
                { "name", "Liquid Summary Unhealthy" },
                { "object_id", "health.nutri.track.liquid.summary.unhealthy.state" },
                { "unique_id", "health.nutri.track.liquid.summary.unhealthy.state" },
                { "device_class", "volume"},
                { "unit_of_measurement", "mL"},
                { "state_topic", "health/nutri/track/liquid/summary/unhealthy/state"},
                { "device", GetHealthDevice() }
            };

            try
            {
                return JsonSerializer.Serialize(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Deu ruim em GetEntityLiquidSummaryUnhealthyPayload!");
                Console.WriteLine(e);
            }
            return null;
        }

        public static String? GetEntityLiquidAcceptablePayload()
        {

            var json = new Dictionary<string, object>
            {
                { "name", "Liquid Acceptable" },
                { "object_id", "health.nutri.track.liquid.acceptable.state" },
                { "unique_id", "health.nutri.track.liquid.acceptable.state" },
                { "state_topic", "health/nutri/track/liquid/acceptable/state"},
                { "device", GetHealthDevice() }
            };

            try
            {
                return JsonSerializer.Serialize(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Deu ruim em GetEntityLiquidAcceptablePayload!");
                Console.WriteLine(e);
            }
            return null;
        }

        public static String? GetEntityNotificationConfigPayload(string id)
        {

            var json = new Dictionary<string, object>
            {
                { "name", $"Message: {id}" },
                { "icon", "mdi:bell-circle" },
                { "unique_id", $"notification.{id}.message" },
                { "state_topic", $"notification/{id}/message/state"},
                { "json_attributes_topic", $"notification/{id}/message/attributes"},
                { "device", GetNotificationDevice() }
            };

            try
            {
                return JsonSerializer.Serialize(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Deu ruim em GetEntityNotificationConfigPayload!");
                Console.WriteLine(e);
            }
            return null;
        }

        public static String GetEntityNotificationStatePayload(string eventRabbit)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(eventRabbit);
                return jsonDoc.RootElement.GetProperty("notification").GetProperty("message").GetString();
            }
            catch (Exception)
            {
                Console.WriteLine("[MenssageTransform] 'notification.message' não encontrado.");
                return "-1";
            }
        }

        public static String GetEntityNotificationAttributesPayload(string eventRabbit)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(eventRabbit);
                return jsonDoc.RootElement.GetProperty("notification").ToString();
            }
            catch (Exception)
            {
                Console.WriteLine("[MenssageTransform] 'notification' não encontrado.");
                return "-1";
            }
        }

        private static Device GetHealthDevice()
        {
            var device = new Device
            {
                Name = "Health Data",
                Manufacturer = "Health",
                Model = "Health V1",
                Identifiers = new[] { "health_data" }
            };

            return device;
        }

        private static Device GetNotificationDevice()
        {
            var device = new Device
            {
                Name = "Notification",
                Manufacturer = "Event Sync",
                Model = "Notification V1",
                Identifiers = new[] { "event_sync_notification_message_v1" }
            };

            return device;
        }
    }
}