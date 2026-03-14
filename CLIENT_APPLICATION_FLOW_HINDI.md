# Security Agency App: End-to-End Flow

## Document ka purpose
Yeh document client ko simple Hindi me samjhane ke liye hai ki application ka poora business flow kaise kaam karta hai, especially company setup se lekar bill banne tak.

## High-level flow
1. Sabse pehle company ka master setup hota hai.
2. Phir client, branch, site aur manpower structure define kiya jata hai.
3. Uske baad guards ko site aur shift ke sath assign kiya jata hai.
4. Daily attendance capture hoti hai.
5. Site ke rate plan ke basis par monthly billing aur wages generate hote hain.
6. Final output bill, attendance report, wage sheet aur payment tracking ke roop me milta hai.

## Flow step by step

### 1. Company onboarding aur profile setup
- Agency ki basic details `Company Profile` me maintain hoti hain.
- Yahin se company name, contact info, compliance documents, invoice-related profile data update hota hai.
- Documents upload bhi isi module se hota hai.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/CompanyProfileController.cs`
- `SecurityAgencyApp.Web/Views/CompanyProfile/Index.cshtml`

Business meaning:
- Yeh application ki top-level identity hai.
- Jo reports aur downloadable files bante hain, unme company profile ka data use hota hai.

### 2. Client master banana
- Har customer ya client ka record `Clients` module me banta hai.
- Yahan company name, billing details, GST/PAN, contact persons, billing cycle, credit period jaise fields save hote hain.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/ClientsController.cs`
- `SecurityAgencyApp.Web/Views/Clients/Create.cshtml`
- `SecurityAgencyApp.Web/Views/Clients/Index.cshtml`

Business meaning:
- Bill client ke naam par banta hai.
- Client configuration future billing aur contract flow ka base hai.

### 3. Branch setup
- Agency ki operational branches `Branches` module me maintain hoti hain.
- Branch ka use site mapping, operational ownership, aur payroll/report grouping me hota hai.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/BranchesController.cs`
- `SecurityAgencyApp.Web/Views/Branches/Index.cshtml`

Business meaning:
- Agar agency multiple cities ya operational zones me kaam karti hai, to branch segregation bahut important hota hai.

### 4. Site creation
- Real working locations `Sites` module me banti hain.
- Site me client mapping, branch mapping, address, contact person, emergency contact, geofence aur operational notes save hote hain.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/SitesController.cs`
- `SecurityAgencyApp.Web/Views/Sites/Create.cshtml`
- `SecurityAgencyApp.Web/Views/Sites/Edit.cshtml`

Business meaning:
- Site hi actual billing unit hai.
- Monthly bill aur wage generation mostly site-based hota hai.

### 5. Site post / sanctioned strength define karna
- Site par kitne guards chahiye, kis shift me chahiye, male/female requirement kya hai, reliever chahiye ya nahi, yeh `Site Posts` me define hota hai.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/SitePostsController.cs`
- `SecurityAgencyApp.Web/Views/SitePosts/Index.cshtml`

Business meaning:
- Yeh planning layer hai.
- Isse staffing expectation clear hoti hai aur operational management easy hota hai.

### 6. Security guard master banana
- Guard ki personal details, contact info, ID details, banking details, supervisor mapping, aur employment data `Security Guards` module me maintain hota hai.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/SecurityGuardsController.cs`
- `SecurityAgencyApp.Web/Views/SecurityGuards/Create.cshtml`
- `SecurityAgencyApp.Web/Views/SecurityGuards/Index.cshtml`

Business meaning:
- Attendance, wages aur assignment sab guard master par depend karte hain.

### 7. Shift setup
- Duty timing `Shifts` module me define hoti hai.
- Example: day shift, night shift, break duration.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/ShiftsController.cs`
- `SecurityAgencyApp.Web/Views/Shifts/Index.cshtml`

Business meaning:
- Assignment aur site staffing dono shift-aware hote hain.

### 8. Guard assignment
- Kisi guard ko kisi site aur shift ke sath assign karna `Guard Assignments` me hota hai.
- Assignment date range aur supervisor mapping bhi yahin hoti hai.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/GuardAssignmentsController.cs`
- `SecurityAgencyApp.Web/Views/GuardAssignments/Create.cshtml`
- `SecurityAgencyApp.Web/Views/GuardAssignments/Index.cshtml`

Business meaning:
- Attendance aur payroll ke liye guard ka active assignment hona zaroori hai.

### 9. Attendance capture
- Daily attendance `Attendance` module me mark hoti hai.
- Manual mark, check-in/check-out, status, remarks sab store hota hai.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/AttendanceController.cs`
- `SecurityAgencyApp.Web/Views/Attendance/Mark.cshtml`
- `SecurityAgencyApp.Application/Features/Attendance/Commands/MarkAttendance/MarkAttendanceCommandHandler.cs`

Business meaning:
- Billing aur wages ka sabse important transactional input attendance hai.
- Monthly generation me `Present` attendance ko billable/payable maana gaya hai.

### 10. Site rate plan setup
- Har site ka per-day rate aur payroll percentages `Site Rates` ya site edit screen ke rate section me set kiya jata hai.
- Rate plan me billing rate ke sath allowance, EPF, ESIC aur wage cap jaise payroll factors bhi save ho sakte hain.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/SiteRatesController.cs`
- `SecurityAgencyApp.Web/Controllers/SitesController.cs`
- `SecurityAgencyApp.Application/Features/SiteRates/Commands/UpsertSiteRatePlan/UpsertSiteRatePlanCommandHandler.cs`

Business meaning:
- Monthly bill amount aur wage amount dono isi rate plan par based hote hain.

### 11. Monthly document generation
- Site level par monthly report / generation action available hai.
- User path:
  `Sites -> Report button / Edit screen -> Generate Report -> Generate Bill / Attendance / Wages / Generate All`
- `Generate All` path ek hi run me bill aur wage sheet generate karta hai.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/SitesController.cs`
- `SecurityAgencyApp.Application/Features/MonthlyDocuments/Commands/GenerateMonthlyDocuments/GenerateMonthlyDocumentsCommandHandler.cs`
- `SecurityAgencyApp.API/Services/MonthlyReportService.cs`

Business meaning:
- System selected month ke liye:
  - site ka active rate plan leta hai
  - active assignments leta hai
  - selected period ki `Present` attendance count karta hai
  - uske basis par bill line items banata hai
  - wage sheet aur wage details bhi generate karta hai

### 12. Bill creation
- Bill do tarah se aa sakta hai:
  1. Manual bill creation from `Bills/Create`
  2. Auto-generated monthly bill from site report/generation flow

Manual flow relevant files:
- `SecurityAgencyApp.Web/Controllers/BillsController.cs`
- `SecurityAgencyApp.Web/Views/Bills/Create.cshtml`
- `SecurityAgencyApp.API/Controllers/v1/BillsController.cs`
- `SecurityAgencyApp.Application/Features/Bills/Commands/CreateBill/CreateBillCommandHandler.cs`

Auto flow relevant files:
- `SecurityAgencyApp.Application/Features/MonthlyDocuments/Commands/GenerateMonthlyDocuments/GenerateMonthlyDocumentsCommandHandler.cs`

Business meaning:
- Manual bill custom invoice ke liye useful hai.
- Auto-generated bill monthly attendance-driven invoice ke liye useful hai.

### 13. Wage sheet creation
- Wage sheet bhi do modes me ho sakti hai:
  1. Manual `Wages/Create`
  2. Monthly auto-generation through site report flow

Relevant files:
- `SecurityAgencyApp.Web/Controllers/WagesController.cs`
- `SecurityAgencyApp.Application/Features/Wages/Commands/CreateWage/CreateWageCommandHandler.cs`
- `SecurityAgencyApp.Application/Features/Wages/Queries/GetWageWithDetails/GetWageWithDetailsQueryHandler.cs`

Business meaning:
- Client billing aur guard payout dono monthly operations ka part hain.

### 14. Payment tracking
- Client se payment receive hone ke baad `Payments` module me entry hoti hai.
- Isse outstanding aur completed billing cycle track hota hai.

Relevant files:
- `SecurityAgencyApp.Web/Controllers/PaymentsController.cs`
- `SecurityAgencyApp.Web/Views/Payments/Create.cshtml`

Business meaning:
- Billing cycle complete tab hota hai jab bill raise ho aur payment record ho jaye.

## Practical client demo flow
Client ko application samjhane ke liye yeh sequence use kiya ja sakta hai:

1. `Company Profile` kholiye aur agency details dikhaiye.
2. `Clients` me client master dikhaiye.
3. `Branches` me operational branch structure dikhaiye.
4. `Sites` me ek site open karke client-site mapping dikhaiye.
5. `Site Posts` me sanctioned strength aur requirement dikhaiye.
6. `Security Guards` me manpower master dikhaiye.
7. `Guard Assignments` me site assignment dikhaiye.
8. `Attendance` me daily attendance entry ya list dikhaiye.
9. `Sites` se report/billing action open kijiye.
10. `Generate Bill` ya `Generate All` dikhaiye.
11. `Bills` module me generated bill dikhaiye.
12. `Payments` module me payment tracking explain kijiye.

## Jo important runtime points verify kiye gaye
- Login local smoke test me successful tha.
- Dashboard load hua.
- `Company Profile`, `Clients`, `Branches`, `Sites`, `SitePosts`, `GuardAssignments`, `Attendance`, `Bills`, `Bills/Create`, `Reports/MonthlyExcel` pages successfully load hui.
- Site report modal se monthly bill/report generation action chal kar Excel download successfully mila.

## Issues jo mile aur fix kiye gaye

### 1. Operations menu me `Branches` submenu missing tha
- Existing tenant/role mapping me `Branches` submenu sab relevant roles ko auto-assign nahi ho raha tha.
- Fix `DbInitializer` me kiya gaya, taaki startup sync ke time `Operations` access rakhne wale non-guard roles ko submenu mapping mile.

Relevant file:
- `SecurityAgencyApp.Infrastructure/Data/DbInitializer.cs`

### 2. Site-specific report generation me wrong wage sheet pick hone ka risk tha
- Wage lookup site filter ke bina latest sheet utha sakta tha.
- Fix karke `SiteId` filtering add ki gayi.

Relevant files:
- `SecurityAgencyApp.Application/Features/Wages/Queries/GetWageList/GetWageListQuery.cs`
- `SecurityAgencyApp.Application/Features/Wages/Queries/GetWageList/GetWageListQueryHandler.cs`
- `SecurityAgencyApp.API/Controllers/v1/WagesController.cs`
- `SecurityAgencyApp.API/Services/MonthlyReportService.cs`
- `SecurityAgencyApp.API/Controllers/v1/ReportsController.cs`

### 3. Report action discoverability weak thi
- `Sites/Index` page par modal already tha, lekin row action button visible nahi tha.
- Fix karke direct report button add kiya gaya.

Relevant file:
- `SecurityAgencyApp.Web/Views/Sites/Index.cshtml`

## Client ko simple language me final explanation
Application ka main logic yeh hai:

- Pehle company aur client ka master data banta hai.
- Fir branch aur site structure banaya jata hai.
- Fir guards, shifts aur site posts define kiye jate hain.
- Fir guards ko assignments diye jate hain.
- Daily attendance maintain hoti hai.
- Site ke rate plan ke basis par month-end par bill aur wage sheet generate hoti hai.
- Fir payment record karke cycle close ki jati hai.

Simple sentence me:
`Setup -> Staffing -> Attendance -> Rate Plan -> Bill/Wages -> Payment`

## Demo ke liye recommended path
`Login -> Company Profile -> Clients -> Branches -> Sites -> Site Posts -> Guard Assignments -> Attendance -> Sites Report Button -> Generate Bill -> Bills -> Payments`

