using CorePOS.Domain.Common;

namespace CorePOS.Domain.Events;

public sealed class ShiftClosedEvent : BaseDomainEvent
{
    public int     ShiftId        { get; }
    public string  ShiftNo        { get; }
    public int     UserId         { get; }
    public decimal OpeningBalance { get; }
    public decimal ActualBalance  { get; }

    public ShiftClosedEvent(int shiftId, string shiftNo, int userId,
        decimal openingBalance, decimal actualBalance)
    {
        ShiftId        = shiftId;
        ShiftNo        = shiftNo;
        UserId         = userId;
        OpeningBalance = openingBalance;
        ActualBalance  = actualBalance;
    }
}
