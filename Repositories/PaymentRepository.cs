using pixAPI.Models;
using pixAPI.DTOs;
using pixAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace pixAPI.Repositories;

public class PaymentRepository(AppDBContext context)
{
  private readonly AppDBContext _context = context;

  public async Task<Payments> RecordPaymentFromUser(Payments payment)
  {
    _context.Payments.Add(payment);
    await _context.SaveChangesAsync();
    return payment;
  }

  public async Task<Payments?> GetPaymentByIdempotenceKey(PaymentIdempotenceKey key, int seconds)
  {
    DateTime secondsAgo = DateTime.UtcNow.AddSeconds(-seconds);
    Payments? payment = await _context.Payments.Where(p =>
      p.PixKeyId.Equals(key.PixKeyId) &&
      p.PaymentProviderAccountId.Equals(key.PaymentProviderAccountId) &&
      p.Amount.Equals(key.Amount) &&
      p.CreatedAt >= secondsAgo
    ).FirstOrDefaultAsync();

    return payment;
  }

  public async Task<Payments?> UpdatePaymentStatus(long paymentId, PaymentStatus status)
  {
    Payments? payment = await _context.Payments.Where(p => p.Id.Equals(paymentId)).FirstOrDefaultAsync();
    if (payment is null)
      return null;

    payment.Status = status;
    payment.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return payment;
  }
}