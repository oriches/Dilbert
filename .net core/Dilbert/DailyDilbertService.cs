using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Dilbert.Common;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;

namespace Dilbert
{
    public sealed class DailyDilbertService : BaseDailyDilbertService
    {
        protected override async Task<Uri> GetImageUriFromRssFeedAsync(Uri feedUri)
        {
            using var client = GetClient();
            using var response = await client.GetAsync(feedUri);
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings {Async = true});
            var feedReader = new AtomFeedReader(xmlReader);
            while (await feedReader.Read())
            {
                if (feedReader.ElementType == SyndicationElementType.Item)
                {
                    var item = await feedReader.ReadContent();
                    if (string.Equals(item.Name, "entry", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var content = item.Fields.Last().Value;
                        var match = Regex.Match(content, ImgHtmlRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        if (match.Success)
                        {
                            var href = match.Groups[1].Value;
                            return href.StartsWith("http:") || href.StartsWith("https:")
                                ? new Uri(href)
                                : new Uri("http:" + href);
                        }

                    }
                }
            }

            return null;
        }

        protected override async Task GetImageAndWriteToFileAsync(Uri imageUri, string filePath)
        {
            using var client = GetClient();
            using var response = await client.GetAsync(imageUri);
            await using var imageStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(filePath);
            imageStream.CopyTo(fileStream);
        }
    }
}