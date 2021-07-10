using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        readonly IOptionsMonitor<Settings> _optionsMonitor;
        readonly HttpClient _httpClient;
        readonly IConversionRateRepository _conversionRateRepository;

        public ConversionRatesController(IOptionsMonitor<Settings> optionsMonitor, HttpClient httpClient, IConversionRateRepository conversionRateRepository)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _conversionRateRepository = conversionRateRepository ?? throw new ArgumentNullException(nameof(conversionRateRepository));
        }

        [HttpGet("{sourceCurrencyCode}/{targetCurrencyCode}")]
        public async Task<double> GetAsync(string sourceCurrencyCode = "AUD", string targetCurrencyCode = "USD", CancellationToken cancellationToken = default)
        {
            sourceCurrencyCode.VerifyCurrencyCodeLength(nameof(sourceCurrencyCode));
            targetCurrencyCode.VerifyCurrencyCodeLength(nameof(targetCurrencyCode));

            if (sourceCurrencyCode == targetCurrencyCode)
            {
                return 1;
            }

            double CalculateRateWithMarkup(double rate)
            {
                return rate + (rate * _optionsMonitor.CurrentValue.MarkupPercentage);
            }

            var cachedValue = _conversionRateRepository.GetRate(sourceCurrencyCode, targetCurrencyCode);
            if (cachedValue != null && cachedValue.LastUpdateTime.Add(_optionsMonitor.CurrentValue.CacheDuration) > DateTime.Now)
            {
                // If the cached value is not older than cache duration, return it without querying the external API
                return CalculateRateWithMarkup(cachedValue.Rate);
            }

            var response = await _httpClient.GetAsync(
                    new Uri(string.Format(ConverterUriTemplate, sourceCurrencyCode, targetCurrencyCode, _optionsMonitor.CurrentValue.CurrencyConverterApiKey)),
                    cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var deserializedObject = JsonConvert.DeserializeObject<Dictionary<string, double>>(jsonString);
            if (deserializedObject != null && deserializedObject.TryGetValue($"{sourceCurrencyCode}_{targetCurrencyCode}", out var value))
            {
                _conversionRateRepository.SaveRate(sourceCurrencyCode, targetCurrencyCode, value);
                return CalculateRateWithMarkup(value);
            }

            throw new InvalidOperationException("There is no data for the requested currency pair");
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