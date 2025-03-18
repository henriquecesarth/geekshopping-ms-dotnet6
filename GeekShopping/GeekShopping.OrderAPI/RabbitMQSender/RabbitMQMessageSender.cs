using System.Text;
using System.Text.Json;
using GeekShopping.MessageBus;
using GeekShopping.OrderAPI.Messages;
using RabbitMQ.Client;

namespace GeekShopping.OrderAPI.RabbitMQSender;

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
        if (ConnectionExists())
        {
            using var channel = _connection.CreateModel();

            channel.QueueDeclare(queue: queueName, false, false, false, arguments: null);
            byte[] body = GetMessageAsByteArray(message);
            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }
    }

    private byte[] GetMessageAsByteArray(BaseMessage message)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        
        var json = JsonSerializer.Serialize<PaymentVO>((PaymentVO)message, options);
        var body = Encoding.UTF8.GetBytes(json);
        
        return body;
    }

    private void CreateConnection()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            
            _connection = factory.CreateConnection();
        }
        catch (Exception e)
        {
            // log exception
            Console.WriteLine(e);
            throw;
        }
    }

    private bool ConnectionExists()
    {
        if (_connection != null) return true;

        CreateConnection();
        
        return _connection != null;
    }
}