using System;
using System.Threading.Tasks;
using Contracts.Types.Common;

namespace Contracts.Repo
{
    public interface IRepo<TEntity>
    {
        Task<Guid> Save(Guid? id, TEntity data);
        Task<TEntity> Get(Guid id);
        Task Delete(Guid id);

        Task<Result<Guid>> Save(TEntity data);
        Task<Result> Update(Guid id, TEntity data);
        Task<Result<TEntity>> Read(Guid id);
        Task<Result> DeleteOne(Guid id);
    }
}