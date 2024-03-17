using pixAPI.Models;
using pixAPI.DTOs;
using pixAPI.Repositories;
using pixAPI.Helpers;
using pixAPI.BLLs;
using pixAPI.Data;
using pixAPI.Exceptions;

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
    PaymentProviderAccount? originBankAccount = await _paymentProviderAccountRepository.
      GetAccountByBankAndUserDetails(bankId, userId, agency, number);

    if (originBankAccount is null)
      throw new NotFoundException("Conta bancária do usuário não encontrada");

    int bankAccountsCounter = await _paymentProviderAccountRepository.GetAccountsByBankIdAndUserCounter(bankId, userId);
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
    PaymentsBLL.SyncPaymentToCloud(_messageService, payment, dto, validBankData.Token);
    dbTransaction.Commit();

    return createdPayment;
  }

  public async Task<Payments> UpdatePaymentStatus(long paymentId, UpdatePaymentStatusDTO dto)
  {
    PaymentStatus status = EnumHelper.MatchStringToPaymentStatus(dto.Status);
    Payments? updatedPayment = await _paymentRepository.UpdatePaymentStatus(paymentId, status);
    if (updatedPayment is null)
      throw new NotFoundException("Pagamento não encontrado");

    return updatedPayment;
  }
}