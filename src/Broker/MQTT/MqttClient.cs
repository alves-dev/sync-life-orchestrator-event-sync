using MQTTnet;
using MQTTnet.Client;

namespace Broker.MQTT
{
    public class MqttClient
    {
        private IMqttClient? _mqttClient;
        private MqttClientOptions? _mqttOptions;

        private MqttClientConnectResult? connectResult;

        public MqttClient()
        {
            //Initialize();
        }

        public async Task InitializeAsync()
        {
            string host = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost";
            int port = int.Parse(Environment.GetEnvironmentVariable("MQTT_PORT") ?? "1883");
            string user = Environment.GetEnvironmentVariable("MQTT_USER") ?? string.Empty;
            string password = Environment.GetEnvironmentVariable("MQTT_PASSWORD") ?? string.Empty;

            // Configura o cliente MQTT
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            // Configura as opções de conexão
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(host, port)
                .WithCredentials(user, password)
                .WithCleanSession()
                .Build();

            connectResult = await _mqttClient.ConnectAsync(_mqttOptions);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine($"[MQTT-Client] Connected to MQTT broker {host}:{port} successfully.");
            }
        }

        public IMqttClient GetClient()
        {
            if (_mqttClient == null || connectResult.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new InvalidOperationException("[MQTT-Client] O canal não está disponível ou foi fechado.");
            }
            return _mqttClient;
        }

        public async ValueTask DisposeAsync()
        {
            if (_mqttClient != null)
            {
                await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
                Console.WriteLine("[MQTT-Client] Cliente desconectado e recursos liberados.");
            }
        }
    }
}