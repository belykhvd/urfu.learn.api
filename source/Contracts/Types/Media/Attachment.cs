using System;

namespace Contracts.Types.Media
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid Author { get; set; }
    }
}