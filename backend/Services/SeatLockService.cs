using backend.Data;
using backend.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Manages seat locking using IDistributedCache (works with Redis or In-Memory).
    /// Key: "seat_lock:{scheduleId}:{seatId}" → Value: userId who locked it.
    /// </summary>
    public interface ISeatLockService
    {
        Task<bool> TryLockSeatAsync(int scheduleId, int seatId, int userId, TimeSpan duration);
        Task<bool> IsSeatLockedAsync(int scheduleId, int seatId, int? excludeUserId = null);
        Task ReleaseSeatAsync(int scheduleId, int seatId);
        Task ReleaseAllLocksForUserAsync(int scheduleId, int userId, IEnumerable<int> seatIds);
        Task<int?> GetSeatLockOwnerAsync(int scheduleId, int seatId);
    }

    public class SeatLockService : ISeatLockService
    {
        private readonly IDistributedCache _cache;

        public SeatLockService(IDistributedCache cache)
        {
            _cache = cache;
        }

        private static string Key(int scheduleId, int seatId) =>
            $"seat_lock:{scheduleId}:{seatId}";

        public async Task<bool> TryLockSeatAsync(int scheduleId, int seatId, int userId, TimeSpan duration)
        {
            var key = Key(scheduleId, seatId);
            var existing = await _cache.GetStringAsync(key);
            if (existing != null)
            {
                if (existing == userId.ToString())
                {
                    await _cache.SetStringAsync(key, userId.ToString(), new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = duration
                    });
                    return true;
                }
                return false;
            }

            await _cache.SetStringAsync(key, userId.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            });
            return true;
        }

        public async Task<bool> IsSeatLockedAsync(int scheduleId, int seatId, int? excludeUserId = null)
        {
            var val = await _cache.GetStringAsync(Key(scheduleId, seatId));
            if (val == null) return false;
            if (excludeUserId.HasValue && val == excludeUserId.Value.ToString()) return false;
            return true;
        }

        public async Task<int?> GetSeatLockOwnerAsync(int scheduleId, int seatId)
        {
            var val = await _cache.GetStringAsync(Key(scheduleId, seatId));
            if (int.TryParse(val, out var userId)) return userId;
            return null;
        }

        public async Task ReleaseSeatAsync(int scheduleId, int seatId) =>
            await _cache.RemoveAsync(Key(scheduleId, seatId));

        public async Task ReleaseAllLocksForUserAsync(int scheduleId, int userId, IEnumerable<int> seatIds)
        {
            foreach (var seatId in seatIds)
            {
                var val = await _cache.GetStringAsync(Key(scheduleId, seatId));
                if (val == userId.ToString())
                    await _cache.RemoveAsync(Key(scheduleId, seatId));
            }
        }
    }
}
