using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SecurityAgencyApp.Application.Common;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Enums;
using SecurityAgencyApp.Infrastructure.Data;

namespace SecurityAgencyApp.API.HostedServices;

/// <summary>
/// Enterprise: (1) Mark assignments as Completed when End Date has passed;
/// (2) Auto-complete attendance (set CheckOutTime = shift end) when shift time has ended;
/// (3) Send next-shift notifications to guards.
/// </summary>
public class AssignmentAndAttendanceBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AssignmentAndAttendanceBackgroundService> _logger;
    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(15);

    public AssignmentAndAttendanceBackgroundService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<AssignmentAndAttendanceBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // allow app to start
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var today = AppTimeHelper.TodayInAppTimeZone();
                var now = AppTimeHelper.NowInAppTimeZone();

                // (1) Mark assignments as Completed when AssignmentEndDate has passed
                var assignmentsToComplete = await db.GuardAssignments.IgnoreQueryFilters()
                    .Where(a => a.Status == AssignmentStatus.Active && a.AssignmentEndDate != null && a.AssignmentEndDate.Value.Date < today.Date)
                    .ToListAsync(stoppingToken);
                foreach (var a in assignmentsToComplete)
                {
                    a.Status = AssignmentStatus.Completed;
                    db.GuardAssignments.Update(a);
                }
                if (assignmentsToComplete.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Marked {Count} assignment(s) as Completed (end date passed).", assignmentsToComplete.Count);
                }

                // (2) Auto-complete attendance: CheckIn done but no CheckOut, and shift end time has passed
                var openAttendances = await db.GuardAttendances.IgnoreQueryFilters()
                    .Where(ga => ga.CheckInTime != null && ga.CheckOutTime == null)
                    .Select(ga => new { ga.Id, ga.AttendanceDate, ga.AssignmentId, ga.CheckInLocation })
                    .ToListAsync(stoppingToken);
                var assignmentIds = openAttendances.Select(x => x.AssignmentId).Distinct().ToList();
                Dictionary<Guid, TimeSpan> assignmentsWithShifts;
                if (assignmentIds.Count == 0)
                {
                    assignmentsWithShifts = new Dictionary<Guid, TimeSpan>();
                }
                else
                {
                    var assignList = await db.GuardAssignments.IgnoreQueryFilters()
                        .Where(a => assignmentIds.Contains(a.Id))
                        .Select(a => new { a.Id, a.ShiftId })
                        .ToListAsync(stoppingToken);
                    var shiftIds = assignList.Select(x => x.ShiftId).Distinct().ToList();
                    var shiftEnds = await db.Shifts.IgnoreQueryFilters()
                        .Where(s => shiftIds.Contains(s.Id))
                        .ToDictionaryAsync(s => s.Id, s => s.EndTime, stoppingToken);
                    assignmentsWithShifts = assignList.Where(a => shiftEnds.ContainsKey(a.ShiftId))
                        .ToDictionary(a => a.Id, a => shiftEnds[a.ShiftId]);
                }
                var updatedCount = 0;
                foreach (var ga in openAttendances)
                {
                    if (!assignmentsWithShifts.TryGetValue(ga.AssignmentId, out var endTime)) continue;
                    var shiftEndToday = ga.AttendanceDate.Date.Add(endTime);
                    if (now > shiftEndToday)
                    {
                        var entity = await db.GuardAttendances.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Id == ga.Id, stoppingToken);
                        if (entity != null)
                        {
                            entity.CheckOutTime = shiftEndToday;
                            entity.CheckOutLocation = ga.CheckInLocation;
                            db.GuardAttendances.Update(entity);
                            updatedCount++;
                        }
                    }
                }
                if (updatedCount > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Auto-completed {Count} attendance record(s) (shift end time passed).", updatedCount);
                }

                // (3) Next-shift notifications: send when shift starts in the next 24 hours (so guard gets "apki duty 2 baje" / upcoming duty).
                // Also send when in the next N minutes (App:NextShiftNotificationMinutesBefore). Dedupe by same UserId + Body.
                var minutesBefore = _configuration.GetValue("App:NextShiftNotificationMinutesBefore", 60);
                if (minutesBefore <= 0) minutesBefore = 60;
                var windowEnd24h = now.AddHours(24);
                var todayDate = today.Date;
                var tomorrowDate = todayDate.AddDays(1);
                var activeAssignments = await db.GuardAssignments.IgnoreQueryFilters()
                    .Where(a => a.Status == AssignmentStatus.Active &&
                                (a.AssignmentEndDate == null || a.AssignmentEndDate.Value.Date >= todayDate))
                    .Select(a => new { a.Id, a.TenantId, a.GuardId, a.SiteId, a.ShiftId, a.AssignmentStartDate, a.AssignmentEndDate })
                    .ToListAsync(stoppingToken);
                if (activeAssignments.Count > 0)
                {
                    var shiftIds = activeAssignments.Select(a => a.ShiftId).Distinct().ToList();
                    var siteIds = activeAssignments.Select(a => a.SiteId).Distinct().ToList();
                    var guardIds = activeAssignments.Select(a => a.GuardId).Distinct().ToList();
                    var shifts = await db.Shifts.IgnoreQueryFilters().Where(s => shiftIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, stoppingToken);
                    var sites = await db.Sites.IgnoreQueryFilters().Where(s => siteIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, stoppingToken);
                    var guards = await db.SecurityGuards.IgnoreQueryFilters().Where(g => guardIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, stoppingToken);
                    var sent = 0;
                    foreach (var a in activeAssignments)
                    {
                        if (!guards.TryGetValue(a.GuardId, out var guard) || guard.UserId == null) continue;
                        if (!sites.TryGetValue(a.SiteId, out var site)) continue;
                        if (!shifts.TryGetValue(a.ShiftId, out var shift)) continue;
                        var startDate = a.AssignmentStartDate.Date;
                        var endDate = a.AssignmentEndDate?.Date ?? tomorrowDate.AddDays(30);
                        var siteName = site.SiteName ?? "Site";

                        // (3a) "Today's duty" – one per guard per day when they have a shift today (so guard sees "apki duty 2 baje" on login).
                        if (todayDate >= startDate && (a.AssignmentEndDate == null || a.AssignmentEndDate.Value.Date >= todayDate))
                        {
                            var startTimeStr = shift.StartTime.ToString(@"hh\:mm");
                            var bodyToday = $"Your duty today at {siteName} is at {startTimeStr}. Address: {site.Address ?? "—"}.";
                            var cutoff = DateTime.UtcNow.AddHours(-24);
                            var alreadyToday = await db.Notifications.IgnoreQueryFilters()
                                .AnyAsync(n => n.UserId == guard.UserId && n.TenantId == a.TenantId && n.Title == "Today's duty" && n.CreatedDate >= cutoff, stoppingToken);
                            if (!alreadyToday)
                            {
                                db.Notifications.Add(new Notification
                                {
                                    Id = Guid.NewGuid(),
                                    TenantId = a.TenantId,
                                    UserId = guard.UserId.Value,
                                    Title = "Today's duty",
                                    Body = bodyToday,
                                    Type = "Info",
                                    IsRead = false,
                                    CreatedDate = DateTime.UtcNow
                                });
                                sent++;
                            }
                        }

                        // (3b) "Next shift reminder" when shift starts within next 24 hours (dedupe by Body).
                        for (var d = todayDate; d <= tomorrowDate && d <= endDate; d = d.AddDays(1))
                        {
                            if (d < startDate) continue;
                            var shiftStart = d.Add(shift.StartTime);
                            if (shiftStart <= now) continue;
                            if (shiftStart > windowEnd24h) continue;
                            var startTimeStr = shift.StartTime.ToString(@"hh\:mm");
                            var body = $"Your next shift at {siteName} starts at {startTimeStr} on {d:dd-MMM-yyyy}. Address: {site.Address ?? "—"}.";
                            var existing = await db.Notifications.IgnoreQueryFilters()
                                .AnyAsync(n => n.UserId == guard.UserId && n.Body == body, stoppingToken);
                            if (existing) continue;
                            db.Notifications.Add(new Notification
                            {
                                Id = Guid.NewGuid(),
                                TenantId = a.TenantId,
                                UserId = guard.UserId.Value,
                                Title = "Next shift reminder",
                                Body = body,
                                Type = "Info",
                                IsRead = false,
                                CreatedDate = DateTime.UtcNow
                            });
                            sent++;
                            break;
                        }
                    }
                    if (sent > 0)
                    {
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Sent {Count} next-shift/today-duty notification(s) to guards.", sent);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Assignment/attendance background job failed.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }
}
