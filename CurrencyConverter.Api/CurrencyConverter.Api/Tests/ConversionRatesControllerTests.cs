using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OlegChibikov.OctetInterview.CurrencyConverter.Api.Controllers;
using OlegChibikov.OctetInterview.CurrencyConverter.Api.Data;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Tests
{
    public class ConversionRatesControllerTests
    {
        const string SourceCurrencyCode = "AUD";
        const string TargetCurrencyCode = "USD";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Get_AppliesMarkup()
        {
            // Arrange
            const double markupPercentage = 0.2;
            const double conversionRate = 0.9;
            var optionsMonitor = GetMockOptionsMonitor(markupPercentage);
            var httpClient = GetMockHttpClient(SourceCurrencyCode, TargetCurrencyCode, conversionRate);

            var sut = new ConversionRatesController(optionsMonitor, httpClient);

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, TargetCurrencyCode, default).ConfigureAwait(false);

            // Assert
            result.Should().Be(conversionRate * markupPercentage);
        }

        static HttpClient GetMockHttpClient(string sourceCurrencyCode, string targetCurrencyCode, double conversionRate)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent($"{{\"{sourceCurrencyCode}_{targetCurrencyCode}\":{conversionRate}}}"),
                    })
                .Verifiable();

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };
        }

        static IOptionsMonitor<Settings> GetMockOptionsMonitor(double markupPercentage)
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<Settings>>();
            optionsMonitorMock.SetupGet(x => x.CurrentValue).Returns(new Settings { MarkupPercentage = markupPercentage });
            return optionsMonitorMock.Object;
        }
    }
}