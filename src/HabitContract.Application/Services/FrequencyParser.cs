using System.Globalization;
using System.Text.RegularExpressions;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.Services;

public partial class FrequencyParser : IFrequencyParser
{
    private const int DefaultDailyCount = 1;
    private const int DefaultWeeklyCount = 1;
    private const int CacheExpirationHours = 24;

    public FrequencyRule Parse(string frequencyString)
    {
        if (string.IsNullOrWhiteSpace(frequencyString))
        {
            return new FrequencyRule
            {
                Type = FrequencyType.Daily,
                Count = DefaultDailyCount,
                OriginalString = frequencyString ?? string.Empty
            };
        }

        var normalizedInput = frequencyString.Trim().ToLowerInvariant();
        var rule = new FrequencyRule
        {
            OriginalString = frequencyString,
            Type = FrequencyType.Daily,
            Count = DefaultDailyCount
        };

        if (TryParseDailyPattern(normalizedInput, out var dailyCount))
        {
            rule.Type = FrequencyType.Daily;
            rule.Count = dailyCount;
            return rule;
        }

        if (TryParseWeeklyPattern(normalizedInput, out var weeklyCount, out var daysOfWeek))
        {
            rule.Type = FrequencyType.Weekly;
            rule.Count = weeklyCount;
            rule.DaysOfWeek = daysOfWeek;
            return rule;
        }

        if (TryParseEnglishPattern(normalizedInput, out var englishCount, out var englishType, out var englishDaysOfWeek))
        {
            rule.Type = englishType;
            rule.Count = englishCount;
            rule.DaysOfWeek = englishDaysOfWeek;
            return rule;
        }

        return rule;
    }

    public FrequencyValidationResult ValidateCheckIn(FrequencyRule rule, DateTime checkInDate, int currentPeriodCount)
    {
        if (rule.Type == FrequencyType.Daily)
        {
            return ValidateDailyCheckIn(rule, currentPeriodCount);
        }

        return ValidateWeeklyCheckIn(rule, checkInDate, currentPeriodCount);
    }

    private static FrequencyValidationResult ValidateDailyCheckIn(FrequencyRule rule, int currentPeriodCount)
    {
        var result = new FrequencyValidationResult
        {
            CurrentCount = currentPeriodCount,
            RequiredCount = rule.Count,
            Period = "今日"
        };

        if (currentPeriodCount >= rule.Count)
        {
            result.IsValid = false;
            result.ErrorMessage = $"今日打卡次数已达上限（{currentPeriodCount}/{rule.Count}次）";
            return result;
        }

        result.IsValid = true;
        return result;
    }

    private static FrequencyValidationResult ValidateWeeklyCheckIn(FrequencyRule rule, DateTime checkInDate, int currentPeriodCount)
    {
        var result = new FrequencyValidationResult
        {
            CurrentCount = currentPeriodCount,
            RequiredCount = rule.Count,
            Period = "本周"
        };

        if (rule.DaysOfWeek != null && rule.DaysOfWeek.Count > 0)
        {
            if (!rule.DaysOfWeek.Contains(checkInDate.DayOfWeek))
            {
                result.IsValid = false;
                var allowedDays = string.Join("、", rule.DaysOfWeek.Select(d => GetChineseDayName(d)));
                result.ErrorMessage = $"今天不允许打卡，仅允许在{allowedDays}打卡";
                return result;
            }
        }

        if (currentPeriodCount >= rule.Count)
        {
            result.IsValid = false;
            result.ErrorMessage = $"本周打卡次数已达上限（{currentPeriodCount}/{rule.Count}次）";
            return result;
        }

        result.IsValid = true;
        return result;
    }

    private static string GetChineseDayName(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "周一",
            DayOfWeek.Tuesday => "周二",
            DayOfWeek.Wednesday => "周三",
            DayOfWeek.Thursday => "周四",
            DayOfWeek.Friday => "周五",
            DayOfWeek.Saturday => "周六",
            DayOfWeek.Sunday => "周日",
            _ => day.ToString()
        };
    }

    private static bool TryParseDailyPattern(string input, out int count)
    {
        count = DefaultDailyCount;

        var match = DailyChinesePattern().Match(input);
        if (match.Success)
        {
            count = match.Groups["count"].Success ? int.Parse(match.Groups["count"].Value, CultureInfo.InvariantCulture) : DefaultDailyCount;
            count = count > 0 ? count : DefaultDailyCount;
            return true;
        }

        if (input.Contains("每日") || input.Contains("每天") || input.Contains("日更"))
        {
            var numberMatch = NumberPattern().Match(input);
            if (numberMatch.Success)
            {
                count = int.Parse(numberMatch.Value, CultureInfo.InvariantCulture);
                count = count > 0 ? count : DefaultDailyCount;
            }
            return true;
        }

        return false;
    }

    private static bool TryParseWeeklyPattern(string input, out int count, out List<DayOfWeek>? daysOfWeek)
    {
        count = DefaultWeeklyCount;
        daysOfWeek = null;

        var isWeekly = false;
        var hasExplicitCount = false;
        var weeklyMatch = WeeklyChinesePattern().Match(input);
        if (weeklyMatch.Success)
        {
            isWeekly = true;
            hasExplicitCount = weeklyMatch.Groups["count"].Success;
            count = weeklyMatch.Groups["count"].Success
                ? int.Parse(weeklyMatch.Groups["count"].Value, CultureInfo.InvariantCulture)
                : DefaultWeeklyCount;
            count = count > 0 ? count : DefaultWeeklyCount;
        }
        else if (input.Contains("每周") || input.Contains("周更"))
        {
            isWeekly = true;
            if (input.Contains("次"))
            {
                var numberMatch = NumberPattern().Match(input);
                if (numberMatch.Success)
                {
                    hasExplicitCount = true;
                    count = int.Parse(numberMatch.Value, CultureInfo.InvariantCulture);
                    count = count > 0 ? count : DefaultWeeklyCount;
                }
            }
        }
        else if (ContainsWeekdayNames(input))
        {
            isWeekly = true;
        }

        if (!isWeekly)
        {
            return false;
        }

        daysOfWeek = ParseDaysOfWeek(input, hasExplicitCount);
        if (!hasExplicitCount && daysOfWeek != null && daysOfWeek.Count > 0)
        {
            count = daysOfWeek.Count;
        }

        return true;
    }

    private static bool ContainsWeekdayNames(string input)
    {
        var weekdayPatterns = new[] { "周一", "周二", "周三", "周四", "周五", "周六", "周日",
            "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期日", "星期天",
            "周1", "周2", "周3", "周4", "周5", "周6", "周7" };
        return weekdayPatterns.Any(p => input.Contains(p));
    }

    private static bool TryParseEnglishPattern(string input, out int count, out FrequencyType type, out List<DayOfWeek>? daysOfWeek)
    {
        count = DefaultDailyCount;
        type = FrequencyType.Daily;
        daysOfWeek = null;

        if (input.Contains("daily") || input.Contains("every day") || input.Contains("per day"))
        {
            type = FrequencyType.Daily;
            var numberMatch = NumberPattern().Match(input);
            if (numberMatch.Success)
            {
                count = int.Parse(numberMatch.Value, CultureInfo.InvariantCulture);
                count = count > 0 ? count : DefaultDailyCount;
            }
            return true;
        }

        if (input.Contains("weekly") || input.Contains("every week") || input.Contains("per week"))
        {
            type = FrequencyType.Weekly;
            var numberMatch = NumberPattern().Match(input);
            if (numberMatch.Success)
            {
                count = int.Parse(numberMatch.Value, CultureInfo.InvariantCulture);
                count = count > 0 ? count : DefaultWeeklyCount;
            }
            daysOfWeek = ParseEnglishDays(input);
            return true;
        }

        return false;
    }

    private static List<DayOfWeek>? ParseDaysOfWeek(string input, bool hasExplicitCount = false)
    {
        var days = new List<DayOfWeek>();

        var chineseCharMap = new Dictionary<char, DayOfWeek>
        {
            ['一'] = DayOfWeek.Monday,
            ['二'] = DayOfWeek.Tuesday,
            ['三'] = DayOfWeek.Wednesday,
            ['四'] = DayOfWeek.Thursday,
            ['五'] = DayOfWeek.Friday,
            ['六'] = DayOfWeek.Saturday,
            ['日'] = DayOfWeek.Sunday,
            ['天'] = DayOfWeek.Sunday
        };

        var fullDayMap = new Dictionary<string, DayOfWeek>
        {
            ["周一"] = DayOfWeek.Monday,
            ["星期一"] = DayOfWeek.Monday,
            ["周二"] = DayOfWeek.Tuesday,
            ["星期二"] = DayOfWeek.Tuesday,
            ["周三"] = DayOfWeek.Wednesday,
            ["星期三"] = DayOfWeek.Wednesday,
            ["周四"] = DayOfWeek.Thursday,
            ["星期四"] = DayOfWeek.Thursday,
            ["周五"] = DayOfWeek.Friday,
            ["星期五"] = DayOfWeek.Friday,
            ["周六"] = DayOfWeek.Saturday,
            ["星期六"] = DayOfWeek.Saturday,
            ["周日"] = DayOfWeek.Sunday,
            ["星期日"] = DayOfWeek.Sunday,
            ["星期天"] = DayOfWeek.Sunday
        };

        if (!hasExplicitCount)
        {
            var numbersMatch = WeekdayNumbersPattern().Match(input);
            if (numbersMatch.Success)
            {
                var numbers = numbersMatch.Value.Split(new[] { ',', '，', '、', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var numStr in numbers)
                {
                    if (int.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num) && num is >= 1 and <= 7)
                    {
                        var day = num == 7 ? DayOfWeek.Sunday : (DayOfWeek)num;
                        if (!days.Contains(day))
                            days.Add(day);
                    }
                }
            }
        }

        foreach (var kvp in fullDayMap)
        {
            if (input.Contains(kvp.Key) && !days.Contains(kvp.Value))
            {
                days.Add(kvp.Value);
            }
        }

        var weeklyPrefixMatch = Regex.Match(input, @"每周([一二三四五六日天][、，,\s]*)+");
        if (weeklyPrefixMatch.Success)
        {
            var matchedPart = weeklyPrefixMatch.Value;
            foreach (var ch in matchedPart)
            {
                if (chineseCharMap.TryGetValue(ch, out var day) && !days.Contains(day))
                {
                    days.Add(day);
                }
            }
        }

        if (days.Count == 0)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (chineseCharMap.TryGetValue(input[i], out var day))
                {
                    bool hasContext = (i > 0 && input[i - 1] == '周') ||
                                     (i < input.Length - 1 && input[i + 1] == '周') ||
                                     (i > 0 && (input[i - 1] == '、' || input[i - 1] == '，' || input[i - 1] == ',' || input[i - 1] == ' ')) ||
                                     (i < input.Length - 1 && (input[i + 1] == '、' || input[i + 1] == '，' || input[i + 1] == ',' || input[i + 1] == ' '));

                    if (hasContext && !days.Contains(day))
                    {
                        days.Add(day);
                    }
                }
            }
        }

        return days.Count > 0 ? days : null;
    }

    private static List<DayOfWeek>? ParseEnglishDays(string input)
    {
        var days = new List<DayOfWeek>();
        var dayMap = new Dictionary<string, DayOfWeek>
        {
            ["monday"] = DayOfWeek.Monday,
            ["mon"] = DayOfWeek.Monday,
            ["tuesday"] = DayOfWeek.Tuesday,
            ["tue"] = DayOfWeek.Tuesday,
            ["wednesday"] = DayOfWeek.Wednesday,
            ["wed"] = DayOfWeek.Wednesday,
            ["thursday"] = DayOfWeek.Thursday,
            ["thu"] = DayOfWeek.Thursday,
            ["friday"] = DayOfWeek.Friday,
            ["fri"] = DayOfWeek.Friday,
            ["saturday"] = DayOfWeek.Saturday,
            ["sat"] = DayOfWeek.Saturday,
            ["sunday"] = DayOfWeek.Sunday,
            ["sun"] = DayOfWeek.Sunday
        };

        foreach (var kvp in dayMap)
        {
            if (input.Contains(kvp.Key) && !days.Contains(kvp.Value))
            {
                days.Add(kvp.Value);
            }
        }

        return days.Count > 0 ? days : null;
    }

    [GeneratedRegex(@"^(每日|每天|日更)\s*(?<count>\d+)?\s*次?$", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex DailyChinesePattern();

    [GeneratedRegex(@"^每周\s*(?<count>\d+)?\s*次?$", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex WeeklyChinesePattern();

    [GeneratedRegex(@"\d+", RegexOptions.None, "zh-CN")]
    private static partial Regex NumberPattern();

    [GeneratedRegex(@"周\s*[一二三四五六日天]\s*[、，,\s]*(周\s*[一二三四五六日天]\s*)*|(周\s*)?[1-7]\s*[、，,\s]*([1-7]\s*)*", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex WeekdayNumbersPattern();
}
