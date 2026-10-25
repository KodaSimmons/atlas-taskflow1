# Atlas TaskFlow

Atlas TaskFlow is a task manager for small business teams. Team leaders create and assign work and review it before it's
closed out; team members see what's on their plate, pick up open work, and submit deliverables. This repository is the
Sprint 4 build: the first working, cloud-hosted version of the Sprint 3 Product Design Package.

**Live app:** <https://atlas-taskflow-koda-d8gjabatgvatgtcj.northcentralus-01.azurewebsites.net>

## Try it

The app opens as **Sarah Chen**, the Team Leader persona. Use the name menu at the top right ("Viewing as") to switch to
**Marcus Vance**, a Team Member, and try the other side of the workflow. Marcus belongs to two businesses, so the team
switcher in the navigation bar lets him move between Brightside Marketing and Corner Bakery Co.

A quick walkthrough of the main workflow (Sprint 3, Journey 2):

1. As Sarah, click **+ New task**, fill in the form and assign it to Marcus.
2. Switch to Marcus. The bell shows a new notification and the task is on his dashboard.
3. Open the task, click **Start working**, attach a file and click **Submit files**. The task moves to **Needs Review**.
4. Try setting the status to **Completed** as Marcus. It's blocked: "Only Team Leaders can mark tasks as Completed."
5. Switch back to Sarah, open the notification, check the file and click **Approve and complete** (or **Request changes**).

Sample data lives in memory and resets whenever the app restarts.

## Run it locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet run --project src/AtlasTaskFlow      # then open the URL it prints
dotnet test                                 # unit and integration tests
```

## How it's built

ASP.NET Core 8 MVC, following the course pattern of models, controllers and Razor views with a shared layout.

```
src/AtlasTaskFlow/
  Models/        Domain classes from the Sprint 3 data design, plus view models for the pages
  Services/      AtlasStore (in-memory data), TaskWorkflowService (business rules),
                 CurrentUserContext (who is using the app), SeedData (sample teams and tasks)
  Controllers/   Home (dashboard), Tasks, Projects, Team, Notifications, Demo (user/team switcher)
  Views/         Razor pages and the shared layout
  wwwroot/       CSS, the polling script and Bootstrap 5.3
tests/AtlasTaskFlow.Tests/
  TaskWorkflowServiceTests.cs   Unit tests for every workflow rule
  AppIntegrationTests.cs        Runs the whole app in memory and drives it over HTTP
.github/workflows/azure-deploy.yml   Build, test, deploy to Azure App Service, then smoke test
```

The rules live in `TaskWorkflowService`, not in controllers or views. Controllers translate web requests into calls on
that service and pick a page to show. That kept the rules easy to unit test and means the same checks apply no matter
which page or button triggers them.

## How it maps to the Sprint 3 design

| Design item | Where it is |
| --- | --- |
| FR-2.1 Default "Tasks" project per team | `AtlasStore.CreateTeam`; new tasks default to it (US-05) |
| FR-2.2 Five statuses | `TaskItemStatus`; columns on each project board |
| FR-2.3 Zero, one or many assignees | Assign-to checkboxes on the task form |
| FR-3.1 / 3.2 Leaders manage tasks, members update their own | `TaskWorkflowService.CheckStatusChange` and the create/edit/delete checks |
| FR-3.3 / US-09 Only leaders can mark Completed | Same, with the exact message from the acceptance criteria |
| US-07 Not Assigned pool | Tasks > Not assigned, and **Pick up this task** |
| US-08 Submit Files moves the task to Needs Review and notifies the leader | **Submit files** on the task page |
| US-10 Discussion thread | Discussion section on each task |
| FR-4.1 / US-11 Dashboard with tasks, projects, calendar | Dashboard (leaders also get a review queue) |
| FR-4.2 Bell notifications on assignment and review | Bell in the top bar and the Notifications page |
| FR-4.3 Updates without a manual refresh | Bell and My Tasks panel refresh every 15 seconds |
| US-04 Switch teams without logging out | Team switcher in the top bar; data is filtered by the active team |
| FT-10 Leader delegates manager permissions | Team page, **Make manager** |

### What's deliberately not in this sprint

The assignment defers these to later sprints, so they are stubbed or left out:

- **Sign-up, login, email verification, password reset and invitations** (FT-01, FT-02 invites): Sprint 6. A "Viewing as"
  switcher stands in for login so both roles can be demonstrated. Only `CurrentUserContext` needs to change when real
  authentication is added.
- **Azure SQL and Blob Storage**: data and uploaded files are held in memory. `AtlasStore` mirrors the planned tables so
  it can be replaced with Entity Framework Core.
- **SignalR**: polling is used instead (see below).

### Where the implementation differs from the design

- **Polling instead of SignalR for live updates.** The design listed polling as the fallback if WebSockets were
  unreliable. With in-memory data on one server, polling every 15 seconds meets FR-4.3 with far fewer moving parts.
  SignalR can be added once there is a real database.
- **`TaskItem` instead of `Task`.** `Task` collides with .NET's `System.Threading.Tasks.Task`, which every async
  controller uses, so the entity was renamed.
- **Recent activity feed.** The dashboard wireframe showed recent activity but the data design had no table for it, so an
  `ActivityEntry` record was added.
- **Request changes is its own button.** The design described the leader commenting and then moving the task back. The
  app does both in one step and requires feedback, so a task is never sent back without an explanation.

## Docs

- [Roadmap](roadmap/ROADMAP.md): what's planned, what state it's in, decisions and Koda's rulings

- [Deployment guide](docs/DEPLOYMENT.md): setting up Azure App Service and the GitHub Actions deploy
- [Testing evidence](docs/TESTING.md): what was tested before and after deployment, with results
- [Development log](docs/DEV-LOG.md): problems hit during the build and how they were solved
