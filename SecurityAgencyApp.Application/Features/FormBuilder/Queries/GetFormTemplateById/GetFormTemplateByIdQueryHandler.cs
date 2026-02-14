using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.FormBuilder.Queries.GetFormTemplateById;

public class GetFormTemplateByIdQueryHandler : IRequestHandler<GetFormTemplateByIdQuery, ApiResponse<FormTemplateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetFormTemplateByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<FormTemplateDto>> Handle(GetFormTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var templateRepo = _unitOfWork.Repository<FormTemplate>();
        var template = await templateRepo.GetByIdAsync(request.Id, cancellationToken);

        if (template == null)
        {
            return ApiResponse<FormTemplateDto>.ErrorResponse("Form template not found");
        }

        // Check tenant access (system templates are accessible to all)
        if (!template.IsSystemTemplate && 
            (!_tenantContext.TenantId.HasValue || template.TenantId != _tenantContext.TenantId.Value))
        {
            return ApiResponse<FormTemplateDto>.ErrorResponse("Form template not found");
        }

        // Get form fields
        var fields = await _unitOfWork.Repository<FormField>().FindAsync(
            f => f.FormTemplateId == template.Id && f.IsActive,
            cancellationToken);

        var templateDto = new FormTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Code = template.Code,
            Description = template.Description,
            Category = template.Category,
            IsSystemTemplate = template.IsSystemTemplate,
            IsActive = template.IsActive,
            Fields = fields
                .OrderBy(f => f.FieldOrder)
                .Select(f => new FormFieldDto
                {
                    Id = f.Id,
                    FieldName = f.FieldName,
                    FieldLabel = f.FieldLabel,
                    FieldType = f.FieldType.ToString(),
                    FieldOrder = f.FieldOrder,
                    IsRequired = f.IsRequired,
                    DefaultValue = f.DefaultValue,
                    Placeholder = f.Placeholder,
                    ValidationRules = f.ValidationRules,
                    Options = f.Options
                })
                .ToList()
        };

        return ApiResponse<FormTemplateDto>.SuccessResponse(templateDto, "Form template retrieved successfully");
    }
}
