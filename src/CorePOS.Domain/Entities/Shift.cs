using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Events;

namespace CorePOS.Domain.Entities;

public class Shift : BaseEntity
{
    public string      ShiftNo        { get; set; } = string.Empty;
    public int         UserId         { get; set; }
    public int         BranchId       { get; set; }
    public int         CashBoxId      { get; set; }
    public decimal     OpeningBalance { get; set; }
    public decimal     ClosingBalance { get; set; }
    public decimal     ActualBalance  { get; set; }
    public DateTime    StartTime      { get; set; }
    public DateTime?   EndTime        { get; set; }
    public ShiftStatus Status         { get; set; } = ShiftStatus.Open;
    public string?     Notes          { get; set; }

    // Added properties (BUG-008)
    public int     InvoiceCount  { get; set; }
    public decimal TotalSales    { get; set; }
    public decimal TotalReturns  { get; set; }

    // Aliases for handler compatibility (BUG-008)
    public DateTime  OpenedAt { get => StartTime;  set => StartTime = value; }
    public DateTime? ClosedAt { get => EndTime;    set => EndTime   = value; }

    public User?    User    { get; private set; }
    public Branch?  Branch  { get; private set; }
    public CashBox? CashBox { get; private set; }

    protected Shift() { }

    public static Shift Create(string shiftNo, int userId, int branchId, int cashBoxId, decimal openingBalance = 0)
    {
        if (string.IsNullOrWhiteSpace(shiftNo)) throw new ArgumentException("Shift number is required.");
        return new Shift
        {
            ShiftNo = shiftNo.Trim(), UserId = userId,
            BranchId = branchId, CashBoxId = cashBoxId,
            OpeningBalance = openingBalance, StartTime = DateTime.Now,
            Status = ShiftStatus.Open
        };
    }

    public void Close(decimal actualBalance, string? notes = null)
    {
        if (Status == ShiftStatus.Closed)
            throw new InvalidOperationException("Shift is already closed.");
        Status         = ShiftStatus.Closed;
        ActualBalance  = actualBalance;
        EndTime        = DateTime.Now;
        Notes          = notes;
        AddDomainEvent(new ShiftClosedEvent(Id, ShiftNo, UserId, OpeningBalance, ActualBalance));
    }

    public void SetClosingBalance(decimal balance) => ClosingBalance = balance;

    public bool IsOpen   => Status == ShiftStatus.Open;
    public bool IsClosed => Status == ShiftStatus.Closed;

    public TimeSpan Duration => EndTime.HasValue
        ? EndTime.Value - StartTime
        : DateTime.Now - StartTime;
}
