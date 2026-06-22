using FluentAssertions;
using HabitContract.Application.Services;
using HabitContract.Domain.Enums;
using Xunit;

namespace HabitContract.Tests;

public class FrequencyParserTests
{
    private readonly FrequencyParser _parser;

    public FrequencyParserTests()
    {
        _parser = new FrequencyParser();
    }

    [Theory]
    [InlineData("每天1次", FrequencyType.Daily, 1)]
    [InlineData("每日1次", FrequencyType.Daily, 1)]
    [InlineData("日更1次", FrequencyType.Daily, 1)]
    [InlineData("每天2次", FrequencyType.Daily, 2)]
    [InlineData("每日3次", FrequencyType.Daily, 3)]
    [InlineData("每天", FrequencyType.Daily, 1)]
    [InlineData("每日", FrequencyType.Daily, 1)]
    [InlineData("日更", FrequencyType.Daily, 1)]
    public void Parse_DailyChinesePatterns_ReturnsCorrectRule(string input, FrequencyType expectedType, int expectedCount)
    {
        var result = _parser.Parse(input);

        result.Type.Should().Be(expectedType);
        result.Count.Should().Be(expectedCount);
        result.OriginalString.Should().Be(input);
        result.DaysOfWeek.Should().BeNull();
    }

    [Theory]
    [InlineData("daily", FrequencyType.Daily, 1)]
    [InlineData("every day", FrequencyType.Daily, 1)]
    [InlineData("per day", FrequencyType.Daily, 1)]
    [InlineData("daily 2 times", FrequencyType.Daily, 2)]
    [InlineData("every day 3 times", FrequencyType.Daily, 3)]
    [InlineData("2 times per day", FrequencyType.Daily, 2)]
    public void Parse_DailyEnglishPatterns_ReturnsCorrectRule(string input, FrequencyType expectedType, int expectedCount)
    {
        var result = _parser.Parse(input);

        result.Type.Should().Be(expectedType);
        result.Count.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("每周1次", FrequencyType.Weekly, 1)]
    [InlineData("每周3次", FrequencyType.Weekly, 3)]
    [InlineData("周更2次", FrequencyType.Weekly, 2)]
    [InlineData("每周", FrequencyType.Weekly, 1)]
    public void Parse_WeeklyChinesePatterns_ReturnsCorrectRule(string input, FrequencyType expectedType, int expectedCount)
    {
        var result = _parser.Parse(input);

        result.Type.Should().Be(expectedType);
        result.Count.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("weekly", FrequencyType.Weekly, 1)]
    [InlineData("every week", FrequencyType.Weekly, 1)]
    [InlineData("per week", FrequencyType.Weekly, 1)]
    [InlineData("weekly 3 times", FrequencyType.Weekly, 3)]
    [InlineData("2 times per week", FrequencyType.Weekly, 2)]
    public void Parse_WeeklyEnglishPatterns_ReturnsCorrectRule(string input, FrequencyType expectedType, int expectedCount)
    {
        var result = _parser.Parse(input);

        result.Type.Should().Be(expectedType);
        result.Count.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("每周一、三、五")]
    [InlineData("每周1、3、5")]
    [InlineData("周一周三周五")]
    [InlineData("每周一、三、五、日")]
    public void Parse_WeeklyWithDays_ParseDaysCorrectly(string input)
    {
        var result = _parser.Parse(input);

        result.Type.Should().Be(FrequencyType.Weekly);
        result.DaysOfWeek.Should().NotBeNull();
        result.DaysOfWeek.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Parse_WeeklyWithSpecificDays_ContainsCorrectDays()
    {
        var result = _parser.Parse("每周一、三、五");

        result.DaysOfWeek.Should().Contain(DayOfWeek.Monday);
        result.DaysOfWeek.Should().Contain(DayOfWeek.Wednesday);
        result.DaysOfWeek.Should().Contain(DayOfWeek.Friday);
    }

    [Fact]
    public void Parse_EmptyString_ReturnsDefaultDailyRule()
    {
        var result = _parser.Parse(string.Empty);

        result.Type.Should().Be(FrequencyType.Daily);
        result.Count.Should().Be(1);
    }

    [Fact]
    public void Parse_WhitespaceString_ReturnsDefaultDailyRule()
    {
        var result = _parser.Parse("   ");

        result.Type.Should().Be(FrequencyType.Daily);
        result.Count.Should().Be(1);
    }

    [Fact]
    public void Parse_NullString_ReturnsDefaultDailyRule()
    {
        var result = _parser.Parse(null!);

        result.Type.Should().Be(FrequencyType.Daily);
        result.Count.Should().Be(1);
    }

    [Fact]
    public void Parse_WeeklyWithEnglishDays_ParseDaysCorrectly()
    {
        var result = _parser.Parse("weekly on Monday, Wednesday, Friday");

        result.Type.Should().Be(FrequencyType.Weekly);
        result.DaysOfWeek.Should().NotBeNull();
        result.DaysOfWeek.Should().Contain(DayOfWeek.Monday);
        result.DaysOfWeek.Should().Contain(DayOfWeek.Wednesday);
        result.DaysOfWeek.Should().Contain(DayOfWeek.Friday);
    }

    [Fact]
    public void ValidateCheckIn_DailyWithZeroCount_IsValid()
    {
        var rule = _parser.Parse("每天1次");
        var result = _parser.ValidateCheckIn(rule, DateTime.Now, 0);

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCheckIn_DailyWithCountEqualToMax_IsInvalid()
    {
        var rule = _parser.Parse("每天1次");
        var result = _parser.ValidateCheckIn(rule, DateTime.Now, 1);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("今日打卡次数已达上限");
        result.Period.Should().Be("今日");
    }

    [Fact]
    public void ValidateCheckIn_DailyWithCountExceedingMax_IsInvalid()
    {
        var rule = _parser.Parse("每天2次");
        var result = _parser.ValidateCheckIn(rule, DateTime.Now, 2);

        result.IsValid.Should().BeFalse();
        result.CurrentCount.Should().Be(2);
        result.RequiredCount.Should().Be(2);
    }

    [Fact]
    public void ValidateCheckIn_WeeklyWithZeroCount_IsValid()
    {
        var rule = _parser.Parse("每周3次");
        var result = _parser.ValidateCheckIn(rule, DateTime.Now, 0);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCheckIn_WeeklyWithCountEqualToMax_IsInvalid()
    {
        var rule = _parser.Parse("每周3次");
        var result = _parser.ValidateCheckIn(rule, DateTime.Now, 3);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("本周打卡次数已达上限");
        result.Period.Should().Be("本周");
    }

    [Fact]
    public void ValidateCheckIn_WeeklyWithAllowedDay_IsValid()
    {
        var rule = _parser.Parse("每周一、三、五");
        var monday = new DateTime(2024, 1, 1);

        var result = _parser.ValidateCheckIn(rule, monday, 0);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCheckIn_WeeklyWithDisallowedDay_IsInvalid()
    {
        var rule = _parser.Parse("每周一、三、五");
        var tuesday = new DateTime(2024, 1, 2);

        var result = _parser.ValidateCheckIn(rule, tuesday, 0);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("今天不允许打卡");
        result.ErrorMessage.Should().Contain("仅允许在");
    }
}
