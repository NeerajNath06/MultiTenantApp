# UI Architecture — Security Agency Management System

Premium SaaS-style UI (Stripe, Linear, Notion, Metronic, Vercel, Tabler inspired).

## Layout Structure

- **Main layout** — `Views/Shared/_Layout.cshtml`
  - Sidebar (fixed left, collapsible) + main content area that **fills all remaining width** (fluid, no max-width).
  - Topbar (sticky) + content wrapper with padding only; `.page-content` holds the view body.
- **Sidebar** — Rendered in `_Layout`; menu items from `SidebarMenu` View Component (`Views/Shared/Components/SidebarMenu/Default.cshtml`). Collapsible groups, active state (left border), compact mode (icons only) on desktop.
- **Topbar** — `Views/Shared/_Topbar.cshtml`: global search, Quick actions (New) dropdown, theme toggle, notifications, user profile dropdown.
- **Breadcrumbs** — `Views/Shared/_Breadcrumb.cshtml`. Set `ViewData["Breadcrumbs"]` as `(Label, Url?)[]`.
- **Page header** — `Views/Shared/_PageHeader.cshtml`. Set `ViewData["Title"]`, optional `ViewData["PageSubtitle"]`, `ViewData["PageActions"]` (HTML for action buttons).

## Design System

- **CSS** — `wwwroot/css/design-system.css` (tokens), `wwwroot/css/site.css` (components and overrides).
- **Themes** — Light (default) and dark via `data-theme="dark"` on `<html>`. Toggle in topbar; preference in `localStorage.appTheme`.
- **Tokens** — Surfaces (`--bg-page`, `--bg-elevated`, `--bg-muted`), text (`--text-primary`, `--text-secondary`, `--text-muted`), borders, shadows (`--shadow-sm` … `--shadow-xl`, `--shadow-glow`), radius (`--radius-sm` … `--radius-2xl`), spacing (`--space-1` … `--space-16`), typography scale, glassmorphism (`--glass-bg`, `--glass-border`), skeleton (`--skeleton-base`, `--skeleton-highlight`).

## Components (CSS classes)

- **Cards** — `.card`, `.card-glass` (glassmorphism), `.stat-card` (dashboard stats). Hover: elevation.
- **Skeletons** — `.skeleton`, `.skeleton-text`, `.skeleton-title`, `.skeleton-avatar`, `.skeleton-card`.
- **Modals** — Bootstrap `.modal-content` styled with design tokens; `.modal-header`, `.modal-body`, `.modal-footer`.
- **Toasts** — `.toast-container` (fixed top-right), `.toast` with design tokens.
- **Tooltips** — Bootstrap tooltips; `.tooltip-inner` overridden for radius and shadow.

## Responsive

- Sidebar: off-canvas on small screens; fixed + collapsible on `min-width: 992px`.
- Topbar: search and quick actions hidden on smaller breakpoints; `min-height` and flex-wrap for stacking.
- Content: `.content-wrapper` padding and max-width scale by breakpoint; `.page-content` spacing.

## Usage in a view

```csharp
ViewData["Title"] = "Clients";
ViewData["PageSubtitle"] = "Manage client information";
ViewData["Breadcrumbs"] = new[] { ("Clients", (string)null) };
ViewData["PageActions"] = "<a href=\"/Clients/Create\" class=\"btn btn-primary\">Add Client</a>"; // optional
```

Then in the view:

```html
@await Html.PartialAsync("_PageHeader")
<!-- or use existing page-header div with page-title, page-subtitle, and buttons -->
```
