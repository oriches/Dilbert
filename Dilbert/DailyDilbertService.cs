namespace Dilbert
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.ServiceModel.Syndication;
    using System.Threading.Tasks;
    using System.Xml;

    public sealed class DailyDilbertService : IDailyDilbertService
    {
        private static readonly Uri RssFeedUri = new Uri("http://pipes.yahoo.com/pipes/pipe.run?_id=1fdc1d7a66bb004a2d9ebfedfb3808e2&_render=rss");
        
        private const string StripStartSegment = "http://www.dilbert.com/dyn/str_strip/";
        private const string StripEndSegment = "strip.zoom.gif";
        private const string DilbertFilename = "dilbert.jpg";

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

                if (date == fileDate)
                {
                    return filePath;
                }
            }

            var imageUri = await GetImageUriFromRssFeedAsync();

            await GetImageAndWriteToFileAsync(imageUri, filePath);

            return filePath;
        }

        private static async Task<Uri> GetImageUriFromRssFeedAsync()
        {
            using(var client = GetClient())
            using(var response = await client.GetAsync(RssFeedUri))
            using(var stream = await response.Content.ReadAsStreamAsync())
            using(var reader = new XmlTextReader(stream))
            {
                var feed = SyndicationFeed.Load(reader);
 
                // The items are in reverse date order - so the latest is first
                var latestItem = feed.Items.First();
 
                // The image url is buried in the Summary text - which is html of the form:
                // <a ref=".."><img src=".."></a>
                var summary = latestItem.Summary;
                var text = summary.Text;
 
                var startIndex = text.IndexOf(StripStartSegment);
                var endIndex = text.IndexOf(StripEndSegment) + StripEndSegment.Length;

                var length = endIndex - startIndex;
                var imageUrl = text.Substring(startIndex, length);
                return new Uri(imageUrl);
            }
        }

        private static async Task GetImageAndWriteToFileAsync(Uri imageUri, string filePath)
        {
            using(var client = GetClient())
            using(var response = await client.GetAsync(imageUri))
            using(var imageStream = await response.Content.ReadAsStreamAsync())
            using(var fileStream = File.Create(filePath))
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

            if (handler.Proxy != null)
            {
                handler.Proxy.Credentials = CredentialCache.DefaultCredentials;
            }

            var client = new HttpClient(handler, true);

            return client;
        }
    }
}