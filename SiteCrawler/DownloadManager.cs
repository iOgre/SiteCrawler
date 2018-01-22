using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SiteCrawler
{
    public class DownloadManager
    {
        private int _maxParallel;

        public DownloadManager(int maxParallel = 5)
        {
            _maxParallel = maxParallel;
        }

        private readonly ConcurrentDictionary<string, Downloader> _downloadRequests = new ConcurrentDictionary<string, Downloader>();
        private List<Task> _tasks = new List<Task>();
        public int TasksCount => _tasks.Count;
        public void Start(string url)
        {
            try
            {
                if (_downloadRequests.TryGetValue(url, out Downloader downloaderToRun) && !downloaderToRun.IsRunning)
                {
                    var task = new Task(async () => await downloaderToRun.Process());
                   /* if (_tasks.Count == 1)
                    {
                        _tasks[0].ContinueWith(t => task);
                    }*/
                    task.ContinueWith(RemoveTask);
                    _tasks.Add(task);
                    task.Start();
                    Debug.Print($"added another task Id = {task.Id}");
                }
            }
            catch (AggregateException agr)
            {
               throw;
            }
            
        }

        private void RemoveTask(Task t)
        {
            Debug.Print($" Task with Id {t.Id} finished work and will be removed");
            _tasks.Remove(t);
            Debug.Print($" now we have  {_tasks.Count} tasks in queue");

        }

        public void AddToDownloadQueue(Downloader downloader)
        {
            if (!_downloadRequests.ContainsKey(downloader.Uri) && downloader.Depth < 5)
            {
                _downloadRequests.TryAdd(downloader.Uri, downloader);
            }
        }
    }
}