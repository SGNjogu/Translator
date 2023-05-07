using System;
using System.Threading;
using System.Threading.Tasks;

namespace Translation.Services.Download
{
    public interface IDownloadService
    {
        Task DownloadFileAsync(string url, IProgress<double> progress, CancellationToken token , string filename);

        void CancelDownload(CancellationTokenSource cancellationToken);
    }
}