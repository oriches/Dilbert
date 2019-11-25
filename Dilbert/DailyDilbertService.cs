using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Dilbert
{
    public sealed class DailyDilbertService : IDailyDilbertService
    {
        private const string ImgHtmlRegex = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
        private const string DilbertFilename = "dilbert.jpg";

        private static readonly Uri RssFeedUri =
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

        private static async Task<string> GetImageAsync()
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

        private static async Task<Uri> GetImageUriFromRssFeedAsync(Uri feedUri)
        {
            using (var client = GetClient())
            using (var response = await client.GetAsync(feedUri))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new XmlTextReader(stream))
            {
                var feed = SyndicationFeed.Load(reader);

                // The items are in reverse date order - so the latest is first
                var latestItem = feed.Items.First();

                var content = (TextSyndicationContent) latestItem.Content;
                var match = Regex.Match(content.Text, ImgHtmlRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (match.Success)
                {
                    var href = match.Groups[1].Value;
                    return new Uri("http:" + href);
                }

                return null;
            }
        }

        private static async Task GetImageAndWriteToFileAsync(Uri imageUri, string filePath)
        {
            using (var client = GetClient())
            using (var response = await client.GetAsync(imageUri))
            using (var imageStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = File.Create(filePath))
            {
                imageStream.CopyTo(fileStream);
            }
        }

        private static HttpClient GetClient()
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