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

  private async Task<User> GetUserByCPFOrFail(string CPF)
  {
    User? user = await _userRepository.GetUserByCPF(CPF) ?? throw new NotFoundException("Usuário não encontrado");
    return user;
  }

  private static void ValidatePixKeyLimitCreation(List<PixKey> pixKeys, int limit)
  {
    if (pixKeys.Count >= limit)
    {
      throw new CannotProceedPixKeyCreation("Limite excedido para criação de chave pix");
    }
  }

  private static void ValidatePixKeyCPFConflict(List<PixKey> userPixKeysFromPSP, KeyType keyType)
  {
    foreach (var pixKey in userPixKeysFromPSP)
    {
      if (pixKey.Type.Equals("CPF") && keyType.Equals("CPF"))
      {
        throw new CannotProceedPixKeyCreation("Não é possível criar mais de uma chave pix do tipo CPF nesta conta associada a esta PSP");
      }
    }
  }

  private static bool CheckUserBankAccountExists(List<PaymentProviderAccount> userAccountsFromPSP, string agency, string number) 
  {
    foreach (var account in userAccountsFromPSP)
    {
      if (account.Agency.Equals(agency) && account.Number.Equals(number))
      {
        return true;
      }
    }
    return false;
  }

  private async Task<List<PixKey>> GetAllPixKeysByUser(List<PaymentProviderAccount> userAccounts)
  {
    List<PixKey> userPixKeys = [];
    foreach (var account in userAccounts)
    {
      long userBankAccountId = account.BankId;
      List<PixKey> pixKeys = await _pixKeyRepository.GetAllPixKeysByUserBankAccountId(userBankAccountId);
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
      {
        userAccountsFromPSP.Add(account);
      }
    }
    return userAccountsFromPSP;
  }

  private static List<PixKey> FilterUserPixKeyByUserBankAccountId(List<PixKey> userPixKeys, long userBankAccountId)
  {
    List<PixKey> userPixKeysFromPSP = [];
    foreach (var pixKey in userPixKeys)
    {
      if (pixKey.PaymentProviderAccountId.Equals(userBankAccountId))
      {
        userPixKeysFromPSP.Add(pixKey);
      }
    }
    return userPixKeysFromPSP;
  }

  private async Task<List<PaymentProviderAccount>> ValidateUserPixKeyCreation(long userId, PaymentProvider bankData, KeyType keyType)
  {
    List<PaymentProviderAccount> userAccounts = await _paymentProviderAccountRepository.GetAllAccountsByUserId(userId);
    List<PixKey> userPixKeys = await GetAllPixKeysByUser(userAccounts);
    ValidatePixKeyLimitCreation(userPixKeys, 20);

    long bankId = bankData.Id;
    List<PaymentProviderAccount> userAccountsFromPSP = FilterUserAccountByBankId(userAccounts, bankId);
    if (userAccountsFromPSP.Count > 0)
    {
      long userBankAccountId = userAccountsFromPSP.ElementAt(0).Id;
      List<PixKey> userPixKeysFromPSP = FilterUserPixKeyByUserBankAccountId(userPixKeys, userBankAccountId);
      ValidatePixKeyLimitCreation(userPixKeysFromPSP, 5);
      ValidatePixKeyCPFConflict(userPixKeysFromPSP, keyType);
    }
    return userAccountsFromPSP;
  }

  public async Task CreatePixKey(PaymentProvider bankData, CreatePixKeyDTO dto)
  {
    string CPF = dto.User.CPF;
    User user = await GetUserByCPFOrFail(CPF);
    if (!CPF.Equals(user.CPF))
    {
      throw new CannotProceedPixKeyCreation("CPF fornecido é diferente do CPF do usuário encontrado");
    }

    string type = dto.Key.Type;
    KeyType keyType = EnumHelper.MatchStringToKeyType(type);

    long userId = user.Id;
    List<PaymentProviderAccount> userAccountsFromPSP = await ValidateUserPixKeyCreation(userId, bankData, keyType);

    string agency = dto.Account.Agency;
    string number = dto.Account.Number;
    bool bankAcountExists = CheckUserBankAccountExists(userAccountsFromPSP, agency, number);
  }
}