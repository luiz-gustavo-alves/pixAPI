using Microsoft.EntityFrameworkCore;
using pixAPI.Data;
using pixAPI.Models;

namespace pixAPI.Repositories;

public class PaymentProviderRepository(AppDBContext context)
{
  private readonly AppDBContext _context = context;

  public async Task<PaymentProvider?> GetBankByToken(string token)
  {
    return await _context.PaymentProvider.FirstOrDefaultAsync(psp => psp.Token.Equals(token));
  }

    public async Task<PaymentProvider?> GetBankByName(string name)
  {
    return await _context.PaymentProvider.FirstOrDefaultAsync(psp => psp.BankName.Equals(name));
  }

  public async Task<PaymentProvider?> GetBankById(int id)
  {
    return await _context.PaymentProvider.FirstOrDefaultAsync(psp => psp.Id.Equals(id));
  }
}