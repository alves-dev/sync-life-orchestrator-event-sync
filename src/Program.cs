using Orchestrator;
using Broker.MQTT;
using Broker.Rabbit;

class Program
{
    static async Task Main(string[] args)
    {
        DotNetEnv.Env.Load();

        Events events = new Events();
        RabbitMqClient rabbitClient = new RabbitMqClient();
        MqttClient mqttClient = new MqttClient();

        try
        {
            await mqttClient.InitializeAsync();
            MqttService mqttService = new MqttService(mqttClient.GetClient(), events);

            await rabbitClient.InitializeAsync();
            RabbitMqService rabbitService = new RabbitMqService(rabbitClient.GetChannel(), events);

            new OrchestratorService(events, rabbitService);

            // RUN
            Console.WriteLine("Aplicação rodando. Pressione Ctrl+C para encerrar.");

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true; // Impede a interrupção imediata
                cts.Cancel();    // Envia o sinal de cancelamento
                Console.WriteLine("Encerrando...");
                await rabbitClient.DisposeAsync();
                await mqttClient.DisposeAsync();
            };
            Task.Delay(Timeout.Infinite, cts.Token).Wait();
        }
        catch (OperationCanceledException)
        {
            // Espera até ser cancelado
            Console.WriteLine("Aplicação encerrada.");
        }
        finally
        {
            // Libera os recursos de forma assíncrona
            await rabbitClient.DisposeAsync();
            await mqttClient.DisposeAsync();
        }
    }
}
