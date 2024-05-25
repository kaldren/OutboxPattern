namespace OrderingService;

public class Outbox
{
    public int Id { get; set; }
    public int AggregateId { get; set; }
    public string AggregateType { get; set; }
    public string EventType { get; set; }
    public string EventPayload { get; set; }
    public DateTimeOffset DateTimeOffset { get; set; }
    public bool Processed { get; set; }
}
