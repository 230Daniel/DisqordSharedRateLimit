using System;
using System.Text.Json;
using System.Threading.Tasks;
using DisqordSharedRateLimit.Rest;
using StackExchange.Redis;

namespace DisqordSharedRateLimit.Extensions
{
    internal static class DatabaseExtensions
    {
        public static Bucket GetBucket(this IDatabase db, string bucketId)
        {
            var value = db.StringGet(bucketId);
            return value.HasValue
                ? JsonSerializer.Deserialize<Bucket>(value)
                : null;
        }
        
        public static void SetBucket(this IDatabase db, Bucket bucket)
        {
            var json = JsonSerializer.Serialize(bucket);
            db.StringSet(bucket.Id, json);
        }

        public static async Task LockBucketAsync(this IDatabase db, string bucketId)
        {
            var success = false;
            while (!success)
            {
                success = await db.LockTakeAsync($"lock-{bucketId}", "", TimeSpan.FromSeconds(30));
                if (!success) await Task.Delay(50);
            }
        }
        
        public static async Task UnlockBucketAsync(this IDatabase db, string bucketId)
        {
            await db.LockReleaseAsync($"lock-{bucketId}", "");
        }
    }
}
