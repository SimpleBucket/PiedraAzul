using PiedraAzul.Domain.Entities.Config;

namespace PiedraAzul.Test.Tests;

public class ScheduleRulesTests
{
    [Fact]
    public void CanBook_WithDateInsideConfiguredWindow_ReturnsTrue()
    {
        var config = new SystemConfig(4);
        var inWindowDate = DateTime.UtcNow.AddDays(14); // claramente dentro de 4 semanas

        var result = config.CanBook(inWindowDate);

        Assert.True(result);
    }

    [Fact]
    public void CanBook_WithDateOutsideConfiguredWindow_ReturnsFalse()
    {
        var config = new SystemConfig(2);
        var outOfWindowDate = DateTime.UtcNow.AddDays(30); // claramente fuera de 2 semanas

        var result = config.CanBook(outOfWindowDate);

        Assert.False(result);
    }

    [Fact]
    public void CanBook_WithDateExactlyAtWindowLimit_ReturnsTrue()
    {
        var config = new SystemConfig(3);
        var limitDate = DateTime.UtcNow.AddDays(21);

        var result = config.CanBook(limitDate);

        Assert.True(result);
    }
}