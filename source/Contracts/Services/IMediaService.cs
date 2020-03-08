using System.Threading.Tasks;

namespace Contracts.Services
{
    public interface IMediaService
    {
        Task UploadFile();
        Task<byte[]> GetFile(string path);
    }
}