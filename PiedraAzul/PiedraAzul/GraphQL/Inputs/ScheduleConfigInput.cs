namespace PiedraAzul.GraphQL.Inputs;

public record SlotInput(
    DayOfWeek DayOfWeek,
    string StartTime,
    string EndTime);

public record ScheduleConfigInput(
    string DoctorId,
    int BookingWindowWeeks,
    IReadOnlyList<SlotInput> ActiveSlots);
