using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dilbert.Common
{
    public abstract class BaseDailyDilbertService : IDailyDilbertService
    {
        protected const string ImgHtmlRegex = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
        protected const string DilbertFilename = "dilbert.jpg";

        protected static readonly Uri RssFeedUri =
            new Uri("http://comicfeeds.chrisbenard.net/view/dilbert/default");

        public async Task<Stream> DailyAsStreamAsync()
        {
            try
            {
                var filePath = await GetImageAsync();

                return new FileInfo(filePath).OpenRead();
            }
            catch (Exception exn)
            {
                throw new Exception("Failed to get daily Dilbert!", exn);
            }
        }

        public async Task<string> DailyAsFileAsync()
        {
            try
            {
                return await GetImageAsync();
            }
            catch (Exception exn)
            {
                throw new Exception("Failed to get daily Dilbert!", exn);
            }
        }

        private async Task<string> GetImageAsync()
        {
            var tempPath = Path.GetTempPath();
            var filePath = Path.Combine(tempPath, DilbertFilename);

            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists)
            {
                var date = DateTimeOffset.UtcNow.Date;
                var fileDate = fileInfo.LastWriteTimeUtc.Date;

                if (date == fileDate) return filePath;
            }

            var imageUri = await GetImageUriFromRssFeedAsync(RssFeedUri);

            await GetImageAndWriteToFileAsync(imageUri, filePath);

            return filePath;
        }

        protected abstract Task<Uri> GetImageUriFromRssFeedAsync(Uri feedUri);

        protected abstract Task GetImageAndWriteToFileAsync(Uri imageUri, string filePath);

        protected HttpClient GetClient()
        {
            var handler = new HttpClientHandler
            {
                Proxy = WebRequest.DefaultWebProxy,
                Credentials = CredentialCache.DefaultCredentials
            };

            if (handler.Proxy != null) handler.Proxy.Credentials = CredentialCache.DefaultCredentials;

            var client = new HttpClient(handler, true);

            return client;
        }
    }
}