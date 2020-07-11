using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Driver.Mtrf64;
using MongoDB.Bson;

namespace Home.Web.Domain.Automation
{
    public class AutomationProcessorService : IAutomationProcessorService
    {
        private readonly Task _processingTask;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentDictionary<ObjectId, IAutomationItem> _automationItems;

        public AutomationProcessorService()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _processingTask = new Task(ProcessingRoutine, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
        }
        public void Initialize()
        {
            _processingTask.Start();
        }

        private void ProcessingRoutine()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _cancellationTokenSource.Cancel();
                    _processingTask.Dispose();
                    _cancellationTokenSource.Dispose();
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AutomationProcessorService()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
