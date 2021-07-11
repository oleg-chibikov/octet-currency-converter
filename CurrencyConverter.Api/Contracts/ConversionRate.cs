using System;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Contracts
{
    public class ConversionRate
    {
        // For ORM
        public ConversionRate()
        {
        }

        public ConversionRate(decimal rate, DateTime lastUpdateTime)
        {
            Rate = rate;
            LastUpdateTime = lastUpdateTime;
        }

        public decimal Rate { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
