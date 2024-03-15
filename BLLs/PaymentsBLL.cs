using pixAPI.Models;
using pixAPI.Repositories;
using pixAPI.Exceptions;
using pixAPI.Services;
using pixAPI.DTOs;
using pixAPI.Helpers;

namespace pixAPI.BLLs;

public class PaymentsBLL
{
  private static readonly int IDEMPOTENCE_KEY_TOLERANCE_SECONDS = 30;
  private static readonly int PAYMENT_MESSAGE_TOLERANCE_SECONDS = 120;

  public static void ValidateBankAccountExists(List<PaymentProviderAccount> bankAccount)
  {
    if (bankAccount.Count == 0)
      throw new NotFoundException("Conta bancária do usuário não encontrada");
  }

  public static void ValidateBankAccountOnwer(
    PaymentProviderAccount originalBankAccount,
    int bankAccountsCounter,
    long userId
  )
  {
    bool isUserBankAccount = originalBankAccount.UserId.Equals(userId);
    if (!isUserBankAccount)
      return;

    if (bankAccountsCounter == 1)
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

  public static void SyncPaymentToCloud(MessageService messageService, Payments payment)
  {
    string paymentStatus = EnumHelper.MatchPaymentStatusToString(payment.Status);
    PaymentTransferStatusDTO dto = new() 
    {
      Id = payment.Id,
      Status = paymentStatus
    };

    try {
      messageService.SendPaymentMessage(dto, PAYMENT_MESSAGE_TOLERANCE_SECONDS);
    } catch {
      throw new ServiceUnavailableException("Serviço indisponível. Tente novamente mais tarde");
    }
  }
}