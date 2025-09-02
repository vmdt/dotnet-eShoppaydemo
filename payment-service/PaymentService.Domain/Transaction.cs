using BuildingBlocks.Core.Core.Model;

namespace PaymentService.Domain;

public record Transaction : Aggregate<Guid>
{
    public Guid UserId { get; set; }
    public Int64 Amount { get; set; }
    public string Currency { get; set; }
    public TransactionStatus Status { get; set; }
    public string PaymentMethod { get; set; }
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}
