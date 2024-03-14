using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using pixAPI.Config;
using pixAPI.Models;
using RabbitMQ.Client;

namespace pixAPI.Services;

public class MessageService(IOptions<QueueConfig> queueConfig)
{
  private readonly string _hostName = queueConfig.Value.HostName;

  public void SendPaymentMessage(Payments createdPayment)
  {
    ConnectionFactory factory = new()
    {
      HostName = _hostName
    };

    IConnection connection = factory.CreateConnection();
    using IModel channel = connection.CreateModel();

    channel.QueueDeclare(
      queue: "payments",
      durable: false,
      exclusive: false,
      autoDelete: false,
      arguments: null
    );

    Payments payment = new()
    {
      Id = createdPayment.Id,
      Status = createdPayment.Status,
      PixKeyId = createdPayment.PixKeyId,
      PaymentProviderAccountId = createdPayment.PaymentProviderAccountId,
      Amount = createdPayment.Amount,
      Description = createdPayment.Description,
      CreatedAt = createdPayment.CreatedAt,
      UpdatedAt = createdPayment.UpdatedAt,
    };

    string json = JsonSerializer.Serialize(payment);
    var body = Encoding.UTF8.GetBytes(json);

    channel.BasicPublish(
      exchange: string.Empty,
      routingKey: "payments",
      basicProperties: null,
      body: body
    );
  }
}