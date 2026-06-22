using System.Text.RegularExpressions;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.Services;

public class PenaltyRuleParser : IPenaltyRuleParser
{
    public PenaltyCalculationResult ParseAndCalculate(
        PenaltyRule? rule,
        Contract contract,
        User user,
        ContractViolation violation,
        int priorViolationCount = 0)
    {
        var result = new PenaltyCalculationResult();

        if (rule == null || !rule.IsActive)
        {
            result.Success = true;
            result.UsesDefaultRule = true;
            result.RequiresAdminConfiguration = true;
            result.Message = "该契约尚未配置惩罚规则，已应用默认提示规则。建议管理员补充配置具体惩罚内容。";
            result.PenaltyType = PenaltyType.Custom;
            result.Severity = violation.IsSevere ? PenaltySeverity.Severe : PenaltySeverity.Medium;
            result.CalculatedContent = GenerateDefaultDescription(PenaltyType.Custom, result.Severity);
            result.CreditScoreChange = result.Severity == PenaltySeverity.Severe ? 10 : 5;
            result.PaymentRequired = false;
            return result;
        }

        result.Success = true;
        result.UsesDefaultRule = false;
        result.PenaltyType = rule.PenaltyType;
        result.Severity = CalculateSeverity(rule, violation, user, priorViolationCount);
        result.PaymentRequired = rule.PaymentRequired;
        result.CreditScoreChange = rule.CreditScoreAffected ? CalculateCreditScoreImpact(rule, result.Severity, priorViolationCount) : 0;

        if (rule.PenaltyType == PenaltyType.Financial)
        {
            result.FinancialAmount = CalculateFinancialAmount(rule, result.Severity, priorViolationCount, user);
        }

        result.CalculatedContent = BuildCalculatedContent(rule, result, violation, priorViolationCount, user);

        return result;
    }

    public bool TryParseRuleExpression(string expression, out string parsedDescription)
    {
        parsedDescription = string.Empty;

        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        try
        {
            parsedDescription = expression
                .Replace("{violation_count}", "违约次数")
                .Replace("{severity}", "严重程度")
                .Replace("{credit_score}", "当前信用分")
                .Replace("{amount}", "惩罚金额")
                .Replace("{days}", "天数")
                .Replace("{habit_name}", "习惯名称")
                .Replace("{date}", "违约日期");

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateDefaultDescription(PenaltyType type, PenaltySeverity severity)
    {
        var severityText = PenaltyTypeMigrator.GetSeverityName(severity);
        var typeText = PenaltyTypeMigrator.GetPenaltyTypeName(type);

        return type switch
        {
            PenaltyType.Financial => $"【{severityText}经济惩罚】根据违约严重程度，需缴纳约定的违约金。具体金额请管理员补充配置。",
            PenaltyType.Service => $"【{severityText}服务惩罚】根据违约严重程度，需为监督伙伴或社区提供约定时长的服务。具体内容请管理员补充配置。",
            PenaltyType.Social => $"【{severityText}社交惩罚】根据违约严重程度，需在约定社交范围内公示违约行为。具体方式请管理员补充配置。",
            PenaltyType.Physical => $"【{severityText}体力惩罚】根据违约严重程度，需完成约定的体力活动。具体内容请管理员补充配置。",
            _ => $"【{severityText}惩罚】已记录违约行为，具体惩罚内容待管理员补充配置。请关注后续通知并及时履行惩罚义务。"
        };
    }

    private static PenaltySeverity CalculateSeverity(PenaltyRule rule, ContractViolation violation, User user, int priorViolationCount)
    {
        if (violation.IsSevere)
        {
            return PenaltySeverity.Severe;
        }

        if (priorViolationCount >= 5)
        {
            return PenaltySeverity.Severe;
        }

        if (priorViolationCount >= 3)
        {
            return PenaltySeverity.Medium;
        }

        if (user.CreditScore < 60)
        {
            return PenaltySeverity.Severe;
        }

        if (user.CreditScore < 80)
        {
            return PenaltySeverity.Medium;
        }

        return rule.DefaultSeverity;
    }

    private static int CalculateCreditScoreImpact(PenaltyRule rule, PenaltySeverity severity, int priorViolationCount)
    {
        var baseImpact = rule.CreditScoreImpact;
        var multiplier = severity switch
        {
            PenaltySeverity.Light => 0.5,
            PenaltySeverity.Medium => 1.0,
            PenaltySeverity.Severe => 2.0,
            _ => 1.0
        };

        var escalationMultiplier = 1.0 + (priorViolationCount * 0.1);
        var totalImpact = (int)Math.Round(baseImpact * multiplier * escalationMultiplier);

        return Math.Min(totalImpact, 30);
    }

    private static decimal? CalculateFinancialAmount(PenaltyRule rule, PenaltySeverity severity, int priorViolationCount, User user)
    {
        if (string.IsNullOrWhiteSpace(rule.BaseAmount))
        {
            return null;
        }

        if (!decimal.TryParse(rule.BaseAmount, out var baseAmount))
        {
            var match = Regex.Match(rule.BaseAmount, @"\d+(\.\d{1,2})?");
            if (!match.Success || !decimal.TryParse(match.Value, out baseAmount))
            {
                return null;
            }
        }

        var severityMultiplier = severity switch
        {
            PenaltySeverity.Light => 0.5m,
            PenaltySeverity.Medium => 1.0m,
            PenaltySeverity.Severe => 2.0m,
            _ => 1.0m
        };

        var escalationMultiplier = 1.0m + (priorViolationCount * 0.2m);
        var creditMultiplier = user.CreditScore < 60 ? 1.5m : user.CreditScore < 80 ? 1.2m : 1.0m;

        var finalAmount = Math.Round(baseAmount * severityMultiplier * escalationMultiplier * creditMultiplier, 2);

        return finalAmount;
    }

    private string BuildCalculatedContent(
        PenaltyRule rule,
        PenaltyCalculationResult result,
        ContractViolation violation,
        int priorViolationCount,
        User user)
    {
        TryParseRuleExpression(rule.RuleExpression, out var parsedExpression);
        var severityText = PenaltyTypeMigrator.GetSeverityName(result.Severity);
        var typeText = PenaltyTypeMigrator.GetPenaltyTypeName(result.PenaltyType);
        var violationTypeText = ViolationTypeMigrator.Normalize(violation.ViolationType) switch
        {
            ViolationType.MissedCheckIn => "未打卡",
            ViolationType.NotMetTarget => "未达标",
            _ => "违约"
        };

        var parts = new List<string>
        {
            $"【{severityText}{typeText}】因{violationTypeText}触发惩罚"
        };

        if (!string.IsNullOrWhiteSpace(parsedExpression))
        {
            parts.Add($"规则说明：{parsedExpression}");
        }
        else if (!string.IsNullOrWhiteSpace(rule.Description))
        {
            parts.Add($"规则说明：{rule.Description}");
        }

        if (result.FinancialAmount.HasValue)
        {
            parts.Add($"惩罚金额：¥{result.FinancialAmount.Value:F2}");
            if (user.IsPaymentSuspended)
            {
                parts.Add("⚠️ 注意：该用户支付功能已被暂停，请联系管理员处理。");
            }
            else if (user.OutstandingPenaltyBalance > 0)
            {
                parts.Add($"历史未缴罚金余额：¥{user.OutstandingPenaltyBalance:F2}");
            }
        }

        if (rule.CreditScoreAffected && result.CreditScoreChange > 0)
        {
            parts.Add($"信用分影响：-{result.CreditScoreChange}分（当前信用分：{user.CreditScore}，执行后预计：{Math.Max(0, user.CreditScore - result.CreditScoreChange)}）");
        }

        if (priorViolationCount > 0)
        {
            parts.Add($"历史违约次数：{priorViolationCount}次（已触发惩罚升级规则）");
        }

        if (!string.IsNullOrWhiteSpace(rule.EscalationRule) && priorViolationCount >= 2)
        {
            parts.Add($"升级规则：{rule.EscalationRule}");
        }

        return string.Join(" | ", parts);
    }
}
