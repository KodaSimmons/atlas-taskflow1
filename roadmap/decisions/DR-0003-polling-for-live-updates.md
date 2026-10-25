# DR-0003 Use polling instead of SignalR for live updates

Date: 2026-10-25 · Status: accepted · Plan: PLAN-001

**Context:** FR-4.3 asks for updates without a manual refresh. The design named SignalR, with polling as the fallback
if WebSockets proved unreliable.

**Decision:** the bell count and the dashboard's My Tasks panel refresh every 15 seconds, and pause in background tabs.

**Why:** with in-memory data on one server, polling meets the requirement with much less code and nothing extra to
configure on Azure. Revisit once PLAN-002 adds a database.
