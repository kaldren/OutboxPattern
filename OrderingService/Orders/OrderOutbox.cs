namespace OrderingService.Orders;

public record OrderOutbox
(
    int Id,
    int AggregateId,
    string AggregateType,
    string EventType,
    string EventPayload,
    DateTimeOffset DateTimeOffset,
    bool Processed
);

