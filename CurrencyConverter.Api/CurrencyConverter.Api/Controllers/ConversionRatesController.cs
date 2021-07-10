using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyConverter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversionRatesController : ControllerBase
    {
        private readonly ILogger<ConversionRatesController> _logger;

        public ConversionRatesController(ILogger<ConversionRatesController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
