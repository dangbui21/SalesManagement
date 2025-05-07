using System.Collections.Concurrent;

namespace PaymentService.Application.Services
{
    public class PaymentNotificationState
    {
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _notificationTasks = new();

        public bool TryAddTask(int orderId, CancellationTokenSource? cts)
        {
            if (cts == null)
            {
                return false;
            }
            return _notificationTasks.TryAdd(orderId, cts);
        }

        public bool TryRemoveTask(int orderId, out CancellationTokenSource? cts)
        {
            cts = null;
            return _notificationTasks.TryRemove(orderId, out cts);
        }

        public bool ContainsTask(int orderId)
        {
            return _notificationTasks.ContainsKey(orderId);
        }

        public void ClearAllTasks()
        {
            foreach (var cts in _notificationTasks.Values)
            {
                if (cts != null)
                {
                    try
                    {
                        cts.Cancel();
                        cts.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Ignore if already disposed
                    }
                }
            }
            _notificationTasks.Clear();
        }
    }
} 