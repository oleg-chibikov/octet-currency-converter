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
using OlegChibikov.OctetInterview.CurrencyConverter.Contracts;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Tests
{
    public class ConversionRatesControllerTests
    {
        const string SourceCurrencyCode = "NZD";
        const string TargetCurrencyCode = "JPY";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Get_ReturnsOne_When_SourceCurrencyEqualsToTarget()
        {
            // Arrange
            const double markupPercentage = 0.2;
            const double conversionRate = 0.9;
            var optionsMonitor = CreateMockOptionsMonitor(markupPercentage);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, SourceCurrencyCode, conversionRate);
            var repository = CreateMockRepository();

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository);

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, SourceCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(1);
        }

        [Test]
        public async Task Get_AppliesMarkup()
        {
            // Arrange
            const double markupPercentage = 0.2;
            const double conversionRate = 0.9;
            var optionsMonitor = CreateMockOptionsMonitor(markupPercentage);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, TargetCurrencyCode, conversionRate);
            var repository = CreateMockRepository();

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository);

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, TargetCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(conversionRate + (conversionRate * markupPercentage));
        }

        [Test]
        public async Task Get_ReturnsNewValue_When_LastUpdateIsWithinCacheDuration_And_IsForDifferentCurrencyPair()
        {
            // Arrange
            const double markupPercentage = 0.2;
            const double newRate = 0.9;
            const double cachedRate = 0.6;
            var cacheDuration = TimeSpan.FromHours(3);
            var optionsMonitor = CreateMockOptionsMonitor(markupPercentage, cacheDuration);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, TargetCurrencyCode, newRate);

            var repository = CreateMockRepository(cacheDuration.Add(TimeSpan.FromSeconds(-1)), cachedRate, "RUR", "EUR");

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository);

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, TargetCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(CalculateRateWithMarkup(newRate, markupPercentage));
        }

        [Test]
        public async Task Get_ReturnsCachedValue_When_LastUpdateIsWithinCacheDuration()
        {
            // Arrange
            const double markupPercentage = 0.2;
            const double newRate = 0.9;
            const double cachedRate = 0.6;
            var cacheDuration = TimeSpan.FromHours(3);
            var optionsMonitor = CreateMockOptionsMonitor(markupPercentage, cacheDuration);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, TargetCurrencyCode, newRate);

            var repository = CreateMockRepository(cacheDuration.Add(TimeSpan.FromSeconds(-1)), cachedRate);

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository);

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, TargetCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(CalculateRateWithMarkup(cachedRate, markupPercentage));
        }

        [Test]
        public async Task Get_ReturnsNewValue_When_LastUpdateIsOutsideCacheDuration()
        {
            // Arrange
            const double markupPercentage = 0.2;
            const double newRate = 0.9;
            const double cachedRate = 0.6;
            var cacheDuration = TimeSpan.FromHours(3);
            var optionsMonitor = CreateMockOptionsMonitor(markupPercentage, cacheDuration);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, TargetCurrencyCode, newRate);

            var repository = CreateMockRepository(cacheDuration.Add(TimeSpan.FromSeconds(1)), cachedRate);

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository);

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, TargetCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(CalculateRateWithMarkup(newRate, markupPercentage));
        }

        static HttpClient CreateMockHttpClient(string sourceCurrencyCode, string targetCurrencyCode, double conversionRate)
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

        static IConversionRateRepository CreateMockRepository(
            TimeSpan? lastUpdateTimeDifferenceWithNow = null,
            double? rate = null,
            string sourceCurrencyCode = SourceCurrencyCode,
            string targetCurrencyCode = TargetCurrencyCode)
        {
            var conversionRateRepositoryMock = new Mock<IConversionRateRepository>();
            if (lastUpdateTimeDifferenceWithNow != null && rate != null)
            {
                conversionRateRepositoryMock.Setup(x => x.GetRate(sourceCurrencyCode, targetCurrencyCode))
                    .Returns<string, string>((_, _) => new ConversionRate(rate.Value, DateTime.Now - lastUpdateTimeDifferenceWithNow.Value));
            }

            return conversionRateRepositoryMock.Object;
        }

        static IOptionsMonitor<Settings> CreateMockOptionsMonitor(double markupPercentage = 0.1, TimeSpan? cacheDuration = null)
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<Settings>>();
            optionsMonitorMock.SetupGet(x => x.CurrentValue).Returns(new Settings { CacheDuration = cacheDuration ?? TimeSpan.FromHours(1), MarkupPercentage = markupPercentage });
            return optionsMonitorMock.Object;
        }

        static double CalculateRateWithMarkup(double value, double markup)
        {
            return value + (value * markup);
        }
    }
}