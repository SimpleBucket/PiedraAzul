using System.Text.Json.Serialization;

public class AvailabilityDayModel
{
    [JsonPropertyName("dayOfWeek")]
    public string DayOfWeekRaw { get; set; } = "";

    [JsonIgnore]
    public DayOfWeek DayOfWeek
    {
        get => Enum.TryParse<DayOfWeek>(DayOfWeekRaw, true, out var d) ? d : default;
        set => DayOfWeekRaw = value.ToString();
    }

    public bool IsEnabled { get; set; } = true;

    public string StartTime { get; set; } = "08:00:00";
    public string EndTime { get; set; } = "17:00:00";

    public TimeSpan StartTimeSpan
    {
        get => TimeSpan.TryParse(StartTime, out var ts) ? ts : new(8, 0, 0);
        set => StartTime = value.ToString(@"hh\:mm\:ss");
    }

    public TimeSpan EndTimeSpan
    {
        get => TimeSpan.TryParse(EndTime, out var ts) ? ts : new(17, 0, 0);
        set => EndTime = value.ToString(@"hh\:mm\:ss");
    }
}