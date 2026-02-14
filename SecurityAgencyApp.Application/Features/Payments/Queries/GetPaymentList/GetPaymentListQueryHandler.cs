using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentList;

public class GetPaymentListQueryHandler : IRequestHandler<GetPaymentListQuery, ApiResponse<PaymentListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetPaymentListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<PaymentListResponseDto>> Handle(GetPaymentListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<PaymentListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var paymentRepo = _unitOfWork.Repository<Payment>();
        var query = paymentRepo.GetQueryable()
            .Where(p => p.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || p.IsActive));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p =>
                p.PaymentNumber.ToLower().Contains(search) ||
                (p.TransactionReference != null && p.TransactionReference.ToLower().Contains(search)));
        }

        if (request.BillId.HasValue)
        {
            query = query.Where(p => p.BillId == request.BillId.Value);
        }

        if (request.ClientId.HasValue)
        {
            query = query.Where(p => p.ClientId == request.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(p => p.Status == request.Status);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= request.EndDate.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "paymentnumber" => request.SortDirection == "asc"
                ? query.OrderBy(p => p.PaymentNumber)
                : query.OrderByDescending(p => p.PaymentNumber),
            "paymentdate" => request.SortDirection == "asc"
                ? query.OrderBy(p => p.PaymentDate)
                : query.OrderByDescending(p => p.PaymentDate),
            "amount" => request.SortDirection == "asc"
                ? query.OrderBy(p => p.Amount)
                : query.OrderByDescending(p => p.Amount),
            _ => query.OrderByDescending(p => p.PaymentDate)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var payments = await paymentRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        // Get related data
        var billIds = payments.Where(p => p.BillId.HasValue).Select(p => p.BillId!.Value).Distinct().ToList();
        var clientIds = payments.Where(p => p.ClientId.HasValue).Select(p => p.ClientId!.Value).Distinct().ToList();

        var bills = billIds.Any()
            ? await _unitOfWork.Repository<Bill>().FindAsync(b => billIds.Contains(b.Id), cancellationToken)
            : new List<Bill>();

        var clients = clientIds.Any()
            ? await _unitOfWork.Repository<Client>().FindAsync(c => clientIds.Contains(c.Id), cancellationToken)
            : new List<Client>();

        var paymentDtos = payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            PaymentNumber = p.PaymentNumber,
            BillId = p.BillId,
            BillNumber = bills.FirstOrDefault(b => b.Id == p.BillId)?.BillNumber,
            ClientId = p.ClientId,
            ClientName = clients.FirstOrDefault(c => c.Id == p.ClientId)?.CompanyName,
            PaymentDate = p.PaymentDate,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status,
            ReceivedDate = p.ReceivedDate,
            IsActive = p.IsActive,
            CreatedDate = p.CreatedDate
        }).ToList();

        var response = new PaymentListResponseDto
        {
            Items = paymentDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<PaymentListResponseDto>.SuccessResponse(response, "Payments retrieved successfully");
    }
}
