namespace SecurityAgencyApp.Model.Api;

public class NotificationListResponse
{
    public List<NotificationItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
}
