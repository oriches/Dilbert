using System.IO;
using System.Threading.Tasks;

namespace Dilbert.Common
{
    public interface IDailyDilbertService
    {
        Task<string> DailyAsFileAsync();

        Task<Stream> DailyAsStreamAsync();
    }
}
