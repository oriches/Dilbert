namespace Dilbert
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.ServiceModel.Syndication;
    using System.Threading.Tasks;
    using System.Xml;

    [Pure]
    public sealed class DailyDilbertService : IDailyDilbertService
    {
        private static readonly Uri OutToLunchUri = new Uri("http://www.greatamericanthings.net/wp-content/uploads/2012/02/Dilbert-by-stripturnhoutdotbe-300x225.jpg");

        private static readonly Uri RssFeed1Uri = new Uri("http://pipes.yahoo.com/pipes/pipe.run?_id=1fdc1d7a66bb004a2d9ebfedfb3808e2&_render=rss");
        private static readonly Uri RssFeed2Uri = new Uri("http://pipes.yahoo.com/pipes/pipe.run?_id=1627e842cee45e7358ef6b2a8530263a&_render=rss");

        private const string StripStartSegment1 = "http://www.dilbert.com/dyn/str_strip/";
        private const string StripStartSegment2 = "http://dilbert.com/dyn/str_strip/";
        private const string StripEndSegment = "strip.zoom.gif";
        private const string DilbertFilename = "dilbert.jpg";

        [Pure]
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

        [Pure]
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

            var imageUri = (await GetImageUriFromRssFeedAsync(RssFeed1Uri)
                ?? await GetImageUriFromRssFeedAsync(RssFeed2Uri))
                ?? OutToLunchUri;

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

                if (feed == null)
                {
                    return null;
                }

                // The items are in reverse date order - so the latest is first
                var latestItem = feed.Items.First();

                // The image url is buried in the Summary text - which is html of the form:
                // <a ref=".."><img src=".."></a>
                var summary = latestItem.Summary;
                var text = summary.Text;

                var startIndex = text.IndexOf(StripStartSegment1, StringComparison.Ordinal);
                if (startIndex == -1)
                {
                    startIndex = text.IndexOf(StripStartSegment2, StringComparison.Ordinal);
                }

                if (startIndex == -1)
                {
                    return null;
                }

                var endIndex = text.IndexOf(StripEndSegment, StringComparison.Ordinal) + StripEndSegment.Length;

                var length = endIndex - startIndex;
                var imageUrl = text.Substring(startIndex, length);
                return new Uri(imageUrl);
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

            if (handler.Proxy != null)
            {
                handler.Proxy.Credentials = CredentialCache.DefaultCredentials;
            }

            var client = new HttpClient(handler, true);

            return client;
        }
    }
}