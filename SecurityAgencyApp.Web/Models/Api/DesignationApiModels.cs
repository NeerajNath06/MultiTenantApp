namespace SecurityAgencyApp.Web.Models.Api;

public class DesignationListResponse
{
    public List<DesignationDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}

public class DesignationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateDesignationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string? Description { get; set; }
}

public class UpdateDesignationRequest : CreateDesignationRequest
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
