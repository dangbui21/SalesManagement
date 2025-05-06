using ReportService.Domain.Events;
using System.Threading.Tasks;

namespace ReportService.Domain.Interfaces
{
    public interface IHandleOrderCompleted
    {
        Task HandleAsync(OrderCompletedEvent orderCompletedEvent);
    }
}
