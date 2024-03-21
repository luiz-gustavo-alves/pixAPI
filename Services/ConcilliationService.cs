
using System.Text.Json;
using pixAPI.DTOs;
using pixAPI.Exceptions;
using pixAPI.Helpers;
using pixAPI.Models;
using pixAPI.Repositories;

namespace pixAPI.Services;

public class ConcilliationService(PaymentRepository paymentRepository)
{
  private readonly int PAYMENT_CHUNK = 1000000;
  private readonly PaymentRepository _paymentRepository = paymentRepository;

  private async Task GenerateComparisonFile(string filePath, long bankId)
  {
    int paymentsCounter = _paymentRepository.GetAllTodayPaymentsByPSPCounter(bankId);
    int currentChunk = 0;
    while (currentChunk < paymentsCounter)
    {
      List<Payments> paymentsByPSP = await _paymentRepository.GetTodayPaymentsByPSP(bankId, currentChunk);
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
      currentChunk += PAYMENT_CHUNK;
    }
  }

  private async Task<ConcilliationOutputDTO> CheckFileToDatabaseAndPaymentStatus(string PSPfile)
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

  private static ConcilliationFileContent[] CheckDatabaseToFile(string DBfile, string PSPfile)
  {
    List<ConcilliationFileContent> databaseToFile = [];

    using StreamReader dbFileReader = new(DBfile);
    string? dbLine;
    while ((dbLine = dbFileReader.ReadLine()) != null)
    {
      ConcilliationFileContent? dbContent = JsonSerializer.Deserialize<ConcilliationFileContent>(dbLine);
      if (dbContent is null)
        break;

      bool PSPhasDbContent = false;
      using StreamReader pspFileReader = new(PSPfile);
      string? pspLine;
      while ((pspLine = pspFileReader.ReadLine()) != null)
      {
        ConcilliationFileContent? pspContent = JsonSerializer.Deserialize<ConcilliationFileContent>(pspLine);
        if (pspContent is null)
          break;
        
        if (pspContent.Id.Equals(dbContent.Id)) 
        {
          PSPhasDbContent = true;
          break;
        }
      }

      if (!PSPhasDbContent)
        databaseToFile.Add(dbContent);
    }

    return databaseToFile.ToArray();
  }

  public async Task<ConcilliationOutputDTO> ConcilliationCheck(PaymentProvider? bankData, ConcilliationCheckDTO dto)
  {
    PaymentProvider validBankData = ValidationHelper.ValidateBankDataOrFail(bankData);
    if (!File.Exists(dto.File))
      throw new FileDoesNotExistException("Arquivo inválido ou não existente");

    DateTime today = DateTime.Today;
    string DBfile = $"./Services/tmp-{validBankData.Token}-{today.ToString("dd-MM-yyyy")}.json";
    if (File.Exists(DBfile))
      File.Delete(DBfile);

    else
      File.Open(DBfile, FileMode.Create).Close();

    await GenerateComparisonFile(DBfile, validBankData.Id);
    ConcilliationOutputDTO outputDTO = await CheckFileToDatabaseAndPaymentStatus(dto.File);
    outputDTO.DatabaseToFile = CheckDatabaseToFile(DBfile, dto.File);
    return outputDTO;
  }
}