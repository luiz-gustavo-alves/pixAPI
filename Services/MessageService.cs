using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using pixAPI.Config;
using pixAPI.DTOs;
using RabbitMQ.Client;

namespace pixAPI.Services;

public class MessageService(IOptions<QueueConfig> queueConfig)
{
  private readonly string _hostName = queueConfig.Value.HostName;

  public void SendPaymentMessage(PaymentTransferStatusDTO dto, int toleranceTime)
  {
    ConnectionFactory factory = new()
    {
      HostName = _hostName
    };

    IConnection connection = factory.CreateConnection();
    using IModel channel = connection.CreateModel();

    channel.QueueDeclare(
      queue: "payments",
      durable: true,
      exclusive: false,
      autoDelete: false,
      arguments: null
    );

    string json = JsonSerializer.Serialize(dto);
    var body = Encoding.UTF8.GetBytes(json);

    IBasicProperties properties = channel.CreateBasicProperties();
    properties.Persistent = true;
    properties.Headers = new Dictionary<string, object>
    {
      { "time-to-live", toleranceTime }
    };

    channel.BasicPublish(
      exchange: string.Empty,
      routingKey: "payments",
      basicProperties: properties,
      body: body
    );
  }
}