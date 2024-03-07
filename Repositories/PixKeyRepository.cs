using Microsoft.EntityFrameworkCore;
using pixAPI.Data;
using pixAPI.Models;

namespace pixAPI.Repositories;

public class PixKeyRepository(AppDBContext context)
{
  private readonly AppDBContext _context = context;

  public async Task<List<PixKey>> GetAllPixKeysByUserBankAccountId(long paymentProviderAccountId)
  {
    return await _context.PixKey.Where(p => p.PaymentProviderAccountId.Equals(paymentProviderAccountId)).ToListAsync();
  }

  public async Task<PixKey?> GetPixKeyByTypeAndValue(KeyType type, float value) 
  {
    return await _context.PixKey.FirstOrDefaultAsync(p => p.Type.Equals(type) && p.Value.Equals(value));
  }
}