using System;

namespace Contracts.Types.Common
{
    public class DbEntity
    {
        public Guid Id { get; set; }

        public void InitAsFresh()
        {
            Id = Guid.NewGuid();
        }

        public void Sanitize()
        {
        }

        public Result Validate()
        {
            return Result.Success;
        }
    }
}