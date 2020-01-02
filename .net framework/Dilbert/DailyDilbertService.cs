using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Dilbert.Common;

namespace Dilbert
{
    public sealed class DailyDilbertService : BaseDailyDilbertService
    {
        protected override async Task<Uri> GetImageUriFromRssFeedAsync(Uri feedUri)
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

        protected override async Task GetImageAndWriteToFileAsync(Uri imageUri, string filePath)
        {
            using (var client = GetClient())
            using (var response = await client.GetAsync(imageUri))
            using (var imageStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = File.Create(filePath))
            {
                imageStream.CopyTo(fileStream);
            }
        }
    }
}