using Broker.Rabbit;
using System.Text.Json;

namespace Orchestrator
{
    public class OrchestratorService
    {
        private readonly Events _events;
        private readonly RabbitMqService _rabbitService;

        public OrchestratorService(Events events, RabbitMqService rabbitService)
        {
            _events = events;
            _rabbitService = rabbitService;

            RegisterListener();
        }

        private void RegisterListener()
        {
            _events.MQTTMessageReceived += (m) =>
            {
                Console.WriteLine($"[Orchestrator] Mensagem do MQTT recebida: {m}");
                _rabbitService.PublicMenssage(getRoutingKeyByMessage(m), m);
            };

            _events.RabbitMessageReceived += (m) =>
            {
                Console.WriteLine($"[Orchestrator] Mensagem do RABBIT recebida: {m}");
            };
        }

        private string getRoutingKeyByMessage(string message)
        {
            return getType(message) switch
            {
                "HEALTH.NUTRI_TRACK.LIQUID.V1" => "health.nutri-track",
                _ => "dead.queu" //TODO: criar fila morta
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
    }
}