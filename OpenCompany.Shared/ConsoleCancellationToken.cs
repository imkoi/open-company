using System;
using System.Threading;

namespace OpenCompany.Shared
{
    public class ConsoleCancellationToken : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellationToken Token => _cancellationTokenSource.Token;

        public ConsoleCancellationToken()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        
            Console.CancelKeyPress += OnCancelKeyPress;
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
        
            _cancellationTokenSource.Cancel();
        }

        void IDisposable.Dispose()
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
            _cancellationTokenSource.Dispose();
        }
    }
}