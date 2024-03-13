namespace pixAPI.Models;

public class PaymentIdempotenceKey(long paymentProviderAccountId, long pixKeyId, int amount)
{
  public long PaymentProviderAccountId { get; } = paymentProviderAccountId;

  public long PixKeyId { get; } = pixKeyId;

  public int Amount { get; } = amount;
}