using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OlegChibikov.OctetInterview.CurrencyConverter.Api.Data;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversionRatesController : ControllerBase, IDisposable
    {
        readonly IOptionsMonitor<Settings> _optionsMonitor;
        readonly HttpClient _httpClient;

        public ConversionRatesController(IOptionsMonitor<Settings> optionsMonitor, HttpClient httpClient)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        [HttpGet("{sourceCurrencyCode}/{targetCurrencyCode}")]
        public double Get(string sourceCurrencyCode, string targetCurrencyCode)
        {
            var markup = _optionsMonitor.CurrentValue.MarkupPercentage;
            return 1 * markup;
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
