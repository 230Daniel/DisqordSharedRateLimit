using System;
using System.Text.Json;
using System.Threading.Tasks;
using DisqordSharedRateLimit.Rest;
using StackExchange.Redis;

namespace DisqordSharedRateLimit.Extensions
{
    internal static class RestDatabaseExtensions
    {
        public static Bucket GetRestBucket(this IDatabase db, string bucketId)
        {
            var value = db.StringGet($"rest-{bucketId}");
            return value.HasValue
                ? JsonSerializer.Deserialize<Bucket>(value)
                : null;
        }
        
        public static void SetRestBucket(this IDatabase db, Bucket bucket)
        {
            var json = JsonSerializer.Serialize(bucket);
            db.StringSet($"rest-{bucket.Id}", json);
        }

        public static async Task LockRestBucketAsync(this IDatabase db, string bucketId)
        {
            var success = false;
            while (!success)
            {
                success = await db.LockTakeAsync($"lock-rest-{bucketId}", "", TimeSpan.FromSeconds(30));
                if (!success) await Task.Delay(50);
            }
        }
        
        public static async Task UnlockRestBucketAsync(this IDatabase db, string bucketId)
        {
            await db.LockReleaseAsync($"lock-rest-{bucketId}", "");
        }
    }
}
