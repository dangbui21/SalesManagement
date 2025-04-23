namespace OrderService.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Completed = 1,
        Canceled = 2
    }

    public enum EventType
    {
        OrderCreated = 0,
        OrderUpdated = 1,
        OrderCanceled = 2
    }
}
