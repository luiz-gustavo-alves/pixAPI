using pixAPI.Models;
using pixAPI.Exceptions;

namespace pixAPI.Helpers;

public class EnumHelper
{
  public static KeyType MatchStringToKeyType(string type) 
  {
    try {
      KeyType matchValue = (KeyType)Enum.Parse(typeof(KeyType), type);
      return matchValue;
    } catch {
      throw new InvalidEnumException("Tipo inválido de chave Pix");
    }
  }

  public static PaymentStatus MatchStringToPaymentStatus(string status) 
  {
    try {
      PaymentStatus matchValue = (PaymentStatus)Enum.Parse(typeof(PaymentStatus), status);
      return matchValue;
    } catch {
      throw new InvalidEnumException("Tipo inválido de Status de pagamento");
    }
  }

  public static string MatchPaymentStatusToString(PaymentStatus status) 
  {
    string? result = Enum.GetName(status);
    if (result is null)
      throw new InvalidEnumException("Tipo inválido de Status de pagamento");

    return result;
  }
}