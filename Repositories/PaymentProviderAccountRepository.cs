using Microsoft.EntityFrameworkCore;
using pixAPI.Data;
using pixAPI.Models;
using pixAPI.DTOs;

namespace pixAPI.Repositories;

public class PaymentProviderAccountRepository(AppDBContext context)
{
  private readonly AppDBContext _context = context;

  public async Task<PaymentProviderAccount> CreateAsync(PaymentProviderAccount account)
  {
    _context.PaymentProviderAccount.Add(account);
    await _context.SaveChangesAsync();
    return account;
  }

  public async Task<List<PaymentProviderAccount>> GetAllAccountsByUserId(long userId)
  {
    return await _context.PaymentProviderAccount.Where(a => a.UserId.Equals(userId)).ToListAsync();
  }

  public async Task<List<PaymentProviderAccount>> GetAllAccountsByBankId(long bankId)
  {
    return await _context.PaymentProviderAccount.Where(a => a.BankId.Equals(bankId)).ToListAsync();
  }

  public async Task<int> GetAccountsByBankIdAndUserCounter(long bankId, long userId)
  {
    List<PaymentProviderAccount> bankAccounts = await _context.PaymentProviderAccount.Where(
      a => a.BankId.Equals(bankId) && a.UserId.Equals(userId)
    ).ToListAsync();

    return bankAccounts.Count;
  }

  public async Task<PaymentProviderAccount?> GetAccountByBankAndUserDetails(
    long bankId,
    long userId,
    string agency,
    string number
  )
  {
    return await _context.PaymentProviderAccount.Where(a =>
      a.BankId.Equals(bankId) &&
      a.UserId.Equals(userId) &&
      a.Agency.Equals(agency) &&
      a.Number.Equals(number)
    ).FirstOrDefaultAsync();
  }
}