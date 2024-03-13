using pixAPI.Models;
using pixAPI.DTOs;
using pixAPI.Repositories;
using pixAPI.Helpers;
using pixAPI.BLLs;

namespace pixAPI.Services;

public class PaymentService(
  UserRepository userRepository,
  PaymentProviderAccountRepository paymentProviderAccountRepository,
  PixKeyRepository pixKeyRepository,
  PaymentRepository paymentRepository
)
{
  private readonly UserRepository _userRepository = userRepository;
  private readonly PaymentProviderAccountRepository _paymentProviderAccountRepository = paymentProviderAccountRepository;
  private readonly PixKeyRepository _pixKeyRepository = pixKeyRepository;
  private readonly PaymentRepository _paymentRepository = paymentRepository;

  private static List<PaymentProviderAccount> FilterBankAccountByNumberAndAgency(
    List<PaymentProviderAccount> bankAccounts,
    string number,
    string agency
  )
  {
    List<PaymentProviderAccount> bankAccount = [];
    foreach (var account in bankAccounts)
    {
      if (account.Number.Equals(number) && account.Agency.Equals(agency))
        bankAccount.Add(account);
    }
    return bankAccount;
  }

  public async Task<Payments> MakePayment(PaymentProvider? bankData, MakePaymentDTO dto)
  {
    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);

    string CPF = dto.Origin.User.CPF;
    User user = await ValidationHelper.GetUserByCPFOrFail(_userRepository, CPF);

    string value = dto.Destiny.Key.Value;
    string type = dto.Destiny.Key.Type;
    KeyType keyType = EnumHelper.MatchStringToKeyType(type);
    PixKey pixKey = await ValidationHelper.GetPixKeyByTypeAndValueOrFail(_pixKeyRepository, keyType, value);

    long bankId = validBankData.Id;
    List<PaymentProviderAccount> bankAccounts = await _paymentProviderAccountRepository.GetAccountsByBankIdAndUserId(
      bankId, user.Id
    );
    PaymentsBLL.ValidateBankAccountExists(bankAccounts);

    string number = dto.Origin.Account.Number;
    string agency = dto.Origin.Account.Agency;
    List<PaymentProviderAccount> bankAccountToPayList = FilterBankAccountByNumberAndAgency(bankAccounts, number, agency);
    PaymentsBLL.ValidateBankAccountExists(bankAccountToPayList);

    PaymentProviderAccount bankAccountToPay = bankAccountToPayList.ElementAt(0);
    PaymentsBLL.ValidateBankAccountOnwer(bankAccountToPay, bankAccounts, user.Id);

    await PaymentsBLL.ValidatePaymentIdempotence(_paymentRepository, bankAccountToPay.Id, pixKey.Id, dto.Amount);
    Payments payment = new()
    {
      Status = PaymentStatus.PROCESSING,
      PixKeyId = pixKey.Id,
      PaymentProviderAccountId = bankAccountToPay.Id,
      Amount = dto.Amount,
      Description = dto.Description,
    };

    Payments createdPayment = await _paymentRepository.RecordPaymentFromUser(payment);
    return createdPayment;
  }
}