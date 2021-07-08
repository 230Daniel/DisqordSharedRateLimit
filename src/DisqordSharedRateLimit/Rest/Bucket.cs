using System;

namespace DisqordSharedRatelimit.Rest
{
    internal class Bucket
    {
        public string Id { get; set; }
        public bool FirstRequest { get; set; }
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public DateTimeOffset ResetsAt { get; set; }

        public Bucket(string id)
        {
            Id = id;
        }

        public Bucket() { }

        public void Update()
        {
            if (ResetsAt <= DateTimeOffset.UtcNow)
            {
                Remaining = 3;
                ResetsAt = DateTimeOffset.UtcNow.AddSeconds(10);
            }
        }
    }
}
