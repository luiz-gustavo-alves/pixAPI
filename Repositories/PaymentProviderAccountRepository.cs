using Microsoft.EntityFrameworkCore;
using pixAPI.Data;
using pixAPI.Models;

namespace pixAPI.Repositories;

public class PaymentProviderAccountRepository(AppDBContext context)
{
  private readonly AppDBContext _context = context;

  public async Task<List<PaymentProviderAccount>> GetAllAccountsByUserId(long userId)
  {
    return await _context.PaymentProviderAccount.Where(a => a.UserId.Equals(userId)).ToListAsync();
  }

  public async Task<List<PaymentProviderAccount>> GetAllAccountsByBankId(long bankId)
  {
    return await _context.PaymentProviderAccount.Where(a => a.BankId.Equals(bankId)).ToListAsync();
  }

    public static implicit operator PaymentProviderAccountRepository(PaymentProviderRepository v)
    {
        throw new NotImplementedException();
    }
}