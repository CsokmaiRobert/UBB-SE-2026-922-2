# Task Markdown Writing Rules

## Purpose

This local file describes how task `.md` files should be written for this project.

It is meant to preserve the same writing convention used in the current project task files, such as:

```text
architecture-task.md
UI-task.md
8-roles-tasks.md
auth-feature-task.md
mvc-option-3-migration.md
```

The goal is to produce task documents that are useful for teammates and also clear enough to be given to an implementation assistant later without creating confusion.

This file covers two things:

```text
how task markdown files should be written;
which common project rules should appear in every implementation task by default.
```

When a later prompt gives only the specific part of a task, the generated task document should still include the relevant common project rules from this file.

## Writing Approach

A task `.md` should be written as a project requirement and research brief, not as a chat message.

It should explain:

```text
what the project state is now;
why the task exists;
what the task is trying to achieve;
what must be changed;
what must not be changed;
which files/features are connected;
what the expected final workflow should look like.
```

The writing should be direct, practical, and specific to the current codebase.

Avoid writing it like a system prompt. Do not use phrases such as:

```text
You are an AI.
Your task is to...
Act as...
Follow these steps exactly...
```

Prefer neutral requirement language:

```text
The feature should...
The MVC controller should...
The final workflow should...
The current scaffold uses...
This file should not...
```

## Codebase First

Before writing or updating a task `.md`, inspect the current codebase.

The document must describe the actual current state, not an older remembered state.

Use exact current project paths, controller names, service names, DTO names, and endpoint names.

If the project changed since an older task document was written, update the document to match the current code.

Do not keep stale paths such as old folders, old namespaces, or old class names just because they appeared in previous notes.

For feature tasks, inspect the matching WinUI desktop implementation before writing the MVC task. The WinUI project is the behavior reference for what the web feature should do.

The MVC feature should reproduce the same user-facing workflow, business meaning, validation expectations, and authorization meaning as the WinUI feature, adapted to ASP.NET Core MVC, Bootstrap, jQuery, and the prepared option 3 architecture.

Do not copy XAML or WinUI-specific code into MVC. Use the WinUI pages, view models, and desktop services to understand the intended logic and experience, then describe how that same feature should exist in the web project.

For this project, current MVC setup paths include:

```text
GUI-BRAP/ProxyServices
GUI-BRAP/Infrastructure
GUI-BRAP/Authorization
GUI-BRAP/Views/Shared/_Layout.cshtml
GUI-BRAP/wwwroot/css/site.css
GUI-BRAP/wwwroot/js/site.js
```

## Tone

The document should sound like a serious student project requirement, not like marketing text and not like generated filler.

Use clear paragraphs and short lists.

Avoid exaggerated language.

Avoid vague phrases such as:

```text
make it better;
fix everything;
handle all cases;
improve the architecture;
do the UI stuff.
```

Replace them with concrete wording:

```text
replace direct AppDbContext access with a proxy service;
register the proxy service in AddProxyServices();
load the current account id from User.GetAccountId();
call GET /api/notifications/user/{accountId};
display API errors through ModelState or an alert.
```

## Level Of Detail

The task file should contain enough detail that the implementer understands the work without asking basic clarification questions.

It should not become a huge step-by-step coding script unless the user explicitly asks for implementation steps.

The normal balance is:

```text
explain the current state clearly;
explain the desired final state clearly;
name the important files;
name the important endpoints;
name the expected architecture flow;
give small examples where they prevent confusion;
avoid prescribing every line of code.
```

Short code or route examples are allowed when they clarify the requirement.

Large code blocks should be avoided unless the user specifically asks for a detailed implementation plan.

## Architecture Consistency

For this project, task documents must preserve the selected option 3 architecture:

```text
MVC Controller
-> Proxy Service
-> API Controller
-> API Service
-> Repository
-> Database
```

When a feature currently uses scaffolded MVC CRUD, say that explicitly.

When a feature still uses `AppDbContext` directly in MVC, say that it is temporary and belongs to the feature migration work.

Do not describe direct MVC database access as acceptable final architecture.

Do not tell feature owners to copy business logic into MVC.

Do not tell feature owners to use repositories from MVC.

## Auth, Authorization, DI, And UI Consistency

Task documents should respect the setup that already exists:

```text
cookie authentication in GUI-BRAP/Program.cs;
fallback authorization policy in GUI-BRAP/Program.cs;
role helpers in GUI-BRAP/Authorization;
proxy registration in GUI-BRAP/Infrastructure/ServiceCollectionExtensions.cs;
API errors handled through GUI-BRAP/Infrastructure;
Bootstrap and jQuery UI conventions in the shared layout, site.css, and site.js.
```

Do not assign every feature owner to redesign these shared foundations.

Feature owners should follow these foundations unless the task is specifically about changing the foundation.

If a feature needs admin-only behavior, the task should say that controller-level authorization is required, not only navbar hiding.

If a feature needs user-specific data, the task should mention the current-user helpers.

## Default Rules For Every Implementation Task

Every feature implementation task should include a concise common-rules section unless the user explicitly says not to include it.

The common-rules section should not dump the full assignment text. It should translate the assignment into practical project rules.

The default implementation rules are:

```text
No code comments should be added.
Code should follow the StyleCop and existing project style conventions.
Variable names should be descriptive; one-character variable names should not be introduced.
Edits should stay scoped to the assigned feature and necessary shared registration files.
Unrelated refactors should not be included.
The matching WinUI feature should be used as the behavior and workflow reference.
The MVC feature should follow the prepared option 3 architecture.
MVC controllers should use proxy services through dependency injection.
Proxy services should call API endpoints through the named HttpClient.
API controllers should continue to call service-layer interfaces.
Business logic should stay in the API service layer, not in MVC controllers or views.
Some MVC scaffold controllers still use AppDbContext directly at the start of feature work.
The task should identify the assigned scaffolded MVC controller or view set and require only that assigned feature to be converted to proxy services.
Repositories and AppDbContext should not be used from final MVC feature controllers.
Final MVC views should use DTOs from BoardRentAndProperty.Contracts or MVC-specific view models.
Final MVC views should not use BoardRentAndProperty.Api.Models classes.
The existing authentication, authorization, DI, and UI setup should be reused.
Bootstrap and jQuery should remain the only frontend libraries.
The feature should be guarded from unauthorized users.
Unauthenticated users should not be able to access feature links or actions and should be redirected to the login page.
Admin-only behavior should be enforced in controllers, not only hidden in the navbar.
The final code should build after the feature is implemented.
```

The wording can be adapted to the task, but the meaning should stay intact.

For example, instead of pasting the full assignment requirement, write:

```text
The feature should preserve the prepared MVC migration architecture. MVC should call a feature proxy service, the proxy should call the API, and the API should continue using its service and repository layers. Dependencies should be registered through the existing DI setup instead of being manually instantiated.
```

## Required Common Context In Feature Tasks

Each feature task should remind the implementer that the project setup already exists and should be followed.

Mention the setup only as much as needed. Do not make every task document repeat the entire architecture history.

Useful default wording:

```text
The MVC project already has authentication, fallback authorization, proxy infrastructure, API error handling, and shared Bootstrap/jQuery UI conventions. This feature should reuse those foundations instead of creating parallel patterns.
```

Each feature task should point to `8-roles-tasks.md` when the feature's place in the wider application might be unclear.

Useful default wording:

```text
If the feature boundaries or links with other features are unclear, check `BoardRentAndProperty/8-roles-tasks.md` before changing shared files.
```

## Task-Specific Parts Are Not Boilerplate

Some sections are important, but they should not be treated as fixed common text for every task.

When creating a specific task `.md`, infer these parts from the particular feature, the current codebase, and the user's description:

```text
what exactly counts as done;
which files should be touched;
which files should not be touched;
which feature owners may have overlap;
which routes and views are expected;
which verification checks are meaningful.
```

These parts should be written automatically by the task writer for the specific task. They should not be delegated to the later implementation agent as something vague to decide.

For example, a Notifications task may need rules about current-user notifications and request-generated notifications, while an Admin task may need rules about administrator-only actions and account-management views. These are not the same common section.

## Required Access Description In Feature Tasks

Every feature task should include an access and permissions section.

The section name can vary, but it should clearly describe normal-user and admin behavior for the specific feature.

The exact content is task-specific and should be inferred from the feature, but the section should cover these questions:

```text
Which feature links/pages are accessible to a logged-in normal user?
Which feature links/pages are accessible only to a logged-in administrator?
Which links/pages should not be accessible to unauthenticated users?
What should a normal user be able to see?
What should a normal user be able to do?
What should an administrator be able to see?
What should an administrator be able to do?
```

The task should make clear that unauthenticated users are redirected to the login page.

If an action is admin-only, the task should say that the MVC controller/action must enforce this, not only the navbar.

If an action is user-owned, the task should say that the user should only see or modify their own data unless administrator behavior is explicitly allowed.

## Code Quality Expectations

When a task document will be used for implementation, include code quality expectations in plain language.

The expected style for this project is:

```text
Follow the existing C# style and StyleCop conventions.
Use explicit and meaningful names.
Avoid one-character local variables and unclear abbreviations.
Avoid comments in added code.
Prefer small focused methods when logic would otherwise become hard to read.
Keep controllers thin.
Keep API-call code inside proxy services.
Keep business rules inside API services.
```

Do not turn these expectations into a lecture. They should be short enough that they help without distracting from the feature.

## Parallel Work Awareness

Task documents should make conflict areas visible when the specific feature has real overlap with other work.

This is a task-writer responsibility, not a fixed common section that should be pasted into every task.

When two features may touch the same files, include a short links or coordination section.

The tone should be useful, not annoying. Avoid repeating phrases such as:

```text
this is part of another task;
someone else will do this;
do not touch anything outside your task.
```

Prefer:

```text
This overlaps with Requests because both use the same proxy service.
Coordinate before changing shared request routes or views.
Shared files likely touched by this feature:
```

This helps the team work in parallel without hiding real merge-risk areas.

## What Not To Do

Do not write task documents based only on assumptions.

Do not include stale code paths.

Do not overpromise that a feature is already finished if only the setup exists.

Do not hide known limitations.

Do not mix old architecture and new architecture as if both are final.

Do not add unrelated refactor goals.

Do not use extra frontend framework suggestions unless the requirement explicitly allows them.

Do not include comments in UI code examples if the team rule says comments are forbidden.

Do not create broken links or placeholders unless the user explicitly wants placeholders.

## Preferred Markdown Shape

Use normal Markdown headings.

Use short paragraphs between lists so the document is readable.

Use fenced `text` blocks for architecture flows, project paths, endpoint lists, and simple examples.

Use inline code for file names, class names, methods, routes, and roles.

Keep the document organized by meaning, not by random implementation order.

Good section names are:

```text
Current Project State
Reason For This Task
Required End State
Current Related Code
API Endpoints
Remaining Work
Expected Final MVC Shape
Links With Other Feature Owners
Final Checks
```

Only include the sections that fit the task.

## Updating Existing Task Documents

When updating an existing task `.md`, first check whether the project has moved forward.

If the setup is now implemented, the document should say it is implemented.

If a controller was migrated to proxy service, the document should not still describe it as direct database scaffold.

If a folder was deleted or replaced, update all references.

If the task is now feature-owner work instead of architecture setup, say that clearly.

The final document should be internally consistent, even if that means rewriting large parts instead of making tiny patches.
