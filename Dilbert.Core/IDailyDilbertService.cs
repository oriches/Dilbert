using System.IO;
using System.Threading.Tasks;

namespace Dilbert.Core
{
    public interface IDailyDilbertService
    {
        Task<string> DailyAsFileAsync();

        Task<Stream> DailyAsStreamAsync();
    }
}
