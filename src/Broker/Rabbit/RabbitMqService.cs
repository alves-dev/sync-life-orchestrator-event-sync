using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Orchestrator;

namespace Broker.Rabbit
{
    public class RabbitMqService
    {
        private readonly IChannel _channel;
        private readonly Events _events;
        private readonly string _queueName;
        private readonly string _exchangeName;

        public RabbitMqService(IChannel channel, Events events)
        {
            _channel = channel;
            _events = events;
            _queueName = Environment.GetEnvironmentVariable("RABBIT_QUEUE") ?? "default.queue";
            _exchangeName = Environment.GetEnvironmentVariable("RABBIT_EXCHANGE") ?? "default.exchange";

            StartListening();
        }

        private void StartListening()
        {
            // Cria um consumidor para a fila
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                HandleMessage(message);

                // Confirmação de mensagem processada
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            // Inicia o consumidor
            _channel.BasicConsumeAsync(_queueName, false, consumer);
            Console.WriteLine($"[RabbitMQ] Escutando na fila {_queueName}");
        }

        public void PublicMenssage(string routingKey, string message)
        {
            BasicProperties props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Transient
            };
            ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublishAsync(_exchangeName, routingKey, false, props, body);
            Console.WriteLine($"[Rabbit] Mensagem enviada para: {_exchangeName} com routingKey: {routingKey}");
        }

        private void HandleMessage(string message)
        {
            Console.WriteLine($"[Rabbit] Mensagem recebida: {message}");
            _events.LaunchRabbitMessageReceived(message);
        }
    }
}