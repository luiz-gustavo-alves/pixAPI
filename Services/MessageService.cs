using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using pixAPI.Config;
using pixAPI.DTOs;
using pixAPI.Helpers;
using pixAPI.Models;
using RabbitMQ.Client;

namespace pixAPI.Services;

public class MessageService(IOptions<QueueConfig> queueConfig)
{
  private readonly string _hostName = queueConfig.Value.HostName;

  public void SendPaymentMessage(Payments payment, MakePaymentDTO dto, string bankToken, int toleranceTime)
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

    string paymentStatus = EnumHelper.MatchPaymentStatusToString(payment.Status);
    PaymentMessageServiceDTO messageDto = new()
    {
      Id = payment.Id,
      Status = paymentStatus,
      Token = bankToken,
      DTO = dto,
    };

    string json = JsonSerializer.Serialize(messageDto);
    var body = Encoding.UTF8.GetBytes(json);
    DateTime timeToLive = DateTime.UtcNow.AddSeconds(toleranceTime);

    IBasicProperties properties = channel.CreateBasicProperties();
    properties.Persistent = true;
    properties.Headers = new Dictionary<string, object>
    {
      { "time-to-live", new DateTimeOffset(timeToLive).ToUnixTimeSeconds() },
    };

    channel.BasicPublish(
      exchange: string.Empty,
      routingKey: "payments",
      basicProperties: properties,
      body: body
    );
  }
}