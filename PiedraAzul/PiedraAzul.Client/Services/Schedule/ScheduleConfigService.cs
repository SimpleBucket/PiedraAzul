using PiedraAzul.Client.Models;
using PiedraAzul.Client.Models.Schedule;
using PiedraAzul.Client.Services.GraphQLServices;

namespace PiedraAzul.Client.Services.Schedule;

public interface IScheduleConfigService
{
    Task<ScheduleConfigModel?> GetByDoctorIdAsync(string doctorId);
    Task<Result<bool>> SaveAsync(ScheduleConfigModel config);
}

public class ScheduleConfigService(GraphQLHttpClient client) : IScheduleConfigService
{
    private readonly Dictionary<string, ScheduleConfigModel> _fallbackStore = new();

    public async Task<ScheduleConfigModel?> GetByDoctorIdAsync(string doctorId)
    {
        if (string.IsNullOrWhiteSpace(doctorId))
            return null;

        var fromBackend = await TryGetFromBackendAsync(doctorId);
        if (fromBackend is not null)
        {
            _fallbackStore[doctorId] = Clone(fromBackend);
            return fromBackend;
        }

        if (_fallbackStore.TryGetValue(doctorId, out var existing))
            return Clone(existing);

        var defaultConfig = BuildDefaultConfig(doctorId);
        _fallbackStore[doctorId] = Clone(defaultConfig);
        return defaultConfig;
    }

    public async Task<Result<bool>> SaveAsync(ScheduleConfigModel config)
    {
        if (config is null || string.IsNullOrWhiteSpace(config.DoctorId))
            return Result<bool>.Failure(new ErrorResult("DoctorId es requerido.", "Validation"));

        var backendSave = await TrySaveToBackendAsync(config);
        if (backendSave.IsSuccess)
        {
            _fallbackStore.Remove(config.DoctorId);
            return backendSave;
        }

        _fallbackStore[config.DoctorId] = Clone(config);
        return Result<bool>.Success(true);
    }

    private async Task<ScheduleConfigModel?> TryGetFromBackendAsync(string doctorId)
    {
        const string query = """
            query GetScheduleConfigByDoctorId($doctorId: String!) {
                scheduleConfigByDoctorId(doctorId: $doctorId) {
                    doctorId
                    bookingWindowWeeks
                    intervalMinutes
                    availability {
                        dayOfWeek
                        isEnabled
                        startTime
                        endTime
                    }
                    slots {
                        id
                        dayOfWeek
                        startTime
                        endTime
                        isDeleted
                    }
                }
            }
            """;

        try
        {
            return await client.ExecuteAsync<ScheduleConfigModel>(
                query,
                new { doctorId },
                "scheduleConfigByDoctorId");
        }
        catch(Exception ex) 
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    private async Task<Result<bool>> TrySaveToBackendAsync(ScheduleConfigModel config)
    {
        const string mutation = """
            mutation SaveScheduleConfig($input: ScheduleConfigInput!) {
                saveScheduleConfig(input: $input)
            }
            """;

        try
        {
            var success = await client.ExecuteAsync<bool>(
                mutation,
                new
                {
                    input = new
                    {
                        doctorId = config.DoctorId,
                        bookingWindowWeeks = config.BookingWindowWeeks,
                        activeSlots = config.ActiveSlots.Select(s => new
                        {
                            dayOfWeek = s.DayOfWeek.ToString().ToUpper(),
                            startTime = s.StartTime,
                            endTime = s.EndTime
                        }).ToList()
                    }
                },
                "saveScheduleConfig");

            return Result<bool>.Success(success);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new ErrorResult($"Error al guardar: {ex.Message}", "ScheduleConfigApi"));
        }
    }

    private static ScheduleConfigModel BuildDefaultConfig(string doctorId)
    {
        return new ScheduleConfigModel
        {
            DoctorId = doctorId,
            BookingWindowWeeks = 4,
            IntervalMinutes = 15,
            Availability =
            [
                new() { DayOfWeek = DayOfWeek.Monday, IsEnabled = true, StartTime = "08:00:00", EndTime = "17:00:00" },
                new() { DayOfWeek = DayOfWeek.Tuesday, IsEnabled = true, StartTime = "08:00:00", EndTime = "17:00:00" },
                new() { DayOfWeek = DayOfWeek.Wednesday, IsEnabled = true, StartTime = "08:00:00", EndTime = "17:00:00" },
                new() { DayOfWeek = DayOfWeek.Thursday, IsEnabled = true, StartTime = "08:00:00", EndTime = "17:00:00" },
                new() { DayOfWeek = DayOfWeek.Friday, IsEnabled = true, StartTime = "08:00:00", EndTime = "17:00:00" }
            ]
        };
    }

    private static ScheduleConfigModel Clone(ScheduleConfigModel model) => new()
    {
        DoctorId = model.DoctorId,
        BookingWindowWeeks = model.BookingWindowWeeks,
        IntervalMinutes = model.IntervalMinutes,
        Availability = model.Availability
            .Select(x => new AvailabilityDayModel
            {
                DayOfWeek = x.DayOfWeek,
                IsEnabled = x.IsEnabled,
                StartTime = x.StartTime,
                EndTime = x.EndTime
            })
            .ToList(),
        Slots = model.Slots
            .Select(x => new RawSlotInfo
            {
                DayOfWeek = x.DayOfWeek,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                IsDeleted = x.IsDeleted
            })
            .ToList()
    };
}
