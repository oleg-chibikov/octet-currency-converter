using System;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Contracts
{
    public static class CurrencyUtilities
    {
        const int CurrencyCodeLength = 3;

        public static void VerifyCurrencyCodeLength(this string currencyCode, string name)
        {
            _ = currencyCode ?? throw new ArgumentNullException(nameof(currencyCode));

            if (currencyCode.Length != CurrencyCodeLength)
            {
                throw new BusinessException(name + " length should be 3");
            }
        }

        public static decimal CalculateRateWithMarkup(this decimal rate, decimal markup)
        {
            return Math.Round(rate + (rate * markup), 6);
        }
    }
}
