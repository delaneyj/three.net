using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Loaders
{
    public delegate void LoadingCompletedHandler();
    public delegate void LoadingProgressHandler(JObject json, int loaded, int total);
    public delegate void LoadingExceptionHandler(Exception e);

    public class LoadingManager
    {
        public static LoadingManager DefaultLoadingManager = new LoadingManager();
        private Action onLoad;
        private Action<string, int, int> onProgress;
        private Action<Exception> onError;
        private int loaded, total;

        public LoadingManager(Action onLoad = null, Action<string, int, int> onProgress = null, Action<Exception> onError = null)
        {
            loaded = 0;
            total = 0;
            this.onLoad = onLoad;
            this.onProgress = onProgress;
            this.onError = onError;
        }

        void ItemStart(string url) 
        {
            total ++;
        }

        void ItemEnd(string url)
        {
            loaded++;

            if (onProgress != null) onProgress(url, loaded, total);
            if (loaded == total && onLoad != null) onLoad();
        }
    }
}