using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentList;

public class GetPaymentListQuery : IRequest<ApiResponse<PaymentListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public Guid? BillId { get; set; }
    public Guid? ClientId { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

public class PaymentListResponseDto
{
    public List<PaymentDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid? BillId { get; set; }
    public string? BillNumber { get; set; }
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ReceivedDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
