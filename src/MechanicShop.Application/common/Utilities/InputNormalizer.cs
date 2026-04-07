namespace MechanicShop.Application.Common.Utilities;

public static class InputNormalizer
{
	public static string NormalizeText(string value)
	{
		return value.Trim();
	}

	public static string NormalizeEmail(string email)
	{
		return NormalizeText(email).ToLowerInvariant();
	}
}