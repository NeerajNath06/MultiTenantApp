using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.TrainingRecords.Queries.GetTrainingRecordById;

public class GetTrainingRecordByIdQueryHandler : IRequestHandler<GetTrainingRecordByIdQuery, ApiResponse<TrainingRecordDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetTrainingRecordByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<TrainingRecordDetailDto>> Handle(GetTrainingRecordByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<TrainingRecordDetailDto>.ErrorResponse("Tenant context not found");

        var tr = await _unitOfWork.Repository<TrainingRecord>().GetByIdAsync(request.Id, cancellationToken);
        if (tr == null || tr.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<TrainingRecordDetailDto>.ErrorResponse("Training record not found");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(tr.GuardId, cancellationToken);
        var guardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : "";
        var guardCode = guard?.GuardCode ?? "";

        var dto = new TrainingRecordDetailDto
        {
            Id = tr.Id,
            GuardId = tr.GuardId,
            GuardName = guardName,
            GuardCode = guardCode,
            TrainingType = tr.TrainingType,
            TrainingName = tr.TrainingName,
            TrainingProvider = tr.TrainingProvider,
            TrainingDate = tr.TrainingDate,
            ExpiryDate = tr.ExpiryDate,
            Status = tr.Status,
            CertificateNumber = tr.CertificateNumber,
            Score = tr.Score,
            Remarks = tr.Remarks,
            IsActive = tr.IsActive,
            CreatedDate = tr.CreatedDate
        };
        return ApiResponse<TrainingRecordDetailDto>.SuccessResponse(dto, "Training record retrieved");
    }
}
