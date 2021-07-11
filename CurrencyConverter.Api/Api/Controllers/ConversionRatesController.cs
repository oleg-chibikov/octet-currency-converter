using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OlegChibikov.OctetInterview.CurrencyConverter.Api.Data;
using OlegChibikov.OctetInterview.CurrencyConverter.Contracts;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversionRatesController : ControllerBase, IDisposable
    {
        const string ConverterUriTemplate = "https://free.currconv.com/api/v7/convert?q={0}_{1}&compact=ultra&apiKey={2}";
        readonly IOptionsMonitor<AppSettings> _optionsMonitor;
        readonly HttpClient _httpClient;
        readonly IConversionRateRepository _conversionRateRepository;
        readonly ILogger<ConversionRatesController> _logger;

        // Used just for the fallback case when the API is unavailable
        readonly Random _randomRateGenerator = new ();

        public ConversionRatesController(IOptionsMonitor<AppSettings> optionsMonitor, HttpClient httpClient, IConversionRateRepository conversionRateRepository, ILogger<ConversionRatesController> logger)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _conversionRateRepository = conversionRateRepository ?? throw new ArgumentNullException(nameof(conversionRateRepository));
            _logger = logger;
        }

        [HttpGet("{sourceCurrencyCode}/{targetCurrencyCode}")]
        public async Task<decimal> GetAsync(string sourceCurrencyCode = "AUD", string targetCurrencyCode = "USD", CancellationToken cancellationToken = default)
        {
            sourceCurrencyCode.VerifyCurrencyCodeLength(nameof(sourceCurrencyCode));
            targetCurrencyCode.VerifyCurrencyCodeLength(nameof(targetCurrencyCode));

            if (sourceCurrencyCode == targetCurrencyCode)
            {
                return 1;
            }

            var cachedValue = _conversionRateRepository.GetRate(sourceCurrencyCode, targetCurrencyCode);
            if (cachedValue != null && cachedValue.LastUpdateTime.Add(_optionsMonitor.CurrentValue.CacheDuration) > DateTime.Now)
            {
                // If the cached value is not older than cache duration, return it without querying the external API
                return cachedValue.Rate.CalculateRateWithMarkup(_optionsMonitor.CurrentValue.MarkupPercentage);
            }

            var response = await _httpClient.GetAsync(
                    new Uri(string.Format(ConverterUriTemplate, sourceCurrencyCode, targetCurrencyCode, _optionsMonitor.CurrentValue.CurrencyConverterApiKey)),
                    cancellationToken)
                .ConfigureAwait(false);

            decimal value;
            try
            {
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var deserializedObject = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(jsonString);
                if (deserializedObject == null || !deserializedObject.TryGetValue($"{sourceCurrencyCode}_{targetCurrencyCode}", out value))
                {
                    throw new InvalidOperationException("There is no data for the requested currency pair");
                }
            }
            catch (HttpRequestException ex)
            {
                // just for the sake of the interview task, if the external api is not working - generate a random number
                _logger.LogWarning(ex, "Falling back to the random rate generation");
                value = Convert.ToDecimal(_randomRateGenerator.Next(900000, 1200000)) / 1000000;
            }

            _conversionRateRepository.SaveRate(sourceCurrencyCode, targetCurrencyCode, value);
            return value.CalculateRateWithMarkup(_optionsMonitor.CurrentValue.MarkupPercentage);
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
                _httpClient.Dispose();
            }
        }
    }
}