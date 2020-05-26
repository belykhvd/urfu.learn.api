using System.IO;

namespace Contracts.Types.Media
{
    public class UserFile
    {
        public string FileName { get; set; }
        public FileStream FileStream { get; set; }
    }
}