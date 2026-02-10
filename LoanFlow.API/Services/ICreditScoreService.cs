using LoanFlow.API.DTOs;

namespace LoanFlow.API.Services;

public interface ICreditScoreService
{
    Task<CreditScoreResponse> RunCreditCheckAsync(Guid borrowerId, CreditCheckRequest request);
    Task<CreditScoreResponse?> GetByBorrowerIdAsync(Guid borrowerId);
}
