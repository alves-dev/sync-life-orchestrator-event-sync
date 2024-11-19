using RabbitMQ.Client;
using System.Threading.Tasks;

namespace Broker.Rabbit
{
    public class RabbitMqConnection
    {
        private IConnection? _connection;
        private IChannel? _channel;

        public async Task InitializeAsync()
        {
            string host = Environment.GetEnvironmentVariable("RABBIT_HOST") ?? "localhost";
            string user = Environment.GetEnvironmentVariable("RABBIT_USER") ?? "guest";
            string password = Environment.GetEnvironmentVariable("RABBIT_PASSWORD") ?? "guest";

            // Configura o factory com as credenciais
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

            Console.WriteLine($"[RabbitMQ] Conexão estabelecida com o host '{host}'");
        }

        // Método para expor o canal
        public IChannel GetChannel()
        {
            if (_channel == null || !_channel.IsOpen)
            {
                throw new InvalidOperationException("O canal não está disponível ou foi fechado.");
            }

            return _channel;
        }

        // Método para liberar recursos (Cleanup)
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