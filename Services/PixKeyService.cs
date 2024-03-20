using pixAPI.BLLs;
using pixAPI.DTOs;
using pixAPI.Models;
using pixAPI.Repositories;
using pixAPI.Helpers;
using pixAPI.Exceptions;
using pixAPI.Data;

namespace pixAPI.Services;

public class PixKeyService(
  UserRepository userRepository,
  PaymentProviderAccountRepository paymentProviderAccountRepository,
  PixKeyRepository pixKeyRepository,
  AppDBContext context
)
{
  private readonly int USER_MAX_PIX_KEYS = 20;
  private readonly int PSP_MAX_PIX_KEYS = 5;
  private readonly UserRepository _userRepository = userRepository;
  private readonly PaymentProviderAccountRepository _paymentProviderAccountRepository = paymentProviderAccountRepository;
  private readonly PixKeyRepository _pixKeyRepository = pixKeyRepository;
  private readonly AppDBContext _context = context;

  private static bool CheckExistingUserBankAccount(List<PaymentProviderAccount> userAccountsFromPSP, CreatePixKeyDTO dto)
  {
    PaymentProviderAccount? existingBankAccount = userAccountsFromPSP.Where(account =>
      account.Agency.Equals(dto.Account.Agency) && account.Number.Equals(dto.Account.Number)).FirstOrDefault();

    return existingBankAccount is not null;
  }

  private static List<PixKey> GetAllPixKeysByUser(List<PaymentProviderAccount> userAccounts)
  {
    List<PixKey> userPixKeys = [];
    List<long> bankIdsList = [];
    foreach (var account in userAccounts)
    {
      if (bankIdsList.Contains(account.BankId))
        continue;

      List<PixKey> pixKeys = account.PixKeys.ToList();
      bankIdsList.Add(account.BankId);
      userPixKeys.AddRange(pixKeys);
    }
    return userPixKeys;
  }

  private static List<PaymentProviderAccount> FilterUserAccountByPSP(List<PaymentProviderAccount> userAccounts, long bankId)
  {
    return userAccounts.Where(account => account.BankId.Equals(bankId)).ToList();
  }

  private static List<PixKey> FilterPixKeysByUserAccount(List<PixKey> userPixKeys, long userBankAccountId)
  {
    return userPixKeys.Where(pixKey => pixKey.PaymentProviderAccountId.Equals(userBankAccountId)).ToList();
  }

  private async Task<List<PaymentProviderAccount>> ValidateUserPixKeyCreation(long userId, long bankId)
  {
    List<PaymentProviderAccount> userAccounts = await _paymentProviderAccountRepository.GetAllAccountsByUserId(userId);
    List<PixKey> userPixKeys = GetAllPixKeysByUser(userAccounts);
    PixKeyBLL.ValidatePixKeyCreationLimit(userPixKeys, USER_MAX_PIX_KEYS);

    List<PaymentProviderAccount> userAccountsFromPSP = FilterUserAccountByPSP(userAccounts, bankId);
    if (userAccountsFromPSP.Count > 0)
    {
      long userBankAccountId = userAccountsFromPSP.ElementAt(0).Id;
      List<PixKey> userPixKeysFromPSP = FilterPixKeysByUserAccount(userPixKeys, userBankAccountId);
      PixKeyBLL.ValidatePixKeyCreationLimit(userPixKeysFromPSP, PSP_MAX_PIX_KEYS);
    }
    return userAccountsFromPSP;
  }

  public async Task<PixKey> CreatePixKey(PaymentProvider? bankData, CreatePixKeyDTO dto)
  {
    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);
    User user = await ValidationHelper.GetUserByCPFOrFail(_userRepository, dto.User.CPF);
    KeyType keyType = EnumHelper.MatchStringToKeyType(dto.Key.Type);

    PixKeyBLL.ValidatePixKeyValue(keyType, dto.Key.Value, dto.User.CPF);
    await PixKeyBLL.ValidatePixKeyValueConflict(_pixKeyRepository, dto.Key.Value);

    List<PaymentProviderAccount> userAccountsFromPSP = await ValidateUserPixKeyCreation(user.Id, validBankData.Id);
    PixKey pixKey = new()
    {
      Type = keyType,
      Value = dto.Key.Value
    };

    bool existingUserBankAccount = CheckExistingUserBankAccount(userAccountsFromPSP, dto);
    var dbTransaction = _context.Database.BeginTransaction();
    if (existingUserBankAccount)
    {
      long userBankAccountId = userAccountsFromPSP.ElementAt(0).Id;
      pixKey.PaymentProviderAccountId = userBankAccountId;
    }
    else
    {
      PaymentProviderAccount account = new()
      {
        UserId = user.Id,
        BankId = validBankData.Id,
        Agency = dto.Account.Agency,
        Number = dto.Account.Number
      };
      PaymentProviderAccount createdAccount = await _paymentProviderAccountRepository.CreateAsync(account);
      pixKey.PaymentProviderAccountId = createdAccount.Id;
    }
    PixKey createdPixKey = await _pixKeyRepository.CreateAsync(pixKey);
    dbTransaction.Commit();

    return createdPixKey;
  }

  public async Task<GetPixKeyDTO> GetPixKey(string type, string value)
  {
    KeyType keyType = EnumHelper.MatchStringToKeyType(type);
    PixKey? pixKey = await _pixKeyRepository.GetUserAndBankAccountDetailsWithPixKey(keyType, value);
    if (pixKey is null)
      throw new NotFoundException("Chave Pix não encontrada.");

    PaymentProviderAccount account = pixKey.PaymentProviderAccount;
    PaymentProvider bank = account.Bank;
    User user = account.User;

    GetPixKeyDTO pixKeyDetails = new()
    {
      Account = new()
      {
        Agency = account.Agency,
        Number = account.Number,
        BankName = bank.BankName,
        BankId = bank.Id
      },
      User = new()
      {
        Name = user.Name,
        MaskedCpf = $"{user.CPF.Substring(0, 3)}{user.CPF.Substring(user.CPF.Length - 2)}",
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