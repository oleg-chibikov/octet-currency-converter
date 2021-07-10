using System;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Contracts
{
    public class ConversionRate
    {
        // For ORM
        public ConversionRate()
        {
        }

        public ConversionRate(double rate, DateTime lastUpdateTime)
        {
            Rate = rate;
            LastUpdateTime = lastUpdateTime;
        }

        public double Rate { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
