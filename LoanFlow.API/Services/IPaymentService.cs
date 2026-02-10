using LoanFlow.API.DTOs;

namespace LoanFlow.API.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponse>> GenerateScheduleAsync(Guid loanApplicationId);
    Task<IEnumerable<PaymentResponse>> GetByLoanIdAsync(Guid loanApplicationId);
    Task<PaymentResponse?> MakePaymentAsync(Guid paymentId, MakePaymentRequest request);
    Task<PaymentSummaryResponse?> GetSummaryAsync(Guid loanApplicationId);
}
