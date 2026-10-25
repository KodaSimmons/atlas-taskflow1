# PLAN-001 Working cloud application

Status: `BLOCKED` · Sprint 4 · Source: Assignment 4 brief

## Goal

Turn the Sprint 3 design into a working ASP.NET Core app, deployed to Azure App Service, with Git history, testing
evidence and a reflection.

## Scope

In: dashboard, projects and status boards, task create/edit/delete, the five-status workflow with the leader-only
completion gate, Not Assigned pool, file submission, discussion, notifications, team switcher, manager permissions.
Out (see later plans): database, sign-in, email.

## Steps

| Step | State |
| --- | --- |
| Build the app with models, controllers and views | Done (`00e7324`..`c807c80`) |
| Unit and integration tests | Done, 44 passing (`998dd09`) |
| Deploy workflow, smoke test, docs and screenshots | Done (`1b4f71e`) |
| Push to `kodasimmons59-prog/atlas-taskflow` | Blocked: the Claude GitHub App is not installed on the repo |
| Create the Azure App Service and add the deploy secret | Waiting on Koda ([DEPLOYMENT.md](../../docs/DEPLOYMENT.md)) |
| First deploy and live checks | After the two steps above ([TESTING.md](../../docs/TESTING.md) §3) |
| Reflection | Koda writes it |

## Decisions

[DR-0001](../decisions/DR-0001-csharp-and-azure.md), [DR-0002](../decisions/DR-0002-in-memory-data.md),
[DR-0003](../decisions/DR-0003-polling-for-live-updates.md), [DR-0004](../decisions/DR-0004-demo-identity-switcher.md),
[DR-0005](../decisions/DR-0005-taskitem-name.md)

## Done when

The live Azure URL passes the smoke test, the live checks in TESTING.md are filled in, and Koda accepts the submission.
