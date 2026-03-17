using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class UserListResponse
{
    public List<UserItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
