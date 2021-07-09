using System;

namespace DisqordSharedRateLimit.Gateway
{
    public class GlobalBucket
    {
        public string Id { get; set; }
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public DateTimeOffset ResetsAt { get; set; }
        
        public GlobalBucket(string id)
        {
            Id = id;
        }

        public GlobalBucket() { }
    }
}
