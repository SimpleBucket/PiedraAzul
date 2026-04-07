using PiedraAzul.Application.Models;

namespace PiedraAzul.Application.Interfaces
{
    public interface ISlotCache
    {
        List<SlotState> GetSlots(string doctorId, DateTime date);
        void SetSlots(string doctorId, DateTime date, List<SlotState> slots);
        bool TryLockSlot(string doctorId, DateTime date, string slotId, string connectionId);
        void ReleaseSlot(string doctorId, DateTime date, string slotId, string? connectionId);
        List<SlotState> ExpireSlots(string doctorId, DateTime date);
    }
}