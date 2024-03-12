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

  public static PaymentProvider ValidateBankDataOrFail(PaymentProvider? bankData) 
  {
    if (bankData is null)
      throw new NotFoundException("Informações do banco não encontradas.");

    return bankData;
  }
}