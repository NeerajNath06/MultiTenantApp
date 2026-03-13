# Enterprise Gap Analysis

## Current coverage

The application already includes a strong MVP-to-mid-market base for a multi-tenant security agency product:

- tenant registration and tenant-scoped data filtering
- users, roles, permissions, menus, and supervisor visibility
- guards, sites, shifts, assignments, attendance, incidents, visitors, vehicle logs, patrol scans, and forms
- clients, contracts, bills, payments, wages, expenses, and monthly document generation
- tenant documents, guard documents, training, announcements, notifications, and audit logs

This means the platform can already run a single or small group of agencies with shared product code and isolated tenant data.

## Enterprise gaps by workflow

### 1. Agency onboarding and company master

Current tenant onboarding is too light for enterprise agency operations. Missing or weak areas:

- legal identity fields: legal name, trade name, company code, CIN, GST, PAN, PF number, ESIC number, labour license number
- primary contacts: owner, compliance officer, billing contact, escalation contact
- commercial setup: billing email, support email, time zone, currency, invoicing prefix, payroll prefix
- subscription and entitlement controls: plan, seat limits, branch limits, storage limits
- approval workflow: onboarding review, KYC verification, activation status, onboarding checklist

Recommended new entities:

- `TenantOnboardingChecklist`
- `TenantComplianceRegistration`
- `TenantBranch`
- `TenantSettings`

### 2. Client onboarding and account management

Current `Client` is usable but not enterprise complete. Missing areas:

- billing address separate from operational address
- account manager, billing contact, escalation matrix
- credit period, billing cycle, GST state, payment mode preference
- SLA terms, escalation TAT, penalties, contract renewal workflow
- client-specific invoice rules and tax treatment

Recommended new entities:

- `ClientContact`
- `ClientBillingProfile`
- `ClientSla`
- `ClientEscalationMatrix`

### 3. Site and deployment structure

Current `Site` and `ContractSite` are present, but deployment still lacks post-level planning.

Missing areas:

- multiple posts/positions inside one site
- sanctioned strength by post, shift, gender, skill, weapon requirement
- reliever requirement, weekly off pattern, reserve pool mapping
- geofence exceptions, access zones, muster point, emergency contact set
- site instruction book and post orders

Recommended new entities:

- `SitePost`
- `SiteDeploymentPlan`
- `SiteInstruction`
- `SiteAccessPoint`
- `DeploymentRequirement`

### 4. HR and guard lifecycle

Current `SecurityGuard` captures basic personal details, but enterprise workforce lifecycle needs more.

Missing areas:

- employment status, exit date, rehire flag, resignation/termination reason
- blood group, nationality, marital status, education, language skills
- UAN/ESIC/PF nominee details
- bank verification status and KYC verification status
- uniform sizes, shoe size, badge number, biometric identifier
- background verification workflow and medical fitness

Recommended new entities:

- `GuardEmploymentHistory`
- `GuardKycVerification`
- `GuardNominee`
- `GuardMedicalRecord`
- `GuardBackgroundCheck`

### 5. Attendance, payroll, and wage calculations

Current system has the right modules, but payroll logic is still basic.

Missing or weak areas:

- salary structure components: basic, HRA, washing, conveyance, bonus, arrears
- deduction components: PF, ESIC, PT, TDS, advance recovery, penalties
- paid days logic: weekly off, holiday, leave without pay, reliever shifts
- overtime policy by site/shift/client rule
- payroll lock, approval, posting, payout batch, bank advice export
- monthly compliance register generation

Recommended new entities:

- `SalaryStructure`
- `SalaryComponent`
- `PayrollRun`
- `PayrollAdjustment`
- `PayrollRecovery`
- `GuardLoanAdvance`

Note:
The current code already has `SiteRatePlan` with `AllowancePercent`, `EpfPercent`, `EsicPercent`, and `EpfWageCap`. This should be treated as an interim payroll rule source until a dedicated salary structure module is added.

### 6. Billing and receivables

Current billing is functional but still invoice-light.

Missing areas:

- invoice tax breakup by CGST/SGST/IGST
- service charge, admin charge, reliever charge, statutory reimbursement
- debit note / credit note handling
- recurring invoice templates and invoice approval
- receivable aging, follow-up reminders, write-off handling
- client-specific billing formulas by post, shift, or attendance source

Recommended new entities:

- `InvoiceTemplate`
- `InvoiceTaxLine`
- `CreditNote`
- `DebitNote`
- `ReceivableFollowUp`

### 7. Compliance and audits

Current compliance dashboard only evaluates training and expiring documents. Enterprise operations need more.

Missing areas:

- labour license tracking by state and branch
- client contract compliance obligations
- PF/ESIC challan and return tracking
- muster roll, wage register, leave with wages register
- incident CAPA workflow
- statutory filing reminders

Recommended new entities:

- `ComplianceObligation`
- `ComplianceFiling`
- `ComplianceEvidence`
- `CorrectiveAction`

### 8. Multi-branch / multi-company operating model

Current tenant isolation is good, but enterprise agencies often run multiple branches under one company.

Missing areas:

- branch master
- branch-level users and approvals
- branch-wise numbering sequences
- branch P&L and branch compliance ownership
- inter-branch staff movement

Recommended new entities:

- `Branch`
- `BranchUserScope`
- `BranchSequence`
- `InterBranchTransfer`

## Suggested implementation order

### Phase 1: Foundation

- enrich tenant onboarding and company profile
- add branch master and tenant settings
- add client billing profile and client contacts
- add site posts and deployment planning

### Phase 2: Operational control

- strengthen attendance policy and approval rules
- introduce payroll run and salary structure
- enhance invoice generation with tax/service components
- add compliance obligations and reminders

### Phase 3: Enterprise scale

- branch-wise approvals and dashboards
- receivable aging and collection workflow
- CAPA and audit trail reporting
- entitlement, subscription, and storage controls

## Enterprise foundation applied now

The codebase now includes the first enterprise foundation slice from this analysis:

- tenant/company profile is expanded with legal identity, compliance, commercial setup, entitlement, and onboarding fields
- client master is expanded with billing, contact, credit-cycle, SLA, tax-treatment, and escalation fields
- site master is expanded with branch linkage, emergency operations metadata, and deployment support fields
- new enterprise entities are added for `Branch`, `SitePost`, `SiteDeploymentPlan`, `SalaryStructure`, and `PayrollRun`
- monthly document generation now also creates a `PayrollRun` summary record tied to the generated wage sheet
- branch master CRUD API is available through `api/v1/Branches`

## Current implementation notes

This change intentionally avoids auto-generating a new EF migration because the repository already contains pending migration/model-snapshot work. The domain, application, API, web model, and UI layers now understand the enterprise fields, but the database migration should be generated only after the current migration queue is reviewed.

The previously applied wage-calculation safety improvements remain in place:

- wage creation and update support day-based or hour-based rate application
- wage totals do not double-count allowances
- monthly document generation uses configured allowance, EPF, ESIC, and EPF wage-cap rules from `SiteRatePlan`

## Next recommended build step

After reviewing and consolidating the pending migration state, the next schema/build step should be:

1. generate a clean EF migration for the enterprise entities and new profile fields
2. add UI/API management screens for branch master and site-post/deployment editing
3. connect `SalaryStructure` into wage creation/update flows
4. extend enterprise coverage into compliance, receivables, and branch-wise approval workflows
