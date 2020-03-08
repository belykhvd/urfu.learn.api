using System;

namespace Contracts.Types.Common
{
    public class DbEntity
    {
        public Guid Id { get; set; }
        public bool Deleted { get; set; }
        public int Version { get; set; }
        public DateTime CreationDateTime { get; set; }

        public void InitAsFresh()
        {
            Id = Guid.NewGuid();
            Deleted = false;
            Version = 0;
            CreationDateTime = DateTime.Now;
        }
    }
}