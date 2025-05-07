using System.Collections.Concurrent;
using PaymentService.Domain.Events;
using PaymentService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace PaymentService.Application.Services
{
    public class PaymentNotificationService : IDisposable
    {
        private readonly IMessageBusPublisher _messageBus;
        private readonly ILogger<PaymentNotificationService> _logger;
        private readonly PaymentNotificationState _state;
        private readonly TimeSpan _notificationInterval = TimeSpan.FromSeconds(2);
        private bool _disposed;

        public PaymentNotificationService(
            IMessageBusPublisher messageBus,
            ILogger<PaymentNotificationService> logger,
            PaymentNotificationState state)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public bool ContainsTask(int orderId)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PaymentNotificationService));
            }
            return _state.ContainsTask(orderId);
        }

        public void StartSendingNotifications(int orderId, PaymentSucceededEventDto paymentEvent)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PaymentNotificationService));
            }

            if (paymentEvent == null)
            {
                throw new ArgumentNullException(nameof(paymentEvent));
            }

            if (_state.ContainsTask(orderId))
            {
                _logger.LogWarning("Notification task already exists for OrderId: {OrderId}", orderId);
                return;
            }

            var cts = new CancellationTokenSource();
            if (!_state.TryAddTask(orderId, cts))
            {
                cts.Dispose();
                return;
            }

            var token = cts.Token;

            // Tạo task mới và lưu lại
            var task = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Starting notification loop for OrderId: {OrderId}", orderId);
                    while (!token.IsCancellationRequested)
                    {
                        try 
                        {
                            await _messageBus.PublishPaymentSucceededAsync(paymentEvent);
                            _logger.LogInformation("Sent payment succeeded notification for OrderId: {OrderId}", orderId);
                            await Task.Delay(_notificationInterval, token);
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("Notification task cancelled for OrderId: {OrderId}", orderId);
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending payment notification for OrderId: {OrderId}", orderId);
                            // Thêm delay ngắn trước khi thử lại để tránh spam log
                            await Task.Delay(1000, token);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Notification task cancelled for OrderId: {OrderId}", orderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in notification task for OrderId: {OrderId}", orderId);
                }
                finally
                {
                    if (_state.TryRemoveTask(orderId, out var removedCts))
                    {
                        try
                        {
                            removedCts?.Cancel();
                            _logger.LogInformation("Notification task cancelled for OrderId: {OrderId}", orderId);
                        }
                        catch (ObjectDisposedException)
                        {
                            // Ignore if already disposed
                        }
                    }
                    _logger.LogInformation("Notification task finally block executed for OrderId: {OrderId}", orderId);
                }
            }, token);

            // Đợi task bắt đầu chạy
            Task.Delay(100).Wait();
        }

        public void StopSendingNotifications(int orderId)
        {
            _logger.LogInformation("StopSendingNotifications CALLED for OrderId: {OrderId}", orderId);

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PaymentNotificationService));
            }

            if (_state.TryRemoveTask(orderId, out var cts))
            {
                try
                {
                    cts?.Cancel();
                    // Đợi một chút để task có thể dừng an toàn
                    Task.Delay(100).Wait();
                    cts?.Dispose();
                    _logger.LogInformation("Stopped sending notifications for OrderId: {OrderId}", orderId);
                }
                catch (ObjectDisposedException)
                {
                    // Ignore if already disposed
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _state.ClearAllTasks();
                _disposed = true;
            }
        }
    }
} 