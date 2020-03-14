using System;
using System.Threading.Tasks;
using Contracts.Types.Common;

namespace Contracts.Repo
{
    public interface ICrudRepo<TEntity>
    {
        Task<Result<Guid>> Save(TEntity data);
        Task<Result> Update(Guid id, TEntity data);
        Task<Result<TEntity>> Read(Guid id);
        Task<Result> Delete(Guid id);
    }
}