using PiedraAzul.Application.Common.Models.Schedule;

namespace PiedraAzul.GraphQL.Types;

public class ScheduleDayType
{
    public string DayOfWeek { get; set; }
    public bool IsEnabled { get; set; }
    public string StartTime { get; set; } = "08:00:00";
    public string EndTime { get; set; } = "17:00:00";
}

public class RawSlotType
{
    public string Id { get; set; } = string.Empty;
    public string DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}

public class ScheduleConfigType
{
    public string DoctorId { get; set; } = string.Empty;
    public int BookingWindowWeeks { get; set; }
    public int IntervalMinutes { get; set; }
    public IReadOnlyList<ScheduleDayType> Availability { get; set; } = [];
    public IReadOnlyList<RawSlotType> Slots { get; set; } = [];

    public static ScheduleConfigType FromDto(ScheduleConfigDto dto)
    {
        return new ScheduleConfigType
        {
            DoctorId = dto.DoctorId,
            BookingWindowWeeks = dto.BookingWindowWeeks,
            IntervalMinutes = dto.IntervalMinutes,
            Availability = dto.Availability
                .Select(day => new ScheduleDayType
                {
                    DayOfWeek = day.DayOfWeek.ToString(),
                    IsEnabled = day.IsEnabled,
                    StartTime = $"{day.StartTime.Hours:00}:{day.StartTime.Minutes:00}:{day.StartTime.Seconds:00}",
                    EndTime = $"{day.EndTime.Hours:00}:{day.EndTime.Minutes:00}:{day.EndTime.Seconds:00}"
                })
                .ToList()
        };
    }
}
