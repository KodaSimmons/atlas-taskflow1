# Testing evidence

Testing happened at three levels: automated unit and integration tests on every build, a scripted browser walkthrough
locally, and a smoke test against the live Azure site after each deployment.

## 1. Automated tests (`dotnet test`)

44 tests, all passing locally on 2026-10-25. They also run in GitHub Actions on every push and pull request; the results
are uploaded as the `test-results` artifact.

**Unit tests** (`TaskWorkflowServiceTests`, 20 tests) check each rule in isolation:

| Area | What's checked |
| --- | --- |
| Projects | New teams get a default "Tasks" project |
| Creating tasks | No assignees means Not Assigned; several assignees all get notified; members can't create; people outside the team are ignored |
| Completion gate | Members get "Only Team Leaders can mark tasks as Completed."; leaders and delegated managers can approve; the completion time is recorded |
| Permissions | Members can't update tasks that aren't theirs; tasks from another team can't be changed or deleted |
| Pool | Picking up assigns the task to you; a task someone already has can't be picked up |
| Submitting work | Files are saved and the task moves to Needs Review with the leader notified; no files, or a file over 5 MB, is rejected |
| Review | Request changes sends the task back to In Progress with the feedback as a comment; empty feedback is rejected |
| Editing | Removing every assignee returns the task to Not Assigned |
| Discussion | Empty comments are rejected |

**Integration tests** (`AppIntegrationTests`, 24 tests) start the real app in memory and use it over HTTP like a browser:

| Assignment test type | Test | Result |
| --- | --- | --- |
| Core navigation | Dashboard, every task tab, new task, projects, new project, team and notifications pages load (10 cases) | Pass |
| Core navigation | The dashboard shows the review queue, my tasks, projects and recent activity | Pass |
| Primary workflow | Leader creates a task and it is saved as Assigned | Pass |
| Primary workflow | Member submits files and the task moves to Needs Review with the file attached | Pass |
| Primary workflow | Leader approves a task in review | Pass |
| Primary workflow | Member trying to complete a task sees the leader-only message and the status doesn't change | Pass |
| Validation | Missing title shows "Give the task a title." and nothing is saved | Pass |
| Validation | A due date in the past is rejected | Pass |
| Unexpected input | A due date that isn't a date is rejected | Pass |
| Unexpected input | An unknown status value is rejected (this test found a real bug, see the dev log) | Pass |
| Unexpected input | A form posted without its anti-forgery token is refused (400) | Pass |
| Error handling | A task that doesn't exist shows the friendly 404 page | Pass |
| Data isolation | After switching teams, the other team's tasks are hidden and can't be opened by URL | Pass |
| Live updates | The notification count endpoint returns the unread count | Pass |
| Deployment | `/health` returns Healthy | Pass |

## 2. Local browser walkthrough

A Playwright script drove Chromium through the main journey on a local Release build running in Production mode.
Every step completed. Screenshots are in [`screenshots/`](screenshots/):

| # | Step | Screenshot |
| --- | --- | --- |
| 1 | Leader dashboard loads | `01-dashboard-leader.png` |
| 2 | Projects list | `02-projects.png` |
| 3 | Project status board with all five columns | `03-board.png` |
| 4 | Task waiting for review, with approve / request changes | `04-review-task.png` |
| 5 | Empty title shows a validation message | `05-validation.png` |
| 6 | Task created and assigned to Marcus | `06-created.png` |
| 7 | Switched to Marcus: new task on his dashboard, bell shows a notification | `07-dashboard-member.png` |
| 8 | Marcus starts work, uploads a file and submits: Needs Review | `08-submitted.png` |
| 9 | Marcus tries to mark it Completed and is blocked | `09-blocked.png` |
| 10 | Marcus switches to his other team | `10-team-switch.png` |
| 11 | Sarah sees the review notification | `11-notifications.png` |
| 12 | Sarah approves: Completed | `12-approved.png` |
| 13 | Unknown task shows the 404 page | `13-not-found.png` |
| 14 | Dashboard on a phone-sized screen (390 px) | `14-mobile-dashboard.png` |

The page had no JavaScript errors; the only console message was the expected 404 from step 13.

## 3. Post-deployment smoke test

The deploy job checks the live site automatically after every deployment:

| Check | Expected |
| --- | --- |
| `/health` | 200, "Healthy" |
| `/` (dashboard) | 200, page contains "Atlas TaskFlow" |
| `/Tasks` | 200 |
| `/Projects` | 200, shows "Spring Campaign Launch" |
| `/Team` | 200, shows "Brightside Marketing" |
| `/Tasks/Details/999999` | 404 |

**First deployment, 2026-10-25** ([workflow run](https://github.com/kodasimmons59-prog/atlas-taskflow/actions/runs/40232502012), commit `d1ba462`), against <https://atlas-taskflow-koda-d8gjabatgvatgtcj.northcentralus-01.azurewebsites.net>:

| Check | Result |
| --- | --- |
| `/health` | 200 Healthy |
| `/` | 200 |
| `/Tasks` | 200 |
| `/Projects` | 200 |
| `/Team` | 200 |
| `/Tasks/Details/999999` | 404 as expected |

**Manual checks on the live site** (fill these in yourself in a browser):

| Check | Result | Date |
| --- | --- | --- |
| Live URL loads over HTTPS |Pass |10/25/26 |
| Create a task as Sarah and assign it to Marcus |Pass |10/25/26 |
| As Marcus: start, upload a file, submit for review |Pass |10/25/26 |
| As Marcus: try to mark Completed and see the block message |Pass |10/25/26 |
| As Sarah: open the notification and approve |Pass |10/25/26 |
| Switch Marcus to Corner Bakery Co. and confirm only its tasks show |Pass |10/25/26 |
| Open the site on a phone |Fail |10/25/26 |
