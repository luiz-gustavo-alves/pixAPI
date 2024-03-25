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

  private static List<ConcilliationFileContent> CheckDatabaseToFile(
    string PSPfile,
    Dictionary<long, string> dbDict, 
    int dictCounter
  )
  {
    List<ConcilliationFileContent> results = [];
    using StreamReader fileReader = new(PSPfile);
    string? PSPline;
    while (dictCounter == 0)
    {
      PSPline = fileReader.ReadLine();
      if (PSPline is null)
        break;

      ConcilliationFileContent? PSPcontent = JsonSerializer.Deserialize<ConcilliationFileContent>(PSPline);
      if (PSPcontent is null)
        break;

      if (dbDict.ContainsKey(PSPcontent.Id)) 
      {
        dbDict.Remove(PSPcontent.Id);
        dictCounter--;
      }
    }

    foreach (var item in dbDict)
    {
      results.Add(new() { Id = item.Key, Status = item.Value });
    }
    return results;
  }

  private async Task<ConcilliationListOutput> CheckFileToDatabaseAndDifferentStatus(Dictionary<long, string> pspDict)
  {
    List<ConcilliationFileContent> fileToDatabase = [];
    List<ConcilliationPaymentId> differentStatus = [];

    List<Payments> payments = await _paymentRepository.GetPaymentByDictOfIds(pspDict);
    foreach (var payment in payments)
    {
      if (pspDict.TryGetValue(payment.Id, out string? status))
      {
        PaymentStatus statusFromDict = EnumHelper.MatchStringToPaymentStatus(status);
        if (!payment.Status.Equals(statusFromDict))
          differentStatus.Add(new() { Id = payment.Id });

        pspDict.Remove(payment.Id);
      }
    }

    foreach (var item in pspDict)
    {
      fileToDatabase.Add(new() { Id = item.Key, Status = item.Value });
    }

    ConcilliationListOutput result = new() { FileToDatabase = fileToDatabase, DifferentStatus = differentStatus };
    return result;
  }

  private async Task<ConcilliationListOutput> GetDatabaseToFileConcilliation(long bankId, ConcilliationMessageServiceDTO dto)
  {
    List<ConcilliationFileContent> databaseToFile = [];
    Dictionary<long, string> dbDict = new Dictionary<long, string>();
    int paymentsCounter = _paymentRepository.GetAllPaymentsByPSPCounterInDate(dto.Date, bankId);
    int skip = 0;

    while (skip <= paymentsCounter)
    {
      List<Payments> dbPayments = await _paymentRepository.GetPaymentsByPSPInDate(dto.Date, bankId, skip, DB_CHUNK);
      foreach (var payment in dbPayments)
      {
        dbDict.Add(payment.Id, EnumHelper.MatchPaymentStatusToString(payment.Status));
      }

      List<ConcilliationFileContent> results = CheckDatabaseToFile(dto.PSPfile, dbDict, dbPayments.Count);
      databaseToFile.AddRange(results);
      dbDict.Clear();
      skip += DB_CHUNK;
    }

    ConcilliationListOutput result = new() { DatabaseToFile = databaseToFile };
    return result;
  }


  private async Task<ConcilliationListOutput> GetFileToDatabaseAndDifferentStatusConcilliation(
    ConcilliationMessageServiceDTO dto
  )
  {
    List<ConcilliationFileContent> fileToDatabase = [];
    List<ConcilliationPaymentId> differentStatus = [];
    Dictionary<long, string> pspDict = new Dictionary<long, string>();
    int pspLineCount = 0;

    using StreamReader fileReader = new(dto.PSPfile);
    string? PSPline;
    while (true)
    {
      PSPline = fileReader.ReadLine();
      if (PSPline is null)
      {
        ConcilliationListOutput output = await CheckFileToDatabaseAndDifferentStatus(pspDict);
        fileToDatabase.AddRange(output.FileToDatabase);
        differentStatus.AddRange(output.DifferentStatus);
        break;
      }

      ConcilliationFileContent? PSPcontent = JsonSerializer.Deserialize<ConcilliationFileContent>(PSPline);
      if (PSPcontent is null)
        break;

      pspDict.Add(PSPcontent.Id, PSPcontent.Status);
      if (pspLineCount < DB_CHUNK)
      {
        pspLineCount++;
      }
      else
      {
        ConcilliationListOutput output = await CheckFileToDatabaseAndDifferentStatus(pspDict);
        fileToDatabase.AddRange(output.FileToDatabase);
        differentStatus.AddRange(output.DifferentStatus);
        pspLineCount = 0;
        pspDict.Clear();
      }
    }

    ConcilliationListOutput result = new() { FileToDatabase = fileToDatabase, DifferentStatus = differentStatus };
    return result;
  }

  public async Task<ConcilliationOutputDTO> ConcilliationOutput(PaymentProvider? bankData, ConcilliationMessageServiceDTO dto)
  {
    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);
    string tempFile = $"./Services/tmp-{validBankData.Token}-{dto.Date.ToString("dd-MM-yyyy")}.json";
    if (File.Exists(tempFile))
      throw new ConcilliationInProgressException("Requisição para concilliação já foi feita. Por favor aguarde.");

    File.Open(tempFile, FileMode.Create).Close();
    /* Make request to read a file from a URL of external site and generate tmp file (PSP file) */
    // await GenerateTempFile(dto.Date, tempFile, validBankData.Id);

    ConcilliationListOutput dbConcilliation = await GetDatabaseToFileConcilliation(validBankData.Id, dto);
    ConcilliationListOutput pspConcilliation = await GetFileToDatabaseAndDifferentStatusConcilliation(dto);
    ConcilliationOutputDTO outputDTO = new()
    {
      DatabaseToFile = dbConcilliation.DatabaseToFile.ToArray(),
      FileToDatabase = pspConcilliation.FileToDatabase.ToArray(),
      DifferentStatus = pspConcilliation.DifferentStatus.ToArray(),
    };

    if (File.Exists(tempFile))
      File.Delete(tempFile);

    return outputDTO;
  }

  public void ConcilliationRequest(PaymentProvider? bankData, ConcilliationRequestDTO dto)
  {
    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);
    if (!File.Exists(dto.File))
      throw new FileDoesNotExistException("Arquivo inválido ou não existente");

    string tempFile = $"./Services/tmp-{validBankData.Token}-{dto.Date.ToString("dd-MM-yyyy")}.json";
    if (File.Exists(tempFile))
      throw new ConcilliationInProgressException("Requisição para concilliação já foi feita. Por favor aguarde.");

    ConcilliationMessageServiceDTO messageDTO = new()
    {
      Token = validBankData.Token,
      Date = dto.Date,
      PSPfile = dto.File,
      Postback = dto.Postback,
    };

    try
    {
      _messageService.SendConcilliationMessage(messageDTO);
    }
    catch
    {
      throw new ServiceUnavailableException("Serviço indisponível. Tente novamente mais tarde");
    }
  }

  public void ConcilliationFinish(PaymentProvider? bankData, ConcilliationMessageServiceDTO dto)
  {
    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);
    string tempFile = $"./Services/tmp-{validBankData.Token}-{dto.Date.ToString("dd-MM-yyyy")}.json";
    if (File.Exists(tempFile))
      File.Delete(tempFile);
  }
}