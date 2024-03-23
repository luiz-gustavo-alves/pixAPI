using System.Text.Json;
using pixAPI.DTOs;
using pixAPI.Exceptions;
using pixAPI.Helpers;
using pixAPI.Models;
using pixAPI.Repositories;

namespace pixAPI.Services;

public class ConcilliationService(PaymentRepository paymentRepository, MessageService messageService)
{
  private readonly int DB_CHUNK = 100000;
  private readonly PaymentRepository _paymentRepository = paymentRepository;
  private readonly MessageService _messageService = messageService;

  private async Task GenerateDBComparisonFile(DateTime date, string filePath, long bankId)
  {
    int paymentsCounter = _paymentRepository.GetAllPaymentsByPSPCounterInDate(date, bankId);
    int skip = 0;
    while (skip < paymentsCounter)
    {
      List<Payments> paymentsByPSP = await _paymentRepository.GetPaymentsByPSPInDate(date, bankId, skip, DB_CHUNK);
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
      skip += DB_CHUNK;
    }
  }

  private static List<ConcilliationFileContent> CheckDatabaseToFile(string PSPfile, Dictionary<long, string> dbDict)
  {
    List<ConcilliationFileContent> results = [];
    using StreamReader pspFileReader = new(PSPfile);
    string? line;
    while ((line = pspFileReader.ReadLine()) != null)
    {
      ConcilliationFileContent? pspContent = JsonSerializer.Deserialize<ConcilliationFileContent>(line);
      if (pspContent is null)
        break;

      dbDict.Remove(pspContent.Id);
    }

    foreach (var item in dbDict)
    {
      results.Add(new() { Id = item.Key, Status = item.Value });
    }

    return results;
  }

  public async Task<ConcilliationOutputDTO> ConcilliationOutput(PaymentProvider? bankData, ConcilliationMessageServiceDTO dto)
  {
    string DBfile = $"./Services/tmp-{dto.Token}-{dto.Date.ToString("dd-MM-yyyy")}.json";
    if (File.Exists(DBfile))
      throw new ConcilliationInProgressException("Requisição para concilliação já foi feita. Por favor aguarde.");

    File.Open(DBfile, FileMode.Create).Close();

    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);
    await GenerateDBComparisonFile(dto.Date, DBfile, validBankData.Id);

    List<ConcilliationFileContent> databaseToFile = [];
    List<ConcilliationFileContent> fileToDatabase = [];
    List<ConcilliationPaymentId> differentStatus = [];

    Dictionary<long, string> dbDict = new Dictionary<long, string>();
    int dbLineCount = 0;

    using StreamReader dbFileReader = new(DBfile);
    string? dbLine;
    while (true)
    {
      dbLine = dbFileReader.ReadLine();
      if (dbLine is null)
      {
        List<ConcilliationFileContent> results = CheckDatabaseToFile(dto.PSPfile, dbDict);
        databaseToFile.AddRange(results);
        break;
      }

      ConcilliationFileContent? dbContent = JsonSerializer.Deserialize<ConcilliationFileContent>(dbLine);
      if (dbContent is null)
        break;

      if (dbLineCount < DB_CHUNK)
      {
        dbDict.Add(dbContent.Id, dbContent.Status);
        dbLineCount++;
      }
      else
      {
        List<ConcilliationFileContent> results = CheckDatabaseToFile(dto.PSPfile, dbDict);
        databaseToFile.AddRange(results);
        dbDict.Clear();
        dbLineCount = 0;
      }
    }
    dbDict.Clear();

    using StreamReader fileReader = new(dto.PSPfile);
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
          differentStatus.Add(new() { Id = payment.Id });
      }
    }

    ConcilliationOutputDTO outputDTO = new()
    {
      DatabaseToFile = databaseToFile.ToArray(),
      FileToDatabase = fileToDatabase.ToArray(),
      DifferentStatus = differentStatus.ToArray(),
    };

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