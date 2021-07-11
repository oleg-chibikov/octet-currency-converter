using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
        const decimal MarkupPercentage = 0.2M;
        const decimal ConversionRate = 0.9M;
        const decimal CachedRate = 0.6M;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Get_ReturnsOne_When_SourceCurrencyEqualsToTarget()
        {
            // Arrange
            var optionsMonitor = CreateMockOptionsMonitor(MarkupPercentage);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, SourceCurrencyCode, ConversionRate);
            var repository = CreateMockRepository();

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository, Mock.Of<ILogger<ConversionRatesController>>());

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, SourceCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(1);
        }

        [Test]
        public async Task Get_AppliesMarkup()
        {
            // Arrange
            var optionsMonitor = CreateMockOptionsMonitor(MarkupPercentage);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, TargetCurrencyCode, ConversionRate);
            var repository = CreateMockRepository();

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository, Mock.Of<ILogger<ConversionRatesController>>());

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, TargetCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(ConversionRate.CalculateRateWithMarkup(MarkupPercentage));
        }

        [Test]
        public async Task Get_ReturnsNewValue_When_LastUpdateIsWithinCacheDuration_And_IsForDifferentCurrencyPair()
        {
            // Arrange
            var cacheDuration = TimeSpan.FromHours(3);
            var optionsMonitor = CreateMockOptionsMonitor(MarkupPercentage, cacheDuration);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, TargetCurrencyCode, ConversionRate);

            var repository = CreateMockRepository(cacheDuration.Add(TimeSpan.FromSeconds(-1)), CachedRate, "RUR", "EUR");

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository, Mock.Of<ILogger<ConversionRatesController>>());

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, TargetCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(ConversionRate.CalculateRateWithMarkup(MarkupPercentage));
        }

        [Test]
        public async Task Get_ReturnsCachedValue_When_LastUpdateIsWithinCacheDuration()
        {
            // Arrange
            var cacheDuration = TimeSpan.FromHours(3);
            var optionsMonitor = CreateMockOptionsMonitor(MarkupPercentage, cacheDuration);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, TargetCurrencyCode, ConversionRate);

            var repository = CreateMockRepository(cacheDuration.Add(TimeSpan.FromSeconds(-1)), CachedRate);

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository, Mock.Of<ILogger<ConversionRatesController>>());

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, TargetCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(CachedRate.CalculateRateWithMarkup(MarkupPercentage));
        }

        [Test]
        public async Task Get_ReturnsNewValue_When_LastUpdateIsOutsideCacheDuration()
        {
            // Arrange
            var cacheDuration = TimeSpan.FromHours(3);
            var optionsMonitor = CreateMockOptionsMonitor(MarkupPercentage, cacheDuration);
            var httpClient = CreateMockHttpClient(SourceCurrencyCode, TargetCurrencyCode, ConversionRate);

            var repository = CreateMockRepository(cacheDuration.Add(TimeSpan.FromSeconds(1)), CachedRate);

            var sut = new ConversionRatesController(optionsMonitor, httpClient, repository, Mock.Of<ILogger<ConversionRatesController>>());

            // Act
            var result = await sut.GetAsync(SourceCurrencyCode, TargetCurrencyCode).ConfigureAwait(false);

            // Assert
            result.Should().Be(ConversionRate.CalculateRateWithMarkup(MarkupPercentage));
        }

        static HttpClient CreateMockHttpClient(string sourceCurrencyCode, string targetCurrencyCode, decimal conversionRate)
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
            decimal? rate = null,
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

        static IOptionsMonitor<AppSettings> CreateMockOptionsMonitor(decimal markupPercentage = 0.1M, TimeSpan? cacheDuration = null)
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<AppSettings>>();
            optionsMonitorMock.SetupGet(x => x.CurrentValue).Returns(new AppSettings { CacheDuration = cacheDuration ?? TimeSpan.FromHours(1), MarkupPercentage = markupPercentage });
            return optionsMonitorMock.Object;
        }
    }
}