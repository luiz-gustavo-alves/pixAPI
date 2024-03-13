using pixAPI.Models;
using pixAPI.Repositories;
using pixAPI.Exceptions;

namespace pixAPI.BLLs;

public class PaymentsBLL
{
  private static readonly int IDEMPOTENCE_KEY_TOLERANCE_SECONDS = 30;

  public static void ValidatePixKeyExists(PixKey? pixKey)
  {
    if (pixKey is null)
      throw new NotFoundException("Chave pix não encontrada");
  }

  public static void ValidateBankAccountExists(List<PaymentProviderAccount> bankAccounts)
  {
    if (bankAccounts.Count == 0)
      throw new NotFoundException("Conta bancária do usuário não encontrada");
  }

  public static void ValidateBankAccountOnwer(
    PaymentProviderAccount bankAccountToPay,
    List<PaymentProviderAccount> bankAccounts,
    long userId
  )
  {
    bool isUserBankAccount = bankAccountToPay.UserId.Equals(userId);
    if (!isUserBankAccount)
      return;

    if (bankAccounts.Count == 1)
      throw new CannotProceedPaymentException("Não é possível realizar pagamentos pela mesma conta bancária");
  }

  public static async Task ValidatePaymentIdempotence(
    PaymentRepository paymentRepository,
    long paymentProviderAccountId,
    long pixKeyId,
    int amount
  )
  {
    PaymentIdempotenceKey key = new(paymentProviderAccountId, pixKeyId, amount);
    Payments? payment = await paymentRepository.GetPaymentByIdempotenceKey(key, IDEMPOTENCE_KEY_TOLERANCE_SECONDS);

    if (payment is not null)
      throw new CannotProceedPaymentException(
        $"Não é possível realizar o mesmo pagamento em um período menor que {IDEMPOTENCE_KEY_TOLERANCE_SECONDS} segundos"
      );
  }

}