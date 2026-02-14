using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.FormBuilder.Queries.GetFormTemplateList;

public class GetFormTemplateListQueryHandler : IRequestHandler<GetFormTemplateListQuery, ApiResponse<FormTemplateListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetFormTemplateListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<FormTemplateListResponseDto>> Handle(GetFormTemplateListQuery request, CancellationToken cancellationToken)
    {
        var templateRepo = _unitOfWork.Repository<FormTemplate>();
        var fieldRepo = _unitOfWork.Repository<FormField>();
        var submissionRepo = _unitOfWork.Repository<FormSubmission>();

        // Build query using queryable for better performance
        var query = templateRepo.GetQueryable();

        // Filter by tenant or system templates
        if (_tenantContext.TenantId.HasValue)
        {
            query = query.Where(t => (t.TenantId == _tenantContext.TenantId.Value || t.IsSystemTemplate));
        }
        else
        {
            query = query.Where(t => t.IsSystemTemplate);
        }

        // Filter by active status
        if (!request.IncludeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        // Filter by category
        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(t => t.Category == request.Category);
        }

        // Get templates - need to materialize the query
        var templates = query.OrderBy(t => t.Name).ToList();

        // Get fields and submissions for each template
        var templateIds = templates.Select(t => t.Id).ToList();
        var allFields = await fieldRepo.FindAsync(f => templateIds.Contains(f.FormTemplateId) && f.IsActive, cancellationToken);
        var allSubmissions = await submissionRepo.FindAsync(s => templateIds.Contains(s.FormTemplateId), cancellationToken);

        var items = templates
            .OrderBy(t => t.Name)
            .Select(t => new FormTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                Category = t.Category,
                IsSystemTemplate = t.IsSystemTemplate,
                IsActive = t.IsActive,
                FieldCount = allFields.Count(f => f.FormTemplateId == t.Id),
                SubmissionCount = allSubmissions.Count(s => s.FormTemplateId == t.Id),
                CreatedDate = t.CreatedDate
            }).ToList();

        var response = new FormTemplateListResponseDto
        {
            Items = items,
            TotalCount = items.Count
        };

        return ApiResponse<FormTemplateListResponseDto>.SuccessResponse(response);
    }
}
