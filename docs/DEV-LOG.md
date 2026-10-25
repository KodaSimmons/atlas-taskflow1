# Development log

Significant problems found while building Sprint 4, how they were investigated, and what fixed them.

## 1. Changing a task to an unknown status silently moved it to Not Assigned

**Found by:** the integration test `InvalidStatusValue_IsHandled`, which posts `status=42` to the change-status action.

**What happened:** the test expected the error "That isn't a valid status." Instead, the task's status became Not
Assigned and the page said "Moved to Not Assigned."

**Investigation:** the controller already checked `Enum.IsDefined(status)`, so the bad value should have been caught.
Printing the bound value showed it wasn't 42 at all; it was `NotAssigned`. ASP.NET Core's model binder refuses enum
values that aren't defined, records an error in `ModelState`, and leaves the parameter at its default. The default for
`TaskItemStatus` is its first value, `NotAssigned`, which *is* a defined value, so the check passed. For a leader, moving
to Not Assigned is allowed, so the task was unassigned.

**Fix:** the action now checks `ModelState.IsValid` as well as the value. Lesson: a failed bind doesn't throw, it
quietly hands you a default, so always check `ModelState` before trusting bound parameters.

## 2. The mobile menu button was invisible

**Found by:** viewing the dashboard at phone width in the browser walkthrough. The button took up space (Playwright
reported its position) but nothing was drawn.

**Investigation:** the project template ships Bootstrap 5.1. In 5.1 the menu icon only gets its image when the navbar
has `navbar-light` or `navbar-dark`. The views also used classes such as `text-body-secondary` and `text-bg-light` and
the button colour variables, which only exist from Bootstrap 5.3, so some styling was quietly missing too.

**Fix:** upgraded the bundled Bootstrap to 5.3.3. The icon, muted text and button colours all appeared.

## 3. `Task` clashes with .NET's `Task`

The design's entity is called Task, but every async controller action returns `System.Threading.Tasks.Task`. Using the
same name would mean writing the full namespace everywhere. The entity became `TaskItem` (and the status enum
`TaskItemStatus`).

## 4. Integration tests looked up the wrong team

The first run of the integration tests failed with "Sequence contains no matching element." The tests assumed the first
team had ID 1, but the in-memory store uses one ID counter for everything, so the four sample users take IDs 1 to 4 and
the first team is 5. The tests now look teams and people up by name instead of hard-coding IDs.

## 5. Cookies in integration tests

The demo user and team cookies are marked `Secure`. The test client's default address is `http://localhost`, where
secure cookies aren't sent back, so switching users in a test would have had no effect. The test client uses
`https://localhost` instead.

## 6. Small UI fixes from the walkthrough

- The fifth status column on project boards was cut off at desktop width; the minimum column width was reduced.
- "Switched to Corner Bakery Co.." had a doubled full stop because the team name ends in a period.
- The disabled team badge (shown for single-team users) picked up a dark border from Bootstrap's disabled style.
