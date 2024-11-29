using RabbitMQ.Client;

namespace Broker.Rabbit
{
    public class RabbitMqClient
    {
        private IConnection? _connection;
        private IChannel? _channel;

        public async Task InitializeAsync()
        {
            string host = Environment.GetEnvironmentVariable("RABBIT_HOST") ?? "localhost";
            string user = Environment.GetEnvironmentVariable("RABBIT_USER") ?? "guest";
            string password = Environment.GetEnvironmentVariable("RABBIT_PASSWORD") ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = password
            };

            // Estabelece a conexão de forma assíncrona
            _connection = await factory.CreateConnectionAsync();

            // Abre o canal
            _channel = await _connection.CreateChannelAsync();

            Console.WriteLine($"[RabbitMQ-Client] Conexão estabelecida com o host '{host}'");
        }

        public IChannel GetChannel()
        {
            if (_channel == null || !_channel.IsOpen)
            {
                throw new InvalidOperationException("[RabbitMQ-Client] O canal não está disponível ou foi fechado.");
            }

            return _channel;
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}