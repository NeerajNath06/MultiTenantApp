using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.TrainingRecords.Queries.GetTrainingRecordList;

public class GetTrainingRecordListQueryHandler : IRequestHandler<GetTrainingRecordListQuery, ApiResponse<TrainingRecordListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetTrainingRecordListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<TrainingRecordListResponseDto>> Handle(GetTrainingRecordListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<TrainingRecordListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var trainingRepo = _unitOfWork.Repository<TrainingRecord>();
        var query = trainingRepo.GetQueryable()
            .Where(tr => tr.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || tr.IsActive));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(tr =>
                tr.TrainingName.ToLower().Contains(search) ||
                (tr.TrainingProvider != null && tr.TrainingProvider.ToLower().Contains(search)) ||
                (tr.CertificateNumber != null && tr.CertificateNumber.ToLower().Contains(search)));
        }

        if (request.GuardId.HasValue)
        {
            query = query.Where(tr => tr.GuardId == request.GuardId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.TrainingType))
        {
            query = query.Where(tr => tr.TrainingType == request.TrainingType);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(tr => tr.Status == request.Status);
        }

        query = request.SortBy?.ToLower() switch
        {
            "trainingdate" => request.SortDirection == "asc"
                ? query.OrderBy(tr => tr.TrainingDate)
                : query.OrderByDescending(tr => tr.TrainingDate),
            "trainingname" => request.SortDirection == "asc"
                ? query.OrderBy(tr => tr.TrainingName)
                : query.OrderByDescending(tr => tr.TrainingName),
            _ => query.OrderByDescending(tr => tr.TrainingDate)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var trainingRecords = await trainingRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        // Get guard names
        var guardIds = trainingRecords.Select(tr => tr.GuardId).Distinct().ToList();
        var guards = guardIds.Any()
            ? await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => guardIds.Contains(g.Id), cancellationToken)
            : new List<SecurityGuard>();

        var trainingDtos = trainingRecords.Select(tr => new TrainingRecordDto
        {
            Id = tr.Id,
            GuardId = tr.GuardId,
            GuardName = $"{guards.FirstOrDefault(g => g.Id == tr.GuardId)?.FirstName} {guards.FirstOrDefault(g => g.Id == tr.GuardId)?.LastName}",
            GuardCode = guards.FirstOrDefault(g => g.Id == tr.GuardId)?.GuardCode ?? "",
            TrainingType = tr.TrainingType,
            TrainingName = tr.TrainingName,
            TrainingProvider = tr.TrainingProvider,
            TrainingDate = tr.TrainingDate,
            ExpiryDate = tr.ExpiryDate,
            Status = tr.Status,
            CertificateNumber = tr.CertificateNumber,
            Score = tr.Score,
            IsActive = tr.IsActive,
            CreatedDate = tr.CreatedDate
        }).ToList();

        var response = new TrainingRecordListResponseDto
        {
            Items = trainingDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<TrainingRecordListResponseDto>.SuccessResponse(response, "Training records retrieved successfully");
    }
}
