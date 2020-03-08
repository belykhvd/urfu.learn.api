using System;
using System.Threading.Tasks;
using Contracts.Types.Common;

namespace Contracts.Repo
{
    public interface ICrudRepo<TEntity>
    {
        Task<OperationStatus<Guid>> Create(TEntity data);
        Task<OperationStatus> Update(TEntity data);
        Task<OperationStatus<TEntity>> Read(Guid id);
        Task<OperationStatus> Delete(Guid id);
    }
}