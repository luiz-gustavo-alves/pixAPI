using pixAPI.Models;
using pixAPI.DTOs;
using pixAPI.Repositories;
using pixAPI.Helpers;
using pixAPI.BLLs;
using pixAPI.Data;

namespace pixAPI.Services;

public class PaymentService(
  UserRepository userRepository,
  PaymentProviderAccountRepository paymentProviderAccountRepository,
  PixKeyRepository pixKeyRepository,
  PaymentRepository paymentRepository,
  MessageService messageService,
  AppDBContext context
)
{
  private readonly UserRepository _userRepository = userRepository;
  private readonly PaymentProviderAccountRepository _paymentProviderAccountRepository = paymentProviderAccountRepository;
  private readonly PixKeyRepository _pixKeyRepository = pixKeyRepository;
  private readonly PaymentRepository _paymentRepository = paymentRepository;
  private readonly MessageService _messageService = messageService;
  private readonly AppDBContext _context = context;

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
    long userId = user.Id;
    string number = dto.Origin.Account.Number;
    string agency = dto.Origin.Account.Agency;
    List<PaymentProviderAccount> originBankAccountList = await _paymentProviderAccountRepository.
      GetAccountByBankAndUserDetails(bankId, userId, agency, number);

    PaymentsBLL.ValidateBankAccountExists(originBankAccountList);

    int bankAccountsCounter = await _paymentProviderAccountRepository.GetAccountsByBankIdAndUserCounter(bankId, userId);
    PaymentProviderAccount originBankAccount = originBankAccountList.ElementAt(0);
    PaymentsBLL.ValidateBankAccountOnwer(originBankAccount, bankAccountsCounter, userId);
    await PaymentsBLL.ValidatePaymentIdempotence(_paymentRepository, originBankAccount.Id, pixKey.Id, dto.Amount);

    var dbTransaction = _context.Database.BeginTransaction();
    Payments payment = new()
    {
      Status = PaymentStatus.PROCESSING,
      PixKeyId = pixKey.Id,
      PaymentProviderAccountId = originBankAccount.Id,
      Amount = dto.Amount,
      Description = dto.Description,
    };
    Payments createdPayment = await _paymentRepository.RecordPaymentFromUser(payment);
    PaymentsBLL.SyncPaymentToCloud(_messageService, payment);
    dbTransaction.Commit();

    return createdPayment;
  }
}