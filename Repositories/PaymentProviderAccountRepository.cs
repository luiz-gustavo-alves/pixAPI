using Microsoft.EntityFrameworkCore;
using pixAPI.Data;
using pixAPI.Models;
using pixAPI.DTOs;
using pixAPI.Helpers;
using pixAPI.Exceptions;

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

  public async Task<List<PaymentProviderAccount>> GetAccountsByBankIdAndUserId(long bankId, long userId)
  {
    return await _context.PaymentProviderAccount.Where(a => a.BankId.Equals(bankId) && a.UserId.Equals(userId)).ToListAsync();
  }

  public GetPixKeyDTO GetUserAndBankDetailsWithPixKey(
    long paymentProviderAccountId,
    string type,
    string value
  )
  {
    var userAndBankDetails = (
                              from account in _context.PaymentProviderAccount
                              join usr in _context.User on account.UserId equals usr.Id
                              join bank in _context.PaymentProvider on account.BankId equals bank.Id
                              where account.Id == paymentProviderAccountId
                              select new
                              {
                                Name = usr.Name,
                                MaskedCpf = $"{usr.CPF.Substring(0, 3)}{usr.CPF.Substring(usr.CPF.Length - 2)}",
                                Agency = account.Agency,
                                Number = account.Number,
                                BankName = bank.BankName,
                                BankId = bank.Id,
                              }
                            ).FirstOrDefault();

    if (userAndBankDetails is null)
      throw new NotFoundException("Conta de usuário não encontrada");

    GetPixKeyDTO pixKeyDetails = new()
    {
      Account = new()
      {
        Agency = userAndBankDetails.Agency,
        Number = userAndBankDetails.Number,
        BankName = userAndBankDetails.BankName,
        BankId = userAndBankDetails.BankId.ToString()
      },
      User = new()
      {
        Name = userAndBankDetails.Name,
        MaskedCpf = userAndBankDetails.MaskedCpf,
      },
      Key = new()
      {
        Type = type,
        Value = value,
      }
    };
    return pixKeyDetails;
  }
}