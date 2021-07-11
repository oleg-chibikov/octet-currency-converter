using System;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Api.Data
{
    public class AppSettings
    {
        public string? WebAppHost { get; set; }

        public string? CurrencyConverterApiKey { get; set; }

        public double MarkupPercentage { get; set; } = 0.1;

        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(1);
    }
}
