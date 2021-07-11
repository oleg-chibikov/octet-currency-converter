using System;
using System.IO;
using LiteDB;
using OlegChibikov.OctetInterview.CurrencyConverter.Contracts;

namespace OlegChibikov.OctetInterview.CurrencyConverter.DataAccessLayer
{
    public class ConversionRateRepository : IDisposable, IConversionRateRepository
    {
        readonly ILiteDatabase _liteDatabase;
        readonly ILiteCollection<ConversionRate> _liteCollection;

        public ConversionRateRepository()
        {
            var path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? string.Empty), @"CurrencyConverter.db");
            _liteDatabase = new LiteDatabase(path);
            _liteCollection = _liteDatabase.GetCollection<ConversionRate>("conversionRates");
        }

        public void SaveRate(string sourceCurrencyCode, string targetCurrencyCode, decimal rate)
        {
            _ = targetCurrencyCode ?? throw new ArgumentNullException(nameof(targetCurrencyCode));
            _ = sourceCurrencyCode ?? throw new ArgumentNullException(nameof(sourceCurrencyCode));

            var entity = new ConversionRate
            {
                LastUpdateTime = DateTime.Now,
                Rate = rate
            };

            _liteCollection.Upsert(GetCurrencyPair(sourceCurrencyCode, targetCurrencyCode), entity);
        }

        public ConversionRate? GetRate(string sourceCurrencyCode, string targetCurrencyCode)
        {
            _ = targetCurrencyCode ?? throw new ArgumentNullException(nameof(targetCurrencyCode));
            _ = sourceCurrencyCode ?? throw new ArgumentNullException(nameof(sourceCurrencyCode));

            return _liteCollection.FindById(GetCurrencyPair(sourceCurrencyCode, targetCurrencyCode));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _liteDatabase.Dispose();
            }
        }

        static string GetCurrencyPair(string sourceCurrencyCode, string targetCurrencyCode) => $"{sourceCurrencyCode}_{targetCurrencyCode}";
    }
}
