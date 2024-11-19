namespace Orchestrator
{
    public class OrchestratorService
    {
        private readonly Events _events;

        public OrchestratorService(Events events)
        {
            _events = events;

            RegisterListener();
        }

        private void RegisterListener()
        {
            _events.MQTTMessageReceived += (m) =>{
                Console.WriteLine($"[Orchestrator] Mensagem do MQTT recebida: {m}");
            };

            _events.RabbitMessageReceived += (m) =>{
                Console.WriteLine($"[Orchestrator] Mensagem do RABBIT recebida: {m}");
            };
        }
    }
}