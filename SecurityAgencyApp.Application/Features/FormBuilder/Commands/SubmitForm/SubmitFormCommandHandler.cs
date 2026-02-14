using MediatR;
using System.Text.Json;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.FormBuilder.Commands.SubmitForm;

public class SubmitFormCommandHandler : IRequestHandler<SubmitFormCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public SubmitFormCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<Guid>> Handle(SubmitFormCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant or user context not found");
        }

        // Verify form template exists
        var templateRepo = _unitOfWork.Repository<FormTemplate>();
        var template = await templateRepo.GetByIdAsync(request.FormTemplateId, cancellationToken);

        if (template == null || !template.IsActive)
        {
            return ApiResponse<Guid>.ErrorResponse("Form template not found or inactive");
        }

        // Check tenant access
        if (!template.IsSystemTemplate && template.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<Guid>.ErrorResponse("Form template not found");
        }

        // Get form fields for validation
        var fields = await _unitOfWork.Repository<FormField>().FindAsync(
            f => f.FormTemplateId == template.Id && f.IsActive,
            cancellationToken);

        // Validate required fields
        foreach (var field in fields.Where(f => f.IsRequired))
        {
            if (!request.Data.ContainsKey(field.FieldName) || 
                request.Data[field.FieldName] == null ||
                string.IsNullOrWhiteSpace(request.Data[field.FieldName]?.ToString()))
            {
                return ApiResponse<Guid>.ErrorResponse($"Field '{field.FieldLabel}' is required");
            }
        }

        // Create form submission
        var submission = new FormSubmission
        {
            TenantId = _tenantContext.TenantId.Value,
            FormTemplateId = template.Id,
            SubmittedBy = _currentUserService.UserId.Value,
            SubmissionDate = DateTime.UtcNow,
            Status = FormSubmissionStatus.Submitted,
            Remarks = request.Remarks
        };

        await _unitOfWork.Repository<FormSubmission>().AddAsync(submission, cancellationToken);

        // Save form data
        foreach (var field in fields)
        {
            if (request.Data.ContainsKey(field.FieldName))
            {
                var fieldValue = request.Data[field.FieldName];
                var valueString = fieldValue is string str ? str : JsonSerializer.Serialize(fieldValue);

                var submissionData = new FormSubmissionData
                {
                    FormSubmissionId = submission.Id,
                    FormFieldId = field.Id,
                    FieldValue = valueString
                };

                await _unitOfWork.Repository<FormSubmissionData>().AddAsync(submissionData, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(submission.Id, "Form submitted successfully");
    }
}
