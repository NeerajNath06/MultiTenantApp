using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class TrainingRecordsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? guardId = null, string? trainingType = null)
    {
        return View(new TrainingRecordListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateTrainingRecordRequest { TrainingDate = DateTime.UtcNow, Status = "Completed" });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.TrainingRecordId = id;
        return View();
    }
}
