# Web → API Client Conversion Guide

Web ab **IApiClient** use karta hai taaki saari data **SecurityAgencyApp.API** se aaye. Jo controllers abhi bhi MediatR use kar rahe hain unhe isi pattern se convert karna hai.

## Done (API client use kar rahe hain)

- **AuthController** – Login via `POST api/v1/Auth/login`
- **HomeController** – Dashboard via `GET api/v1/Dashboard`
- **SitesController** – Full CRUD via `api/v1/Sites`
- **DepartmentsController** – Full CRUD via `api/v1/Departments`
- **DesignationsController** – Full CRUD via `api/v1/Designations`
- **RolesController** – Full CRUD + ManagePermissions + ManageMenus via `api/v1/Roles`, `api/v1/Permissions`, `api/v1/Menus`
- **Layout** – Sidebar menus via `GET api/v1/Menus` (ViewComponent)

## Conversion pattern (har naye controller ke liye)

1. **Web/Models/Api/XxxApiModels.cs** – List response, item DTO, Create/Update request (API response shape match kare).
2. **Controller** – `IMediator` hatao, `IApiClient _apiClient` inject karo.
3. **Index** – `_apiClient.GetAsync<XxxListResponse>("api/v1/Xxx", query)` → `View(result.Data)`.
4. **Create GET** – Dropdowns ke liye `_apiClient.GetAsync<YyyListResponse>("api/v1/Yyy", ...)` → ViewBag.
5. **Create POST** – `_apiClient.PostAsync<Guid>("api/v1/Xxx", body)`.
6. **Edit GET** – `_apiClient.GetAsync<XxxDto>($"api/v1/Xxx/{id}")` ya list se find by id.
7. **Edit POST** – `_apiClient.PutAsync<object>($"api/v1/Xxx/{id}", body)`.
8. **Delete** – `_apiClient.DeleteAsync($"api/v1/Xxx/{id}")`.
9. **Views** – `@model` ko `SecurityAgencyApp.Web.Models.Api.XxxListResponse` / `CreateXxxRequest` / `UpdateXxxRequest` par change karo.

## Remaining controllers (abhi MediatR use kar rahe hain)

| Controller           | API base path        | Notes                          |
|----------------------|----------------------|---------------------------------|
| UsersController      | api/v1/Users         | GetById, Create, Update, Delete |
| MenusController      | api/v1/Menus         | CRUD                           |
| SubMenusController   | api/v1/SubMenus      | CRUD                           |
| SecurityGuardsController | api/v1/SecurityGuards | CRUD                        |
| ShiftsController     | api/v1/Shifts        | CRUD                           |
| FormBuilderController| api/v1/FormBuilder   | List, CRUD                     |
| AttendanceController | api/v1/Attendance   | Index, Mark                    |
| GuardAssignmentsController | api/v1/GuardAssignments | List, Create              |
| IncidentsController  | api/v1/Incidents    | List, Create, Update           |
| BillsController      | api/v1/Bills        | List, GetById, Create, Update  |
| WagesController      | api/v1/Wages        | List, Create                   |
| ClientsController    | api/v1/Clients      | CRUD                           |
| ContractsController  | api/v1/Contracts    | CRUD                           |
| PaymentsController  | api/v1/Payments      | CRUD                           |
| LeaveRequestsController | api/v1/LeaveRequests | List, Create, Approve        |
| ExpensesController   | api/v1/Expenses     | List, Create                   |
| TrainingRecordsController | api/v1/TrainingRecords | List, Create               |
| EquipmentController  | api/v1/Equipment    | List, Create                   |

In sab ke liye API endpoints **SecurityAgencyApp.API** mein add ho chuke hain. Bas Web controllers ko upar wale pattern se **IApiClient** use karke convert karna hai.

## API base URL

`appsettings.json` → `ApiSettings:BaseUrl` (e.g. `https://localhost:5014`). Web har request mein session ka **Bearer token** bhejta hai.
