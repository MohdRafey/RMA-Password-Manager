namespace RMA.Windows.Validator
{
  public static class LoginValidator
  {

    public class ValidationResult
    {
      public bool IsIdInvalid { get; set; }
      public bool IsPinInvalid { get; set; }
      public string Message { get; set; } = string.Empty;
      public bool IsSuccess => !IsIdInvalid && !IsPinInvalid;
    }

    public static ValidationResult ValidateLogin(string userId, string pin)
    {
      var result = new ValidationResult();

      if (string.IsNullOrWhiteSpace(userId))
      {
        result.IsIdInvalid = true;
        result.Message = "User ID cannot be empty.";
      }

      if (string.IsNullOrWhiteSpace(pin))
      {
        result.IsPinInvalid = true;
        result.Message = result.IsIdInvalid ? "Required fields missing." : "PIN cannot be empty.";
      }
      else if (pin.Length != 6 || !pin.All(char.IsDigit))
      {
        result.IsPinInvalid = true;
        result.Message = "PIN must be exactly 6 digits.";
      }

      return result;
    }

    public static (bool IsValid, string Message) ValidateNewVault(string vaultName, string pin, string confirmPin)
    {
      if (string.IsNullOrWhiteSpace(vaultName))
        return (false, "Vault name is required.");

      if (pin != confirmPin)
        return (false, "PINs do not match.");

      if (pin.Length != 6)
        return (false, "PIN must be 6 digits.");

      return (true, string.Empty);
    }
  }
}