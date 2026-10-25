# DR-0005 Name the task entity TaskItem

Date: 2026-10-25 · Status: accepted · Plan: PLAN-001

**Context:** the design's entity is `Task`, which clashes with .NET's `System.Threading.Tasks.Task` used by every async
action.

**Decision:** the entity is `TaskItem` and its status enum is `TaskItemStatus`. The database table can still be called
Task in PLAN-002.
