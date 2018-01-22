using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SiteCrawler
{
    public class Page
    {
        public string Html { get; set; }
        public string Name { get; set; }
    }
    public class Downloader
    {
        private readonly Uri _uri;
        private const string link = "a href";
        public string Uri => _uri.ToString();
        public string Host => _uri.Host;
        public string Scheme => _uri.Scheme;
        public bool IsRunning { get; private set; } = false;
        public int Depth => _uri.Segments.Length;

        public Downloader(string url)
        {
            this._uri = new Uri(url);
        }

        public Downloader(Uri uri)
        {
            _uri = uri;
        }
        public event EventHandler<string> LinkFound;
        public event EventHandler<Page> DownloadFinished;
        private void OnLinkFound(string url)
        {
            LinkFound?.Invoke(this, url);
        }
        
        
        private void OnDownloadFinished(Page page)
        {
            DownloadFinished?.Invoke(this, page);
        }
        public async Task Process()
        {
            await InnerProcess();
        }

        private async Task InnerProcess()
        {
            IsRunning = true;
            var request = WebRequest.CreateHttp(_uri);
            Debug.Print($" URL is : {_uri.ToString()}, download process started");
            StringBuilder result = new StringBuilder();
            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    Debug.Print($"Can read : {stream.CanRead}");
                    using (var reader = new StreamReader(stream))
                    {
                        string singleLine;
                        while ((singleLine = await reader.ReadLineAsync()) != null)
                        {
                            result.AppendLine(singleLine);
                            var parser = new StringParser(singleLine);
                            var hrefs = parser.ParseHrefs();
                            foreach (var href in hrefs)
                            {
                                OnLinkFound(href);
                            }
                        }
                    }
                }
            }

            var page = new Page
            {
                Html = result.ToString(),
                Name = _uri.LocalPath.Trim('/')
            };
            OnDownloadFinished(page);
            Debug.Print($"Download process finished, for {_uri.ToString()}");
        }
    }
    
}