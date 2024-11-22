using MQTTnet;
using MQTTnet.Client;
using System.Text;
using MQTTnet.Protocol;
using Orchestrator;

namespace Broker.MQTT
{
    public class MqttClientService
    {
        private IMqttClient _mqttClient;
        private MqttClientOptions? _mqttOptions;

        private Events _events;

        public MqttClientService(IMqttClient client,Events events)
        {
            _mqttClient = client;
            _events = events;

            StartListem();
        }

        public async Task StartListem()
        {
            string topic = Environment.GetEnvironmentVariable("MQTT_TOPIC") ?? "default_topic";

                // Subscribe to a topic
            await _mqttClient.SubscribeAsync(topic);

                // Callback function when a message is received
            _mqttClient.ApplicationMessageReceivedAsync += e =>
                {

                    var topic = e.ApplicationMessage.Topic;
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                    // Lógica para processar mensagens recebidas
                    ProcessMessage(topic, payload);


                    return Task.CompletedTask;
                };
            Console.WriteLine($"[MQTT] Inscrito no tópico: {topic}");
        }

    
        public async void PublicMenssage(string m)
        {
            var message = new MqttApplicationMessageBuilder()
                        .WithTopic("topic_test")
                        .WithPayload(m)
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                        .WithRetainFlag()
                        .Build();

            await _mqttClient.PublishAsync(message);
        }

        private void ProcessMessage(string topic, string message)
        {
            Console.WriteLine($"[MQTT] Processando mensagem do tópico '{topic}': {message}");
            _events.LaunchMQTTMessageReceived(message);
        }

        public async ValueTask DisposeAsync()
        {
            if (_mqttClient != null)
            {
                await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
                Console.WriteLine("[MQTT] Cliente desconectado e recursos liberados.");
            }
        }
    }
}