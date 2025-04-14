namespace OrderService.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        Completed = 2,
        Canceled = 3
    }

    public enum EventType
    {
        OrderCreated = 1,
        OrderUpdated = 2,
        OrderCanceled = 3
    }
}
