using Microsoft.EntityFrameworkCore;
using pixAPI.Data;
using pixAPI.Models;

namespace pixAPI.Repositories;

public class PixKeyRepository(AppDBContext context)
{
  private readonly AppDBContext _context = context;

  public async Task<PixKey> CreateAsync(PixKey pixKey)
  {
    _context.PixKey.Add(pixKey);
    await _context.SaveChangesAsync();
    return pixKey;
  }

  public async Task<List<PixKey>> GetAllPixKeysByUserBankAccountId(long paymentProviderAccountId)
  {
    return await _context.PixKey.Where(p => p.PaymentProviderAccountId.Equals(paymentProviderAccountId)).ToListAsync();
  }

  public async Task<PixKey?> GetPixKeyByValue(string value)
  {
    return await _context.PixKey.FirstOrDefaultAsync(p => p.Value.Equals(value));
  }

  public async Task<PixKey?> GetPixKeyByTypeAndValue(KeyType type, string value)
  {
    return await _context.PixKey.FirstOrDefaultAsync(p => p.Type.Equals(type) && p.Value.Equals(value));
  }

  public async Task<PixKey?> GetUserAndBankAccountDetailsWithPixKey(KeyType type, string value)
  {
    return await _context.PixKey
      .Include(p => p.PaymentProviderAccount)
      .Include(p => p.PaymentProviderAccount.Bank)
      .Include(p => p.PaymentProviderAccount.User)
      .FirstOrDefaultAsync(p => p.Type.Equals(type) && p.Value.Equals(value));
  }
}