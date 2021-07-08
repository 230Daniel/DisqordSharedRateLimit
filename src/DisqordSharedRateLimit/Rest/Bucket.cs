using System;

namespace DisqordSharedRateLimit.Rest
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
    }
}
