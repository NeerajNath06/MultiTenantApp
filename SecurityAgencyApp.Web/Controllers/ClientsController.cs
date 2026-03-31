using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ClientsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(new ClientListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateClientRequest { Status = "Active" });
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateClientRequest
        {
            Id = id,
            Status = "Active"
        });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.ClientId = id;
        return View();
    }
}
