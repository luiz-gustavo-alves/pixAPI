using Microsoft.EntityFrameworkCore;
using pixAPI.Data;
using pixAPI.Models;
using pixAPI.DTOs;
using pixAPI.Helpers;

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

  public GetPixKeyDTO? GetUserAndBankDetailsWithPixKey(long paymentProviderAccountId, string type, string value)
  {
    var userAndBankDetails = from account in _context.PaymentProviderAccount
                             join bank in _context.PaymentProvider on account.BankId equals bank.Id
                             join usr in _context.User on account.UserId equals usr.Id
                             where account.Id == paymentProviderAccountId
                             select new
                             {
                               Name = usr.Name,
                               MaskedCpf = $"{usr.CPF.Substring(0, 3)}{usr.CPF.Substring(usr.CPF.Length - 2)}",
                               Agency = account.Agency,
                               Number = account.Number,
                               BankName = bank.BankName,
                               BankId = bank.Id,
                             };

    List<GetPixKeyDTO> pixKeyDTO = [];
    foreach (var detail in userAndBankDetails)
    {
      GetPixKeyDTO pixKeyDetails = new()
      {
        Account = new()
        {
          Agency = detail.Agency,
          Number = detail.Number,
          BankName = detail.BankName,
          BankId = detail.BankId.ToString()
        },
        User = new()
        {
          Name = detail.Name,
          MaskedCpf = detail.MaskedCpf,
        },
        Key = new()
        {
          Type = type,
          Value = value,
        }
      };
      pixKeyDTO.Add(pixKeyDetails);
    }
    return pixKeyDTO.FirstOrDefault();
  }
}