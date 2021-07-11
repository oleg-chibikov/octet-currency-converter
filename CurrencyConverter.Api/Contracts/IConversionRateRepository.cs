namespace OlegChibikov.OctetInterview.CurrencyConverter.Contracts
{
    public interface IConversionRateRepository
    {
        void SaveRate(string sourceCurrencyCode, string targetCurrencyCode, decimal rate);

        ConversionRate? GetRate(string sourceCurrencyCode, string targetCurrencyCode);
    }
}