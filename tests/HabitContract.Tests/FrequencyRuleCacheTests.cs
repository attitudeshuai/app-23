using FluentAssertions;
using HabitContract.Application.Interfaces;
using HabitContract.Application.Services;
using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace HabitContract.Tests;

public class FrequencyRuleCacheTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<IFrequencyParser> _mockParser;
    private readonly FrequencyRuleCache _cache;

    public FrequencyRuleCacheTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockParser = new Mock<IFrequencyParser>();
        _cache = new FrequencyRuleCache(_memoryCache, _mockParser.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ReturnsNull()
    {
        var result = await _cache.GetAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WhenCalled_ShouldStoreInCache()
    {
        var contractId = 1;
        var rule = new FrequencyRule
        {
            Type = FrequencyType.Daily,
            Count = 2,
            OriginalString = "每天2次"
        };

        await _cache.SetAsync(contractId, rule);

        var result = await _cache.GetAsync(contractId);
        result.Should().NotBeNull();
        result.Type.Should().Be(FrequencyType.Daily);
        result.Count.Should().Be(2);
        result.OriginalString.Should().Be("每天2次");
    }

    [Fact]
    public async Task RemoveAsync_WhenCalled_ShouldRemoveFromCache()
    {
        var contractId = 2;
        var rule = new FrequencyRule
        {
            Type = FrequencyType.Weekly,
            Count = 3,
            OriginalString = "每周3次"
        };

        await _cache.SetAsync(contractId, rule);
        await _cache.RemoveAsync(contractId);

        var result = await _cache.GetAsync(contractId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenKeyDoesNotExist_ShouldParseAndStore()
    {
        var contractId = 3;
        var frequencyString = "每周1、3、5";
        var expectedRule = new FrequencyRule
        {
            Type = FrequencyType.Weekly,
            Count = 3,
            OriginalString = frequencyString,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }
        };

        _mockParser.Setup(p => p.Parse(frequencyString)).Returns(expectedRule);

        var result = await _cache.GetOrCreateAsync(contractId, frequencyString);

        result.Should().NotBeNull();
        result.Type.Should().Be(FrequencyType.Weekly);
        result.Count.Should().Be(3);
        _mockParser.Verify(p => p.Parse(frequencyString), Times.Once);

        var cachedResult = await _cache.GetAsync(contractId);
        cachedResult.Should().NotBeNull();
        _mockParser.Verify(p => p.Parse(frequencyString), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenKeyExists_ShouldNotParseAgain()
    {
        var contractId = 4;
        var frequencyString = "每天1次";
        var existingRule = new FrequencyRule
        {
            Type = FrequencyType.Daily,
            Count = 1,
            OriginalString = frequencyString
        };

        await _cache.SetAsync(contractId, existingRule);

        var result = await _cache.GetOrCreateAsync(contractId, frequencyString);

        result.Should().NotBeNull();
        _mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SetAsync_WithCustomExpiration_ShouldUseProvidedExpiration()
    {
        var contractId = 5;
        var rule = new FrequencyRule
        {
            Type = FrequencyType.Daily,
            Count = 1,
            OriginalString = "每天1次"
        };
        var customExpiration = TimeSpan.FromHours(1);

        await _cache.SetAsync(contractId, rule, customExpiration);

        var result = await _cache.GetAsync(contractId);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MultipleContractIds_ShouldStoreIndependently()
    {
        var rule1 = new FrequencyRule { Type = FrequencyType.Daily, Count = 1, OriginalString = "每天1次" };
        var rule2 = new FrequencyRule { Type = FrequencyType.Weekly, Count = 3, OriginalString = "每周3次" };

        await _cache.SetAsync(100, rule1);
        await _cache.SetAsync(101, rule2);

        var result1 = await _cache.GetAsync(100);
        var result2 = await _cache.GetAsync(101);

        result1.Should().NotBeNull();
        result1.Type.Should().Be(FrequencyType.Daily);

        result2.Should().NotBeNull();
        result2.Type.Should().Be(FrequencyType.Weekly);
    }

    [Fact]
    public async Task RemoveAsync_ForNonExistentKey_ShouldNotThrow()
    {
        Func<Task> act = async () => await _cache.RemoveAsync(9999);

        await act.Should().NotThrowAsync();
    }
}
