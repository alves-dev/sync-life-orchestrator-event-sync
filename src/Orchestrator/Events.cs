namespace Orchestrator
{
    public class Events
    {
      
        public delegate void MessageEventHandler(string message);

        public event MessageEventHandler RabbitMessageReceived;

        public event MessageEventHandler MQTTMessageReceived;

        public event MessageEventHandler RabbitMessageToSend;

        public event MessageEventHandler MQTTMessageToSend;

        public void LaunchRabbitMessageReceived(string message)
        {
            RabbitMessageReceived?.Invoke(message);
        }

        public void LaunchMQTTMessageReceived(string message)
        {
            MQTTMessageReceived?.Invoke(message);
        }

        public void LaunchRabbitMessageToSend(string message)
        {
            RabbitMessageToSend?.Invoke(message);
        }

        public void LaunchMQTTMessageToSend(string message)
        {
            MQTTMessageToSend?.Invoke(message);
        }
    }
}