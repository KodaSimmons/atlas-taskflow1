// Atlas TaskFlow: keeps the notification bell and the dashboard's "My tasks" panel current.
// The Sprint 3 design called for SignalR; this sprint uses simple polling (the design's own
// fallback plan) because the data is in memory on a single server.
(function () {
  "use strict";

  const POLL_MS = 15000;
  const script = document.currentScript;
  const summaryUrl = script && script.dataset.summaryUrl;
  const bell = document.getElementById("bell-count");
  const myTasks = document.getElementById("my-tasks");

  // Show server UTC timestamps in the viewer's local time zone.
  document.querySelectorAll("time[datetime]").forEach(function (el) {
    const d = new Date(el.getAttribute("datetime"));
    if (!isNaN(d)) {
      el.textContent = d.toLocaleString(undefined, { month: "short", day: "numeric", hour: "numeric", minute: "2-digit" });
    }
  });

  async function refreshBell() {
    if (!summaryUrl || !bell) return;
    const res = await fetch(summaryUrl, { headers: { Accept: "application/json" }, cache: "no-store" });
    if (!res.ok) return;
    const data = await res.json();
    bell.textContent = data.unread;
    bell.classList.toggle("d-none", data.unread === 0);
  }

  async function refreshMyTasks() {
    if (!myTasks) return;
    const res = await fetch(myTasks.dataset.refreshUrl, { cache: "no-store" });
    if (!res.ok) return;
    const html = await res.text();
    if (html !== myTasks.innerHTML) myTasks.innerHTML = html;
  }

  function tick() {
    if (document.hidden) return; // don't poll from background tabs
    refreshBell().catch(function () { /* offline: try again next tick */ });
    refreshMyTasks().catch(function () { });
  }

  setInterval(tick, POLL_MS);
  document.addEventListener("visibilitychange", function () { if (!document.hidden) tick(); });
})();
