using System;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Contracts
{
    public static class CurrencyCodeExtensions
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
    }
}
