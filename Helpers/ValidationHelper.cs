using pixAPI.Models;
using pixAPI.Repositories;
using pixAPI.Exceptions;

namespace pixAPI.Helpers;

public class ValidationHelper
{
  public static async Task<User> GetUserByCPFOrFail(UserRepository userRepository, string CPF)
  {
    User? user = await userRepository.GetUserByCPF(CPF);
    if (user is null)
      throw new NotFoundException("Usuário não encontrado.");

    return user;
  }

  public static async Task<PixKey> GetPixKeyByTypeAndValueOrFail(PixKeyRepository pixKeyRepository, KeyType type, string value)
  {
    PixKey? pixKey = await pixKeyRepository.GetPixKeyByTypeAndValue(type, value);
    if (pixKey is null)
      throw new NotFoundException("Chave pix não encontrada");

    return pixKey;
  }

  public static PaymentProvider ValidateBankDataOrFail(PaymentProvider? bankData)
  {
    if (bankData is null)
      throw new NotFoundException("Informações do banco não encontradas.");

    return bankData;
  }
}