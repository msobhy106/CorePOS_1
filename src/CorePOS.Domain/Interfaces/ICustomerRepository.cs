using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Customer?> GetByPhoneAsync(string phone, CancellationToken ct = default);
    Task<Customer?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> SearchAsync(string term, int maxResults = 20, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetWithDebtAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetByGroupAsync(int groupId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
    Task UpdateBalanceAsync(int customerId, decimal newBalance, CancellationToken ct = default);
    Task UpdatePointsAsync(int customerId, decimal newPoints, CancellationToken ct = default);

    // Added methods (BUG-011)
    Task<IReadOnlyList<Customer>> GetAllWithBalanceAsync(int branchId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerPayment>> GetPaymentsByCustomerAsync(int customerId, DateTime from, DateTime to, CancellationToken ct = default);
}
