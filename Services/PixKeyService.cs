using pixAPI.BLLs;
using pixAPI.DTOs;
using pixAPI.Models;
using pixAPI.Repositories;
using pixAPI.Exceptions;
using pixAPI.Helpers;

namespace pixAPI.Services;

public class PixKeyService(
  UserRepository userRepository,
  PaymentProviderAccountRepository paymentProviderAccountRepository,
  PixKeyRepository pixKeyRepository)
{
  private readonly UserRepository _userRepository = userRepository;
  private readonly PaymentProviderAccountRepository _paymentProviderAccountRepository = paymentProviderAccountRepository;
  private readonly PixKeyRepository _pixKeyRepository = pixKeyRepository;

  private static bool CheckUserBankAccountExists(List<PaymentProviderAccount> userAccountsFromPSP, string agency, string number)
  {
    foreach (var account in userAccountsFromPSP)
    {
      if (account.Agency.Equals(agency) && account.Number.Equals(number))
        return true;
    }
    return false;
  }

  private async Task<List<PixKey>> GetAllPixKeysByUser(List<PaymentProviderAccount> userAccounts)
  {
    List<PixKey> userPixKeys = [];
    List<long> bankIdsList = [];
    foreach (var account in userAccounts)
    {
      if (bankIdsList.Contains(account.BankId))
        continue;

      List<PixKey> pixKeys = await _pixKeyRepository.GetAllPixKeysByUserBankAccountId(account.Id);
      bankIdsList.Add(account.BankId);
      userPixKeys.AddRange(pixKeys);
    }
    return userPixKeys;
  }

  private static List<PaymentProviderAccount> FilterUserAccountByBankId(List<PaymentProviderAccount> userAccounts, long bankId)
  {
    List<PaymentProviderAccount> userAccountsFromPSP = [];
    foreach (var account in userAccounts)
    {
      if (account.BankId.Equals(bankId))
        userAccountsFromPSP.Add(account);
    }
    return userAccountsFromPSP;
  }

  private static List<PixKey> FilterPixKeyByUserBankAccountId(List<PixKey> userPixKeys, long userBankAccountId)
  {
    List<PixKey> userPixKeysFromPSP = [];
    foreach (var pixKey in userPixKeys)
    {
      if (pixKey.PaymentProviderAccountId.Equals(userBankAccountId))
        userPixKeysFromPSP.Add(pixKey);
    }
    return userPixKeysFromPSP;
  }

  private async Task<List<PaymentProviderAccount>> ValidateUserPixKeyCreation(long userId, long bankId)
  {
    List<PaymentProviderAccount> userAccounts = await _paymentProviderAccountRepository.GetAllAccountsByUserId(userId);
    List<PixKey> userPixKeys = await GetAllPixKeysByUser(userAccounts);
    PixKeyBLL.ValidatePixKeyCreationLimit(userPixKeys, 20);

    List<PaymentProviderAccount> userAccountsFromPSP = FilterUserAccountByBankId(userAccounts, bankId);
    if (userAccountsFromPSP.Count > 0)
    {
      long userBankAccountId = userAccountsFromPSP.ElementAt(0).Id;
      List<PixKey> userPixKeysFromPSP = FilterPixKeyByUserBankAccountId(userPixKeys, userBankAccountId);
      PixKeyBLL.ValidatePixKeyCreationLimit(userPixKeysFromPSP, 5);
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

    long userId = user.Id;
    long bankId = validBankData.Id;
    List<PaymentProviderAccount> userAccountsFromPSP = await ValidateUserPixKeyCreation(userId, bankId);

    PixKey pixKey = new()
    {
      Type = keyType,
      Value = dto.Key.Value
    };

    if (CheckUserBankAccountExists(userAccountsFromPSP, dto.Account.Agency, dto.Account.Number))
    {
      long userBankAccountId = userAccountsFromPSP.ElementAt(0).Id;
      pixKey.PaymentProviderAccountId = userBankAccountId;
    }
    else
    {
      PaymentProviderAccount account = new() 
      { 
        UserId = userId,
        BankId = bankId,
        Agency = dto.Account.Agency,
        Number = dto.Account.Number
      };
      PaymentProviderAccount createdAccount = await _paymentProviderAccountRepository.CreateAsync(account);
      pixKey.PaymentProviderAccountId = createdAccount.Id;
    }
    PixKey createdPixKey = await _pixKeyRepository.CreateAsync(pixKey);
    return createdPixKey;
  }

  public async Task<GetPixKeyDTO> GetPixKey(PaymentProvider? bankData, string type, string value) 
  {
    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);
    KeyType keyType = EnumHelper.MatchStringToKeyType(type);
    PixKey pixKey = await ValidationHelper.GetPixKeyByTypeAndValueOrFail(_pixKeyRepository, keyType, value);

    long paymentProviderAccountId = pixKey.PaymentProviderAccountId;
    GetPixKeyDTO? pixKeyDetails = PixKeyBLL.GetPixKeyDetailsOrFail(
      _paymentProviderAccountRepository, paymentProviderAccountId, type, value, validBankData
    );

    return pixKeyDetails;
  }
}