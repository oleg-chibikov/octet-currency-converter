using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OlegChibikov.OctetInterview.CurrencyConverter.Api.Data;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversionRatesController : ControllerBase, IDisposable
    {
        const string ConverterUriTemplate = "https://free.currconv.com/api/v7/convert?q={0}_{1}&compact=ultra&apiKey={2}";
        const int CurrencyCodeLength = 3;
        readonly IOptionsMonitor<Settings> _optionsMonitor;
        readonly HttpClient _httpClient;

        public ConversionRatesController(IOptionsMonitor<Settings> optionsMonitor, HttpClient httpClient)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        [HttpGet("{sourceCurrencyCode}/{targetCurrencyCode}")]
        public async Task<double> GetAsync(string sourceCurrencyCode = "AUD", string targetCurrencyCode = "USD", CancellationToken cancellationToken = default)
        {
            _ = targetCurrencyCode ?? throw new ArgumentNullException(nameof(targetCurrencyCode));
            _ = sourceCurrencyCode ?? throw new ArgumentNullException(nameof(sourceCurrencyCode));

            void VerifyCurrencyCodeLength(string currencyCode)
            {
                if (currencyCode.Length != CurrencyCodeLength)
                {
                    throw new ArgumentException(nameof(sourceCurrencyCode) + " length should be 3");
                }
            }

            VerifyCurrencyCodeLength(sourceCurrencyCode);
            VerifyCurrencyCodeLength(targetCurrencyCode);

            var response = await _httpClient.GetAsync(
                    new Uri(string.Format(ConverterUriTemplate, sourceCurrencyCode, targetCurrencyCode, _optionsMonitor.CurrentValue.CurrencyConverterApiKey)),
                    cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var deserializedObject = JsonConvert.DeserializeObject<Dictionary<string, double>>(jsonString);
            if (deserializedObject != null && deserializedObject.TryGetValue($"{sourceCurrencyCode}_{targetCurrencyCode}", out var value))
            {
                return value + (value * _optionsMonitor.CurrentValue.MarkupPercentage);
            }

            throw new InvalidOperationException("Incorrect response from external API");
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