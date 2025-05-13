using MQTTnet;
using MQTTnet.Client;
using System.Text;
using MQTTnet.Protocol;
using Orchestrator;

namespace Broker.MQTT
{
    public class MqttService
    {
        private readonly IMqttClient _mqttClient;
        private readonly Events _events;

        public MqttService(IMqttClient client, Events events)
        {
            _mqttClient = client;
            _events = events;

            StartListem();
        }

        public async Task StartListem()
        {
            string?[] topics = new[]
            {
                Environment.GetEnvironmentVariable("MQTT_TOPIC"),
                Environment.GetEnvironmentVariable("MQTT_TOPIC_V3")
            };

            foreach (var topic in topics)
            {
                await _mqttClient.SubscribeAsync(topic);
                Console.WriteLine($"[MQTT] Inscrito no tópico: {topic}");
            }

            _mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    var topic = e.ApplicationMessage.Topic;
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                    HandleMessage(topic, payload);
                    return Task.CompletedTask;
                };
        }


        public async void PublicMenssage(string topic, string message)
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(message)
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                        .WithRetainFlag()
                        .Build();

            await _mqttClient.PublishAsync(mqttMessage);
        }

        private void HandleMessage(string topic, string message)
        {
            Console.WriteLine($"[MQTT] Processando mensagem do tópico '{topic}': {message}");

            if (topic.Contains("v3"))
            {
                _events.LaunchMQTTMessageReceivedV3(message);
            }
            else
            {
                _events.LaunchMQTTMessageReceived(message);
            }
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