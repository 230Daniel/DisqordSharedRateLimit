using System;
using System.Text.Json;
using System.Threading.Tasks;
using DisqordSharedRateLimit.Gateway;
using StackExchange.Redis;

namespace DisqordSharedRateLimit.Extensions
{
    internal static class GatewayDatabaseExtensions
    {
        public static GlobalBucket GetGatewayBucket(this IDatabase db, string bucketId)
        {
            var value = db.StringGet($"gateway-{bucketId}");
            return value.HasValue
                ? JsonSerializer.Deserialize<GlobalBucket>(value)
                : null;
        }
        
        public static void SetGatewayBucket(this IDatabase db, GlobalBucket bucket)
        {
            var json = JsonSerializer.Serialize(bucket);
            db.StringSet($"gateway-{bucket.Id}", json);
        }

        public static async Task LockGatewayBucketAsync(this IDatabase db, string bucketId)
        {
            var success = false;
            while (!success)
            {
                success = await db.LockTakeAsync($"lock-gateway-{bucketId}", "", TimeSpan.FromSeconds(8));
                if (!success) await Task.Delay(50);
            }
        }
        
        public static void UnlockGatewayBucket(this IDatabase db, string bucketId)
        {
            db.LockRelease($"lock-gateway-{bucketId}", "");
        }
    }
}
