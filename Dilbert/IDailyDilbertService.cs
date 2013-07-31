namespace Dilbert.Services
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IDailyDilbertService
    {
        Task<string> DailyAsFileAsync();

        Task<Stream> DailyAsStreamAsync();
    }
}
