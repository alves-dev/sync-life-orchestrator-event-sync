using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Orchestrator;
using System.Text.Json;

namespace Broker.Rabbit
{
    public class RabbitMqService
    {
        private readonly IChannel _channel;
        private readonly Events _events;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _exchangeNameV3;

        public RabbitMqService(IChannel channel, Events events)
        {
            _channel = channel;
            _events = events;
            _queueName = Environment.GetEnvironmentVariable("RABBIT_QUEUE") ?? "default.queue";
            _exchangeName = Environment.GetEnvironmentVariable("RABBIT_EXCHANGE") ?? "default.exchange";
            _exchangeNameV3 = Environment.GetEnvironmentVariable("RABBIT_EXCHANGE_V3") ?? "default.exchange";

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

        public void PublicMenssageV3(EventBaseV3 myEvent)
        {
            BasicProperties props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Transient
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(myEvent, options);
            ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(json);

            string routingKey = myEvent.Type;

            _channel.BasicPublishAsync(_exchangeNameV3, routingKey, false, props, body);
            Console.WriteLine($"[Rabbit] Evento enviado para: {_exchangeNameV3} com routingKey: {routingKey}:\n{myEvent}");
        }

        private void HandleMessage(string message)
        {
            Console.WriteLine("--------------------------");
            Console.WriteLine($"[Rabbit] Evento recebido: {message}");
            _events.LaunchRabbitMessageReceived(message);
        }
    }
}