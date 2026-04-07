using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PiedraAzul.Application.Interfaces;
using PiedraAzul.Application.Models;

namespace PiedraAzul.Infrastructure.Caching
{
    public class SlotCache : ISlotCache
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<SlotCache> _logger;
        private static readonly object _lockObject = new();

        public SlotCache(IMemoryCache cache, ILogger<SlotCache> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        private string GetKey(string doctorId, DateTime date)
            => $"slots:{doctorId}:{date:yyyyMMdd}";

        public List<SlotState> GetSlots(string doctorId, DateTime date)
        {
            var slots = _cache.Get<List<SlotState>>(GetKey(doctorId, date));
            return slots ?? new List<SlotState>();
        }

        public void SetSlots(string doctorId, DateTime date, List<SlotState> slots)
        {
            _cache.Set(GetKey(doctorId, date), slots, TimeSpan.FromHours(12));
        }

        public bool TryLockSlot(string doctorId, DateTime date, string slotId, string connectionId)
        {
            lock (_lockObject)
            {
                var slots = GetSlots(doctorId, date);
                var target = slots.FirstOrDefault(s => s.SlotId == slotId);

                if (target == null)
                    return false;

                if (target.IsReserved)
                    return false;

                if (target.LockedBy != null &&
                    target.LockedBy != connectionId &&
                    target.LockExpiresAt > DateTime.UtcNow)
                    return false;

                target.LockedBy = connectionId;
                target.LockExpiresAt = DateTime.UtcNow.AddMinutes(5);

                SetSlots(doctorId, date, slots);
                return true;
            }
        }

        public void ReleaseSlot(string doctorId, DateTime date, string slotId, string? connectionId)
        {
            var slots = GetSlots(doctorId, date);
            var target = slots.FirstOrDefault(s => s.SlotId == slotId);

            if (target == null) return;

            if (target.LockedBy != connectionId && target.LockExpiresAt > DateTime.UtcNow)
                return;

            target.LockedBy = null;
            target.LockExpiresAt = null;

            SetSlots(doctorId, date, slots);
        }

        public List<SlotState> ExpireSlots(string doctorId, DateTime date)
        {
            lock (_lockObject)
            {
                var slots = GetSlots(doctorId, date);

                var expired = slots
                    .Where(s => s.LockedBy != null && s.LockExpiresAt <= DateTime.UtcNow)
                    .ToList();

                foreach (var s in expired)
                {
                    s.LockedBy = null;
                    s.LockExpiresAt = null;
                }

                if (expired.Any())
                    SetSlots(doctorId, date, slots);

                return expired;
            }
        }
    }
}