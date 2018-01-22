using System;
using System.Data;
using System.Diagnostics;
using System.Security.Policy;
using System.Xml.Schema;
using Xunit;

namespace SiteCrawler.Test
{
    public class Tests
    {
        DownloadManager downloadManager = new DownloadManager();
        [Fact]
        public void Test1()
        {
            downloadManager = new DownloadManager();
            string initialUrl = @"http://localhost/api";
            var downloader = CreateDownloader(initialUrl);
            downloadManager.AddToDownloadQueue(downloader);
            downloadManager.Start(initialUrl);
            Assert.True(true);
        }

        private Downloader CreateDownloader(string initialUrl)
        {
            var downloader = new Downloader(initialUrl);
            downloader.LinkFound += DownloaderOnLinkFound;
            return downloader;
        }
        
        private Downloader CreateDownloader(Uri initialUrl)
        {
            var downloader = new Downloader(initialUrl);
            downloader.LinkFound += DownloaderOnLinkFound;
            return downloader;
        }

        private void DownloaderOnLinkFound(object sender, string foundLink)
        {
            var downloader = (Downloader) sender;
            Uri newDownloadUri = null;
            if (foundLink.StartsWith("http"))
            {
                 var uri = new Uri(foundLink);
                if (uri.Host == downloader.Host)
                {
                    Debug.Print($" can add {uri.ToString()} to download queue");
                    newDownloadUri = uri;
                }
            }
            else
            {
                var builder = new UriBuilder(downloader.Scheme, downloader.Host);
                builder.Path = foundLink;
                newDownloadUri = builder.Uri;
                Debug.Print($"also can add {newDownloadUri.ToString()} to download queue");
            }

            if (newDownloadUri != null)
            {
                var newDownloader = CreateDownloader(newDownloadUri);
                downloadManager.AddToDownloadQueue(newDownloader);
                downloadManager.Start(newDownloadUri.ToString());
            }
            
        }

        [Fact]
        public void TestParser()
        {
            var example =
                @"fsfsiffjef <a href = http://google.com sffwefewe >   < a href='/innerlink.html'  rrert>   <a href= /otherlink/other eeeeee > <a href =http://xxx/rrr.html>";
            var parser = new StringParser(example);
            var result = parser.ParseHrefs();
            Assert.NotNull(result);
            foreach (var s in result)
            {
               Debug.Print(s); 
            }
            
        }
    }
}