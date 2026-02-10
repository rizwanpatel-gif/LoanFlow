using LoanFlow.API.DTOs;

namespace LoanFlow.API.Services;

public interface IBorrowerService
{
    Task<BorrowerResponse> CreateAsync(CreateBorrowerRequest request);
    Task<BorrowerResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<BorrowerResponse>> GetAllAsync(int page, int pageSize);
    Task<BorrowerResponse?> UpdateAsync(Guid id, UpdateBorrowerRequest request);
    Task<bool> DeleteAsync(Guid id);
}
