using System.Text;
using System.Text.Json;
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GeekShopping.OrderAPI.MessageConsumer;

public class RabbitMQCheckoutConsumer : BackgroundService
{
    private readonly OrderRepository _repository;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMQCheckoutConsumer(OrderRepository repository)
    {
        _repository = repository;
        
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "checkoutqueue", false, false, false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (channel, evt) =>
        {
            var content = Encoding.UTF8.GetString(evt.Body.ToArray());
            CheckoutHeaderVO vo = JsonSerializer.Deserialize<CheckoutHeaderVO>(content);
            ProcessOrder(vo).GetAwaiter().GetResult();
            _channel.BasicAck(evt.DeliveryTag, false);
        };

        _channel.BasicConsume("checkoutqueue", false, consumer);
        return Task.CompletedTask;
    }

    private async Task ProcessOrder(CheckoutHeaderVO vo)
    {
        OrderHeader order = new()
        {
            UserId = vo.UserId,
            FirstName = vo.FirstName,
            LastName = vo.LastName,
            OrderDetails = new List<OrderDetail>(),
            CardNumber = vo.CardNumber,
            CouponCode = vo.CouponCode,
            CVV = vo.CVV,
            DiscountAmount = vo.DiscountAmount,
            Email = vo.Email,
            ExpireMonthYear = vo.ExpireMonthYear,
            OrderTime = DateTime.Now,
            PurchaseAmount = vo.PurchaseAmount,
            PaymentStatus = false,
            Phone = vo.Phone
        };

        foreach (var cartDetail in vo.CartDetails)
        {
            OrderDetail detail = new()
            {
                ProductId = cartDetail.ProductId,
                ProductName = cartDetail.Product.Name,
                Price = cartDetail.Product.Price,
                Count = cartDetail.Count
            };

            order.OrderTotalItens += detail.Count;
            order.OrderDetails.Add(detail);
        }

        await _repository.AddOrder(order);
    }
}