using pixAPI.DTOs;
using pixAPI.Models;
using pixAPI.Repositories;
using pixAPI.Exceptions;
using pixAPI.Helpers;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace pixAPI.Services;

public class PixKeyService(
  UserRepository userRepository,
  PaymentProviderAccountRepository paymentProviderAccountRepository,
  PixKeyRepository pixKeyRepository)
{
  private readonly UserRepository _userRepository = userRepository;
  private readonly PaymentProviderAccountRepository _paymentProviderAccountRepository = paymentProviderAccountRepository;
  private readonly PixKeyRepository _pixKeyRepository = pixKeyRepository;

  private async Task<User> GetUserByCPFOrFail(string CPF)
  {
    User? user = await _userRepository.GetUserByCPF(CPF) ?? throw new NotFoundException("Usuário não encontrado");
    return user;
  }

  private static void ValidatePixKeyValue(KeyType keyType, string value, string CPF)
  {
    switch (keyType)
    {
      case KeyType.CPF:
        Regex cpfRegex = new(@"\d{11}");
        if (!cpfRegex.IsMatch(value) || !CPF.Equals(value))
          throw new CannotProceedPixKeyCreation("Valor inválido para chave pix CPF");
        break;

      case KeyType.Email:
        if (!MailAddress.TryCreate(value, out var mailAddress))
          throw new CannotProceedPixKeyCreation("Valor inválido para chave pix Email");
        break;

      case KeyType.Phone:
        int MAX_PHONE_LENGTH = 11;
        Regex phoneRegex = new(@"^\(?(?:[14689][1-9]|2[12478]|3[1234578]|5[1345]|7[134579])\)? ?(?:[2-8]|9[0-9])[0-9]{3}\-?[0-9]{4}$");
        if (!phoneRegex.IsMatch(value) || value.Length != MAX_PHONE_LENGTH)
          throw new CannotProceedPixKeyCreation("Valor inválido para chave pix Celular");
        break;

      case KeyType.Random:
        if (!Guid.TryParse(value, out _))
          throw new CannotProceedPixKeyCreation("Valor inválido para chave pix Aleatória");
        break;
    }
  }

  private static void ValidatePixKeyCreationLimit(List<PixKey> pixKeys, int limit)
  {
    if (pixKeys.Count >= limit)
      throw new CannotProceedPixKeyCreation("Limite excedido para criação de chave pix");
  }

  private static void ValidatePixKeyCPFConflict(List<PixKey> userPixKeysFromPSP, KeyType keyType)
  {
    foreach (var pixKey in userPixKeysFromPSP)
    {
      if (pixKey.Type.Equals(KeyType.CPF) && keyType.Equals(KeyType.CPF))
        throw new CannotProceedPixKeyCreation("Não é possível criar mais de uma chave pix CPF");
    }
  }

  private static void ValidatePixKeyValueConflict(List<PixKey> userPixKeysFromPSP, string value)
  {
    foreach (var pixKey in userPixKeysFromPSP)
    {
      if (pixKey.Value.Equals(value))
        throw new CannotProceedPixKeyCreation("Não é possível criar chave pix com mesmo valor de chave associada a esta conta.");
    }
  }

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

  private async Task<List<PaymentProviderAccount>> ValidateUserPixKeyCreation(
    long userId,
    PaymentProvider bankData,
    KeyType keyType,
    string value)
  {
    List<PaymentProviderAccount> userAccounts = await _paymentProviderAccountRepository.GetAllAccountsByUserId(userId);
    List<PixKey> userPixKeys = await GetAllPixKeysByUser(userAccounts);
    ValidatePixKeyCreationLimit(userPixKeys, 20);
    ValidatePixKeyCPFConflict(userPixKeys, keyType);

    long bankId = bankData.Id;
    List<PaymentProviderAccount> userAccountsFromPSP = FilterUserAccountByBankId(userAccounts, bankId);
    if (userAccountsFromPSP.Count > 0)
    {
      long userBankAccountId = userAccountsFromPSP.ElementAt(0).Id;
      List<PixKey> userPixKeysFromPSP = FilterPixKeyByUserBankAccountId(userPixKeys, userBankAccountId);
      ValidatePixKeyCreationLimit(userPixKeysFromPSP, 5);
      ValidatePixKeyValueConflict(userPixKeysFromPSP, value);
    }
    return userAccountsFromPSP;
  }

  public async Task CreatePixKey(PaymentProvider bankData, CreatePixKeyDTO dto)
  {
    string CPF = dto.User.CPF;
    User user = await GetUserByCPFOrFail(CPF);

    string value = dto.Key.Value;
    string type = dto.Key.Type;
    KeyType keyType = EnumHelper.MatchStringToKeyType(type);
    ValidatePixKeyValue(keyType, value, CPF);

    long userId = user.Id;
    List<PaymentProviderAccount> userAccountsFromPSP = await ValidateUserPixKeyCreation(userId, bankData, keyType, value);

    PixKey pixKey = new()
    {
      Type = keyType,
      Value = value,
    };

    string agency = dto.Account.Agency;
    string number = dto.Account.Number;

    bool bankAcountExists = CheckUserBankAccountExists(userAccountsFromPSP, agency, number);
    if (bankAcountExists)
    {
      long userBankAccountId = userAccountsFromPSP.ElementAt(0).Id;
      pixKey.PaymentProviderAccountId = userBankAccountId;
    }
    else
    {
      long bankId = bankData.Id;
      PaymentProviderAccount account = new()
      {
        UserId = userId,
        BankId = bankId,
        Agency = agency,
        Number = number
      };
      PaymentProviderAccount createdAccount = await _paymentProviderAccountRepository.CreateAsync(account);
      pixKey.PaymentProviderAccountId = createdAccount.Id;
    }
    await _pixKeyRepository.CreateAsync(pixKey);
  }
}