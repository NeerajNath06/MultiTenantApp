using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.TrainingRecords.Commands.CreateTrainingRecord;

public class CreateTrainingRecordCommandHandler : IRequestHandler<CreateTrainingRecordCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateTrainingRecordCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateTrainingRecordCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Validate guard
        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<Guid>.ErrorResponse("Invalid security guard");
        }

        var trainingRecord = new TrainingRecord
        {
            TenantId = _tenantContext.TenantId.Value,
            GuardId = request.GuardId,
            TrainingType = request.TrainingType,
            TrainingName = request.TrainingName,
            TrainingProvider = request.TrainingProvider,
            TrainingDate = request.TrainingDate,
            ExpiryDate = request.ExpiryDate,
            Status = request.Status,
            CertificateNumber = request.CertificateNumber,
            Score = request.Score,
            Remarks = request.Remarks,
            IsActive = true
        };

        await _unitOfWork.Repository<TrainingRecord>().AddAsync(trainingRecord, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(trainingRecord.Id, "Training record created successfully");
    }
}
