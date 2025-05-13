using Broker.Rabbit;
using Broker.MQTT;
using System.Text.Json;

namespace Orchestrator
{
    public class OrchestratorService
    {
        private readonly Events _events;
        private readonly RabbitMqService _rabbitService;
        private readonly MqttService _mqttService;

        public OrchestratorService(Events events, RabbitMqService rabbitService, MqttService mqttService)
        {
            _events = events;
            _rabbitService = rabbitService;
            _mqttService = mqttService;

            RegisterListener();
        }

        private void RegisterListener()
        {
            _events.MQTTMessageReceived += (m) =>
            {
                Console.WriteLine($"[Orchestrator] Evento do MQTT recebido");
                _rabbitService.PublicMenssage(getRoutingKeyByMessage(m), m);
            };

            _events.MQTTMessageReceivedV3 += (message) =>
            {
                Console.WriteLine($"[Orchestrator] Evento V3 do MQTT recebido");
                _rabbitService.PublicMenssageV3(new EventBaseV3(message));
            };

            _events.RabbitMessageReceived += (m) =>
            {
                Console.WriteLine($"[Orchestrator] Evento do RABBIT recebido: {m}");
                string type = getType(m);
                if (type == "HEALTH.NUTRI_TRACK.LIQUID_SUMMARY.V1")
                {
                    string entityHealthy = MenssageTransform.GetEntityLiquidSummaryHealthyPayload();
                    _mqttService.PublicMenssage("homeassistant/sensor/health_nutri_track_liquid_summary_healthy_state/config", entityHealthy);

                    string entityUnhealthy = MenssageTransform.GetEntityLiquidSummaryUnhealthyPayload();
                    _mqttService.PublicMenssage("homeassistant/sensor/health_nutri_track_liquid_summary_unhealthy_state/config", entityUnhealthy);

                    string healthyPayload = MenssageTransform.GetLiquidSummaryHealthyPayload(m);
                    string unhealthyPayload = MenssageTransform.GetLiquidSummaryUnhealthyPayload(m);
                    
                    _mqttService.PublicMenssage("health/nutri/track/liquid/summary/healthy/state", healthyPayload);
                    _mqttService.PublicMenssage("health/nutri/track/liquid/summary/unhealthy/state", unhealthyPayload);
                }
                else if (type == "HEALTH.NUTRI_TRACK.LIQUID_ACCEPTABLE.V1")
                {
                    string entityPayload = MenssageTransform.GetEntityLiquidAcceptablePayload();
                    _mqttService.PublicMenssage("homeassistant/sensor/health_nutri_track_liquid_acceptable_state/config", entityPayload);

                    string acceptablePayload = MenssageTransform.GetLiquidSummaryAcceptablePayload(m);
                    _mqttService.PublicMenssage("health/nutri/track/liquid/acceptable/state", acceptablePayload);
                }
                else if (type == "NOTIFICATION.V1")
                {
                    string id = getNotificationId(m);

                    string entityPayload = MenssageTransform.GetEntityNotificationConfigPayload(id);
                    _mqttService.PublicMenssage($"homeassistant/sensor/notification-{id}/message/config", entityPayload);

                    string entityState = MenssageTransform.GetEntityNotificationStatePayload(m);
                    _mqttService.PublicMenssage($"notification/{id}/message/state", entityState);

                    string entityAttributes = MenssageTransform.GetEntityNotificationAttributesPayload(m);
                    _mqttService.PublicMenssage($"notification/{id}/message/attributes", entityAttributes);
                }
                else
                {
                    Console.WriteLine($"Event: {type} not known");
                }
            };
        }

        private string getRoutingKeyByMessage(string message)
        {
            return getType(message) switch
            {
                "HEALTH.NUTRI_TRACK.LIQUID.V1" => "health.nutri-track",
                "HEALTH.NUTRI_TRACK.SOLID.V1" => "health.nutri-track",
                "HEALTH.BODY_TRACK.SLEEP.V1" => "health.body-track",
                _ => "dead.queu"
            };
        }

        private string getType(string message)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(message);
                return jsonDoc.RootElement.GetProperty("type").GetString();
            }
            catch (Exception)
            {
                Console.WriteLine("[Orchestrator] 'type' não encontrado.");
                return "not_type";
            }
        }

        private string getNotificationId(string message)
        {
           try
            {
                var jsonDoc = JsonDocument.Parse(message);
                return jsonDoc.RootElement.GetProperty("notification").GetProperty("id").GetString();
            }
            catch (Exception)
            {
                Console.WriteLine("[Orchestrator] 'notification.id' não encontrado.");
                return "not_notification_id";
            } 
        }
    }
}