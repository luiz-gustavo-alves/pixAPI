using pixAPI.Models;
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

  public async Task<Payments?> GetPaymentById(long paymentId) 
  {
    return await _context.Payments.FirstOrDefaultAsync(p => p.Id.Equals(paymentId));
  }

  public int GetAllTodayPaymentsByPSPCounter(long bankId)
  {
    DateTime today = DateTime.UtcNow;
    DateTime minDate = new DateTime(today.Date.Year, today.Date.Month, today.Date.Day);
    DateTime maxDate = minDate.AddDays(1);

    int paymentsCounter = _context.Payments.Where(p =>
        p.PaymentProviderAccount.BankId.Equals(bankId) &&
        p.CreatedAt.ToUniversalTime() >= minDate.ToUniversalTime() &&
        p.CreatedAt.ToUniversalTime() <= maxDate.ToUniversalTime()
      )
      .Count();

    return paymentsCounter;
  }

  public async Task<List<Payments>> GetTodayPaymentsByPSP(long bankId, int PAYMENT_CHUNK)
  {
    DateTime today = DateTime.UtcNow;
    DateTime minDate = new DateTime(today.Date.Year, today.Date.Month, today.Date.Day);
    DateTime maxDate = minDate.AddDays(1);

    List<Payments> todayPaymentsByPSP = await _context.Payments.Where(p =>
        p.PaymentProviderAccount.BankId.Equals(bankId) &&
        p.CreatedAt.ToUniversalTime() >= minDate.ToUniversalTime() &&
        p.CreatedAt.ToUniversalTime() <= maxDate.ToUniversalTime()
      )
      .OrderBy(p => p.CreatedAt)
      .Skip(PAYMENT_CHUNK)
      .ToListAsync();

    return todayPaymentsByPSP;
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