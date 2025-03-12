using System.Text;
using System.Text.Json;
using GeekShopping.CartAPI.Messages;
using GeekShopping.MessageBus;
using RabbitMQ.Client;

namespace GeekShopping.CartAPI.RabbitMQSender;

public class RabbitMQMessageSender : IRabbitMQMessageSender
{
    private readonly string _hostname;
    private readonly string _password;
    private readonly string _username;
    private IConnection _connection;

    public RabbitMQMessageSender()
    {
        _hostname = "localhost";
        _password = "guest";
        _username = "guest";
    }

    public void SendMessage(BaseMessage message, string queueName)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostname,
            UserName = _username,
            Password = _password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: queueName, false, false, false, arguments: null);
        byte[] body = GetMessageAsByteArray(message);
        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }

    private byte[] GetMessageAsByteArray(BaseMessage message)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        
        var json = JsonSerializer.Serialize<CheckoutHeaderVO>((CheckoutHeaderVO)message, options);
        var body = Encoding.UTF8.GetBytes(json);
        
        return body;
    }
}