using System.Text.Json.Serialization;

namespace PiedraAzul.Client.Models.Schedule;

public class SlotItem
{
    [JsonPropertyName("dayOfWeek")]
    public string DayOfWeekRaw { get; set; } = "";

    [JsonIgnore]
    public DayOfWeek DayOfWeek
    {
        get => Enum.TryParse<DayOfWeek>(DayOfWeekRaw, true, out var d) ? d : default;
        set => DayOfWeekRaw = value.ToString();
    }

    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}


public class RawSlotInfo
{
    [JsonPropertyName("dayOfWeek")]
    public string DayOfWeekRaw { get; set; } = "";

    [JsonIgnore]
    public DayOfWeek DayOfWeek
    {
        get => Enum.TryParse<DayOfWeek>(DayOfWeekRaw, true, out var d) ? d : default;
        set => DayOfWeekRaw = value.ToString();
    }

    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}
