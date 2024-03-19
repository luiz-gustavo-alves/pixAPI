using pixAPI.Models;
using pixAPI.Repositories;
using pixAPI.Exceptions;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace pixAPI.BLLs;

public class PixKeyBLL
{
  public static void ValidatePixKeyValue(KeyType keyType, string value, string CPF)
  {
    switch (keyType)
    {
      case KeyType.CPF:
        Regex cpfRegex = new(@"\d{11}");
        if (!cpfRegex.IsMatch(value) || !CPF.Equals(value))
          throw new BadRequestException("Valor inválido para chave Pix do tipo CPF.");
        break;

      case KeyType.Email:
        if (!MailAddress.TryCreate(value, out _))
          throw new BadRequestException("Valor inválido para chave Pix do tipo Email.");
        break;

      case KeyType.Phone:
        int MAX_PHONE_LENGTH = 11;
        Regex phoneRegex = new(@"^\(?(?:[14689][1-9]|2[12478]|3[1234578]|5[1345]|7[134579])\)? ?(?:[2-8]|9[0-9])[0-9]{3}\-?[0-9]{4}$");
        if (!phoneRegex.IsMatch(value) || value.Length != MAX_PHONE_LENGTH)
          throw new BadRequestException("Valor inválido para chave Pix do tipo Celular.");
        break;

      case KeyType.Random:
        if (!Guid.TryParse(value, out _))
          throw new BadRequestException("Valor inválido para chave Pix do tipo Aleatória.");
        break;
    }
  }

  public static async Task ValidatePixKeyValueConflict(PixKeyRepository pixKeyRepository, string value)
  {
    PixKey? pixKey = await pixKeyRepository.GetPixKeyByValue(value);
    if (pixKey is not null)
      throw new ConflictException("Já existe uma chave Pix associada a este valor.");
  }

  public static void ValidatePixKeyCreationLimit(List<PixKey> pixKeys, int limit)
  {
    if (pixKeys.Count >= limit)
      throw new CannotProceedPixKeyCreationException("Limite excedido para criação de chave Pix");
  }
}