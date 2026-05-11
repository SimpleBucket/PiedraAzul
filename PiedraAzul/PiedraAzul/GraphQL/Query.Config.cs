using PiedraAzul.Domain.Repositories;

namespace PiedraAzul.GraphQL;

public partial class Query
{
    public async Task<int> GetBookingWindowWeeksAsync(
        [Service] ISystemConfigRepository systemConfigRepository)
    {
        var config = await systemConfigRepository.GetOrCreateAsync();
        return config.BookingWindowWeeks;
    }
}
