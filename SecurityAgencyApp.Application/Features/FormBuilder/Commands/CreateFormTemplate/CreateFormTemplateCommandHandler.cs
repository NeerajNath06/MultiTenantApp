using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.FormBuilder.Commands.CreateFormTemplate;

public class CreateFormTemplateCommandHandler : IRequestHandler<CreateFormTemplateCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateFormTemplateCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateFormTemplateCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant or user context not found");
        }

        // Check if code already exists
        var templateRepo = _unitOfWork.Repository<FormTemplate>();
        var existing = await templateRepo.FirstOrDefaultAsync(
            ft => ft.Code == request.Code && 
                  (ft.TenantId == null || ft.TenantId == _tenantContext.TenantId.Value),
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Form template code already exists");
        }

        var template = new FormTemplate
        {
            TenantId = request.IsSystemTemplate ? null : _tenantContext.TenantId.Value,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Category = request.Category,
            IsSystemTemplate = request.IsSystemTemplate,
            IsActive = true,
            CreatedBy = _currentUserService.UserId.Value
        };

        await templateRepo.AddAsync(template, cancellationToken);

        // Add form fields
        foreach (var fieldDto in request.Fields.OrderBy(f => f.FieldOrder))
        {
            if (!Enum.TryParse<FormFieldType>(fieldDto.FieldType, out var fieldType))
            {
                continue; // Skip invalid field types
            }

            var field = new FormField
            {
                FormTemplateId = template.Id,
                FieldName = fieldDto.FieldName,
                FieldLabel = fieldDto.FieldLabel,
                FieldType = fieldType,
                FieldOrder = fieldDto.FieldOrder,
                IsRequired = fieldDto.IsRequired,
                DefaultValue = fieldDto.DefaultValue,
                Placeholder = fieldDto.Placeholder,
                ValidationRules = fieldDto.ValidationRules,
                Options = fieldDto.Options,
                IsActive = true
            };

            await _unitOfWork.Repository<FormField>().AddAsync(field, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(template.Id, "Form template created successfully");
    }
}
