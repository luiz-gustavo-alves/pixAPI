using pixAPI.Models;
using pixAPI.Exceptions;

namespace pixAPI.Helpers;

public class EnumHelper
{
  public static KeyType MatchStringToKeyType(string type) 
  {
    try {
      KeyType matchValue = (KeyType)Enum.Parse(typeof(KeyType), type, true);
      return matchValue;
    } catch {
      throw new InvalidKeyTypeException("Tipo Inválido de Chave Pix");
    }
  }
}