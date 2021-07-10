using System;

namespace DisqordSharedRateLimit.Rest
{
    public class GlobalBucket
    {
        public int Remaining { get; set; }
        public DateTimeOffset ResetsAt { get; set; }

        public void Reset()
        {
            Remaining = 50;
            ResetsAt = DateTimeOffset.UtcNow.AddSeconds(1);
        }
    }
}
