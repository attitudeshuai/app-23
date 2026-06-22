using HabitContract.Domain.Common;

namespace HabitContract.Application.Interfaces;

public interface IFrequencyParser
{
    FrequencyRule Parse(string frequencyString);
    FrequencyValidationResult ValidateCheckIn(FrequencyRule rule, DateTime checkInDate, int currentPeriodCount);
}
