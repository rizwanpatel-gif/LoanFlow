using LoanFlow.API.DTOs;

namespace LoanFlow.API.Services;

public interface ILoanApplicationService
{
    Task<LoanApplicationResponse> CreateAsync(CreateLoanApplicationRequest request);
    Task<LoanApplicationResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<LoanApplicationResponse>> GetAllAsync(int page, int pageSize, string? status);
    Task<IEnumerable<LoanApplicationResponse>> GetByBorrowerIdAsync(Guid borrowerId);
    Task<LoanApplicationResponse?> ProcessDecisionAsync(Guid id, LoanDecisionRequest request);
}
