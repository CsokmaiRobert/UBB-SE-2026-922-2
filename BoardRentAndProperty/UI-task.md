# MVC UI Preparation Task

## Current Project State

The ASP.NET Core MVC project is `GUI-BRAP`.

The MVC project already uses the default frontend stack that comes with ASP.NET Core MVC:

- Bootstrap is loaded from `wwwroot/lib/bootstrap`.
- jQuery is loaded from `wwwroot/lib/jquery`.
- jQuery validation is available through the default validation partial.
- The shared layout is `Views/Shared/_Layout.cshtml`.
- The shared project CSS file is `wwwroot/css/site.css`.
- The shared project JavaScript file is `wwwroot/js/site.js`.

The current UI is still mostly the default MVC scaffold appearance:

- the navbar is basic;
- the project name still appears as `GUI_BRAP`;
- only a small number of navigation links exist;
- list pages mostly use plain Bootstrap tables;
- forms mostly use default scaffolded Bootstrap inputs;
- action links are not consistently styled as buttons;
- status values are displayed as plain text;
- `site.css` still contains mostly template CSS;
- `site.js` is still basically empty;
- there is no common delete confirmation behavior;
- there is no shared success/error alert convention beyond a few local examples.

The project should continue using Bootstrap and jQuery only. No extra frontend framework or JavaScript/CSS library should be introduced unless approval is explicitly given by the other team and the client.

## Reason For This Task

The assignment says:

```text
By default, ASP.NET core MVC comes with jQuery and Bootstrap.
If you want to use any extra javascript / css framework(s) you need to get the express approval
of the other team in the project, and the client.
```

This means the MVC application should rely on the built-in Bootstrap and jQuery setup unless there is a strong approved reason to add something else.

The purpose of this task is to prepare a consistent shared UI foundation before individual feature owners finish their own pages.

The goal is not to fully design every feature page. The goal is to make the application feel consistent enough that feature owners can adapt their own pages using the same layout, navigation, Bootstrap classes, alert style, button style, and common JavaScript helpers.

## Required End State

After this task, the MVC project should have a clearer shared UI structure:

- a project-appropriate shared layout;
- a consistent navbar;
- main navigation links for important application areas that already exist or are safe to expose;
- a consistent authenticated user area in the navbar;
- an admin-only navbar area or placeholder structure controlled by the existing role/auth helpers;
- a common page container style;
- consistent Bootstrap conventions for tables, forms, details pages, delete pages, buttons, badges, and alerts;
- shared CSS used only for project-wide styling;
- shared jQuery used only for common behaviors;
- no extra frontend frameworks.

The UI should look appropriate for a board game rental/property management application: simple, clean, functional, and easy to navigate. It should not look like a marketing landing page, and it should not become visually overcomplicated.

## Shared Layout

The shared layout in `Views/Shared/_Layout.cshtml` should be reviewed and improved.

The layout should keep the normal MVC structure:

```text
head
navbar/header
main content container
footer
scripts
```

Bootstrap and jQuery should continue to be loaded from the existing local `wwwroot/lib` folders.

The project name in the layout should be changed from the default `GUI_BRAP` style to a project-appropriate name, for example:

```text
BoardRent & Property
```

The layout should remain simple and usable. It should not introduce large hero sections, decorative pages, or unnecessary visual effects.

The main content area should have a consistent container and spacing so feature pages do not all invent different outer layouts.

Example direction:

```html
<main role="main" class="container py-4">
    @RenderBody()
</main>
```

The exact classes can be adjusted, but the project should have one common page wrapper.

## Navbar

The navbar should become the common navigation point for the MVC application.

It should include important application areas that are already present in the MVC project or safe as current routes:

- Games;
- Requests;
- Rentals;
- Notifications;
- Accounts/Admin area if appropriate for the current project state.

Navigation links should not point to pages that are known to be broken or not yet implemented. The navbar should stay useful while features are still being migrated.

The navbar should include a logged-in user area.

When the user is authenticated, the navbar should show:

- the current display name or username;
- a logout action.

When the user is not authenticated, the navbar should show:

- login.

The navbar should be responsive using Bootstrap's existing navbar behavior.

Admin-only navigation should use the shared role/auth helper pattern once available.

Expected idea:

```csharp
if (User.IsAdministrator())
{
    // show admin navigation
}
```

The exact admin links can stay minimal, but the structure should be clear and not duplicated across pages.

## Bootstrap Table Convention

List pages should use a consistent Bootstrap table style.

Recommended baseline:

```html
<div class="table-responsive">
    <table class="table table-striped table-hover align-middle">
        ...
    </table>
</div>
```

Tables should have:

- clear headers;
- readable spacing;
- action buttons aligned consistently;
- responsive wrapping for smaller screens;
- no excessive custom styling.

Feature owners should later apply this convention to their own list pages.

## Bootstrap Form Convention

Create and edit pages should use a consistent form style.

Recommended baseline:

```html
<div class="row">
    <div class="col-md-6 col-lg-5">
        <form method="post">
            <div asp-validation-summary="All" class="text-danger"></div>
            ...
        </form>
    </div>
</div>
```

Forms should consistently use:

- `form-control` for text inputs;
- `form-select` for selects where appropriate;
- `form-check` for checkboxes;
- `mb-3` spacing between fields;
- validation messages under fields;
- one clear submit button;
- one clear back/cancel link or button.

The existing scaffolded forms often use default `form-group` spacing. The shared style should make the intended Bootstrap spacing clear for future pages.

## Details Page Convention

Details pages should have a consistent display style.

Recommended baseline:

```html
<dl class="row">
    <dt class="col-sm-3">Name</dt>
    <dd class="col-sm-9">Value</dd>
</dl>
```

Details pages should include:

- a clear title;
- readable labels and values;
- action buttons or links at the bottom;
- a back button or link.

## Delete And Confirmation Convention

Delete or destructive confirmation pages should have a clear warning style.

Recommended baseline:

```html
<div class="alert alert-danger">
    Are you sure you want to delete this item?
</div>
```

Delete buttons should consistently use:

```text
btn btn-danger
```

Back/cancel actions should consistently use:

```text
btn btn-secondary
```

The project should also have a common jQuery delete confirmation helper in `wwwroot/js/site.js`.

The helper can be based on a shared data attribute, for example:

```html
<button type="submit" class="btn btn-danger" data-confirm="Are you sure?">
    Delete
</button>
```

The JavaScript behavior should ask for confirmation before the destructive action continues.

This should be a small shared helper, not a new plugin or library.

## Button Convention

Action buttons should use consistent Bootstrap classes.

Recommended convention:

```text
Create  -> btn btn-primary
Edit    -> btn btn-warning
Details -> btn btn-info
Delete  -> btn btn-danger
Back    -> btn btn-secondary
Save    -> btn btn-primary
Cancel  -> btn btn-secondary
```

Plain text action links like:

```text
Edit | Details | Delete
```

should be avoided in final pages. Feature owners should use this button convention when they finish their own views.

## Status Badge Convention

Status values should have a consistent badge style.

Recommended convention:

```text
Open/Pending      -> badge bg-warning text-dark
Approved/Active   -> badge bg-success
Denied/Rejected   -> badge bg-danger
Cancelled/Closed  -> badge bg-secondary
Informational     -> badge bg-info text-dark
```

The exact statuses depend on each feature, but the visual convention should be documented and available for feature owners.

Status badges should use Bootstrap badge classes. No extra badge library should be added.

## Alert Convention

Success, error, warning, and information messages should use Bootstrap alerts.

Recommended convention:

```text
Success -> alert alert-success
Error   -> alert alert-danger
Warning -> alert alert-warning
Info    -> alert alert-info
```

Pages that display `TempData` or API error messages should use this convention.

Feature owners should later use the same alert style when showing validation errors, failed API calls, successful actions, or blocked operations.

## Shared CSS

`wwwroot/css/site.css` should contain only common project-wide styling.

It should not contain page-specific rules for one feature unless that style is truly shared.

Useful shared styling can include:

- page spacing;
- footer behavior;
- navbar polish;
- table action spacing;
- consistent card or section spacing if used;
- small helper classes for page headers or action rows.

Avoid large custom design systems. Bootstrap should remain the main styling framework.

Avoid overriding Bootstrap heavily. The UI should stay maintainable for the team.

## Shared jQuery

`wwwroot/js/site.js` should contain only common project-wide behavior.

Useful shared behavior can include:

- delete confirmation through `data-confirm`;
- dismissing alerts if needed;
- small helpers that work across multiple pages.

Do not add unrelated scripts or page-specific feature logic to `site.js`.

Do not add extra JavaScript frameworks.

Do not add code comments in the UI files. Comments are forbidden for this task.

## Frontend Framework Rule

The MVC project should keep using:

```text
Bootstrap
jQuery
jQuery Validation
```

Do not add:

```text
React
Vue
Angular
Tailwind
DataTables
Toastr
SweetAlert
Bootstrap icon packages
extra CSS templates
extra JavaScript UI plugins
```

unless approval is explicitly given according to the assignment requirement.

If a feature owner needs something visual, they should first try to solve it with Bootstrap and small local CSS.

## What Should Not Be Changed In This Preparation

Do not migrate MVC controllers to proxy services as part of this UI task.

Do not change API endpoints.

Do not change service or repository behavior.

Do not implement business authorization rules inside feature controllers.

Do not fully restyle every scaffolded feature page.

Do not create new pages just to satisfy navbar links.

Do not introduce broken links in the navbar.

Do not add extra frontend frameworks or libraries.

Do not replace Bootstrap with a different design system.

Do not put feature-specific JavaScript into the shared `site.js`.

Do not put feature-specific CSS into the shared `site.css` unless it is clearly reusable.

Do not add HTML, CSS, JavaScript, or Razor comments.

## Completion Criteria

This UI preparation is complete when:

- Bootstrap and jQuery are still loaded correctly from local MVC assets;
- `_Layout.cshtml` has project-appropriate branding;
- the navbar is consistent, responsive, and useful;
- authenticated and unauthenticated navbar states are clear;
- admin navigation structure is prepared without exposing it to normal users;
- the main page container layout is consistent;
- common Bootstrap conventions are documented for tables, forms, details pages, delete pages, buttons, badges, and alerts;
- `site.css` contains only shared project styles;
- `site.js` contains a small shared jQuery delete-confirmation helper;
- no comments were added to the edited UI files;
- no extra frontend framework or UI library has been added;
- the MVC project still builds.
