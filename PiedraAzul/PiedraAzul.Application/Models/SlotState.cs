namespace PiedraAzul.Application.Models;

public class SlotState
{
    public string SlotId { get; set; } = default!;
    public bool IsReserved { get; set; }

    public string? LockedBy { get; set; }
    public DateTime? LockExpiresAt { get; set; }
}