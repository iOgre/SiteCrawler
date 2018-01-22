using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace SiteCrawler.Terminal
{
    internal class Program
    {
        static DownloadManager downloadManager = new DownloadManager();
        private static DirectoryInfo saveDir = null;

        public static void HelpMessage()
        {
            Console.WriteLine("Usage information");
            Console.WriteLine("-p <download site url>");
            Console.WriteLine("-s Where to save");
        }
        public static void Main(string[] args)
        {
            if (args == null)
            {
                HelpMessage();
                return;
            }
            int hostIndex = Array.IndexOf(args, "-p");
            if (hostIndex == -1 || hostIndex + 1 >= args.Length || String.IsNullOrEmpty(args[hostIndex+1]))
            {
                HelpMessage();
                return;
            }
            var savePathIndex = Array.IndexOf(args, "-s");
            var initialUrl = new Uri(args[hostIndex+1]);
            var rootFolder = savePathIndex != -1 && savePathIndex +1 < args.Length && !String.IsNullOrEmpty(args[savePathIndex +1])
                             ? args[savePathIndex + 1]
                :
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string pathToSave = Path.Combine(rootFolder, initialUrl.Host);

            downloadManager = new DownloadManager();
            
            var downloader = CreateDownloader(initialUrl);
            downloadManager.AddToDownloadQueue(downloader);
            saveDir = new DirectoryInfo(pathToSave);
            if (!saveDir.Exists)
            {
                saveDir.Create();
            }
            
            downloadManager.Start(initialUrl.AbsoluteUri);
            while (downloadManager.TasksCount > 0) ;
            Console.WriteLine("All done");
        }

       

        private static void DownloaderOnDownloadFinished(object sender, Page page)
        {
            Debug.Print($" Ready to save: {page.Name}");
            var pathParts = page.Name.Split('/');
            var subdir = saveDir.CreateSubdirectory(page.Name);
            var fileName = new FileInfo(pathParts.Last());
            if (String.IsNullOrWhiteSpace(fileName.Extension))
            {
                fileName = new FileInfo(Path.ChangeExtension(fileName.Name, "html"));
            }
            var path = Path.Combine(subdir.FullName, fileName.Name);
          
            File.WriteAllText(path, page.Html);
        }

        private static Downloader CreateDownloader(Uri initialUrl)
        {
            var downloader = new Downloader(initialUrl);
            downloader.LinkFound += DownloaderOnLinkFound;
            downloader.DownloadFinished += DownloaderOnDownloadFinished;
            return downloader;
        }

        private static void DownloaderOnLinkFound(object sender, string foundLink)
        {
            var downloader = (Downloader) sender;
            Uri newDownloadUri = null;
            if (foundLink.StartsWith("http"))
            {
                var uri = new Uri(foundLink);
                if (uri.Host == downloader.Host)
                {
                   // Debug.Print($" can add {uri.ToString()} to download queue");
                    newDownloadUri = uri;
                }
            }
            else
            {
                var builder = new UriBuilder(downloader.Scheme, downloader.Host);
                builder.Path = foundLink;
                newDownloadUri = builder.Uri;
              //  Debug.Print($"also can add {newDownloadUri.ToString()} to download queue");
            }

            if (newDownloadUri != null)
            {
                var newDownloader = CreateDownloader(newDownloadUri);
                downloadManager.AddToDownloadQueue(newDownloader);
                downloadManager.Start(newDownloadUri.ToString());
            }
        }
    }
}