using System.Text.Json;
using pixAPI.DTOs;
using pixAPI.Exceptions;
using pixAPI.Helpers;
using pixAPI.Models;
using pixAPI.Repositories;

namespace pixAPI.Services;

public class ConcilliationService(PaymentRepository paymentRepository, MessageService messageService)
{
  private readonly int PAYMENT_CHUNK = 1000000;
  private readonly PaymentRepository _paymentRepository = paymentRepository;
  private readonly MessageService _messageService = messageService;

  private async Task GenerateDBComparisonFile(DateTime date, string filePath, long bankId, int paymentsCounter)
  {
    int skip = 0;
    while (skip < paymentsCounter)
    {
      List<Payments> paymentsByPSP = await _paymentRepository.GetPaymentsByPSPInDate(date, bankId, skip, PAYMENT_CHUNK);
      using TextWriter file = new StreamWriter(filePath, true);
      foreach (var payment in paymentsByPSP)
      {
        ConcilliationFileContent content = new()
        {
          Id = payment.Id,
          Status = EnumHelper.MatchPaymentStatusToString(payment.Status)
        };
        string json = JsonSerializer.Serialize(content);
        file.WriteLine(json);
      }
      skip += PAYMENT_CHUNK;
    }
  }

  private static bool HasToCheckFileToDatabase(int lineCount, int paymentsCounter)
  {
    return lineCount == paymentsCounter;
  }

  private async Task<ConcilliationOutputDTO> GenerateFileToDatabaseAndDifferentStatusOutput(string PSPfile)
  {
    List<ConcilliationFileContent> fileToDatabase = [];
    List<ConcilliationPaymentId> differentStatus = [];

    using StreamReader fileReader = new(PSPfile);
    string? line;
    while ((line = fileReader.ReadLine()) != null)
    {
      ConcilliationFileContent? content = JsonSerializer.Deserialize<ConcilliationFileContent>(line);
      if (content is null)
        break;

      Payments? payment = await _paymentRepository.GetPaymentById(content.Id);
      if (payment is null)
        fileToDatabase.Add(content);

      else
      {
        PaymentStatus contentStatus = EnumHelper.MatchStringToPaymentStatus(content.Status);
        if (!payment.Status.Equals(contentStatus))
        {
          ConcilliationPaymentId concilliationPaymentId = new() { Id = payment.Id };
          differentStatus.Add(concilliationPaymentId);
        }
      }
    }

    ConcilliationOutputDTO outputDTO = new()
    {
      FileToDatabase = fileToDatabase.ToArray(),
      DifferentStatus = differentStatus.ToArray()
    };
    return outputDTO;
  }

  private static bool CheckDatabaseToFile(string PSPfile, ConcilliationFileContent dbContent)
  {
    using StreamReader pspFileReader = new(PSPfile);
    string? line;
    while ((line = pspFileReader.ReadLine()) != null)
    {
      ConcilliationFileContent? pspContent = JsonSerializer.Deserialize<ConcilliationFileContent>(line);
      if (pspContent is null)
        break;

      if (pspContent.Id.Equals(dbContent.Id))
        return false;
    }
    return true;
  }

  public async Task<ConcilliationOutputDTO> ConcilliationOutput(
    PaymentProvider? bankData,
    ConcilliationMessageServiceDTO dto
  )
  {
    string DBfile = $"./Services/tmp-{dto.Token}-{dto.Date.ToString("dd-MM-yyyy")}.json";
    if (File.Exists(DBfile))
      throw new ConcilliationInProgressException("Requisição para concilliação já foi feita. Por favor aguarde.");

    File.Open(DBfile, FileMode.Create).Close();

    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);
    int paymentsCounter = _paymentRepository.GetAllPaymentsByPSPCounterInDate(dto.Date, validBankData.Id);
    await GenerateDBComparisonFile(dto.Date, DBfile, validBankData.Id, paymentsCounter);

    int dbLineCount = 1;
    List<ConcilliationFileContent> databaseToFile = [];
    ConcilliationOutputDTO outputDTO = new();

    using StreamReader dbFileReader = new(DBfile);
    string? dbLine;
    while ((dbLine = dbFileReader.ReadLine()) != null)
    {
      ConcilliationFileContent? dbContent = JsonSerializer.Deserialize<ConcilliationFileContent>(dbLine);
      if (dbContent is null)
        break;

      if (HasToCheckFileToDatabase(dbLineCount, paymentsCounter))
      {
        ConcilliationOutputDTO checkOutput = await GenerateFileToDatabaseAndDifferentStatusOutput(dto.PSPfile);
        outputDTO.FileToDatabase = checkOutput.FileToDatabase;
        outputDTO.DifferentStatus = checkOutput.DifferentStatus;
      }

      bool PSPhasNoDbContent = CheckDatabaseToFile(dto.PSPfile, dbContent);
      if (PSPhasNoDbContent)
        databaseToFile.Add(dbContent);

      dbLineCount++;
    }

    outputDTO.DatabaseToFile = databaseToFile.ToArray();

    // Edge case: has content in PSPfile but no content in DBfile
    if (outputDTO.FileToDatabase is null)
    {
      ConcilliationOutputDTO checkOutput = await GenerateFileToDatabaseAndDifferentStatusOutput(dto.PSPfile);
      outputDTO.FileToDatabase = checkOutput.FileToDatabase;
      outputDTO.DifferentStatus = checkOutput.DifferentStatus;
    }

    if (File.Exists(DBfile))
      File.Delete(DBfile);

    return outputDTO;
  }

  public void ConcilliationRequest(PaymentProvider? bankData, ConcilliationRequestDTO dto)
  {
    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);
    if (!File.Exists(dto.File))
      throw new FileDoesNotExistException("Arquivo inválido ou não existente");

    string DBfile = $"./Services/tmp-{validBankData.Token}-{dto.Date.ToString("dd-MM-yyyy")}.json";
    if (File.Exists(DBfile))
      throw new ConcilliationInProgressException("Requisição para concilliação já foi feita. Por favor aguarde.");

    ConcilliationMessageServiceDTO messageDTO = new()
    {
      Token = validBankData.Token,
      Date = dto.Date,
      PSPfile = dto.File,
      Postback = dto.Postback,
    };
    _messageService.SendConcilliationMessage(messageDTO);
  }
}