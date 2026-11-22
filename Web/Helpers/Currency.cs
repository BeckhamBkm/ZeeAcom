using System.Globalization;

namespace ZeeAcom.Web.Helpers;

public static class Currency
{
    private static readonly CultureInfo SouthAfrica = CultureInfo.GetCultureInfo("en-ZA");

    /// <summary>Formats an amount in rand, e.g. 2500 -> "R2 500" (cents shown only when there are any).</summary>
    public static string Rand(decimal amount)
        => amount.ToString(amount == decimal.Truncate(amount) ? "C0" : "C2", SouthAfrica);
}
