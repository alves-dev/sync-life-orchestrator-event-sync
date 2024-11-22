using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Broker.Rabbit
{
    public class RabbitMqService
    {
        private readonly RabbitMqConnection _connection;
        private readonly string _queueName;
        private readonly string _exchangeName;


        public RabbitMqService(RabbitMqConnection rabbitConnection)
        {
            _connection = rabbitConnection;
            _queueName = Environment.GetEnvironmentVariable("RABBIT_QUEUE") ?? "default.queue";
            _exchangeName = Environment.GetEnvironmentVariable("RABBIT_EXCHANGE") ?? "default.exchange";
        }

        public void StartListening()
        {
            var channel = _connection.GetChannel();

            // Cria um consumidor para a fila
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Aqui você pode processar a mensagem
                HandleMessage(message);

                // Confirmação de mensagem processada
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            // Inicia o consumidor
            channel.BasicConsumeAsync(_queueName, false, consumer);
            Console.WriteLine($"[RabbitMQ] Escutando na fila {_queueName}");
        }

        public void SubscribeToMessage(Action<BasicDeliverEventArgs> onMessageReceived)
        {
            var channel = _connection.GetChannel();

            // Cria um consumidor para a fila
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                onMessageReceived(ea);
            };

            channel.BasicConsumeAsync(_queueName, false, consumer);
        }

        public void PublicMenssage(string m)
        {
            var channel = _connection.GetChannel();
            var props = new BasicProperties
            {
                // Set any desired properties, e.g., delivery mode, content type, etc.
                DeliveryMode = DeliveryModes.Transient
            };
            ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(m);
            channel.BasicPublishAsync(_exchangeName, "routingKey", false, props, body);
        }

        // Método para tratar a mensagem (pode ser customizado ou sobrescrito)
        public virtual void HandleMessage(string message)
        {
            Console.WriteLine($"[Rabbit] Mensagem recebida: {message}");
        }
    }
}