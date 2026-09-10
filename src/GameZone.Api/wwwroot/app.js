const METHODS = [
  { id: 1, name: "Cash" },
  { id: 2, name: "UPI" },
  { id: 6, name: "Free" },
  { id: 7, name: "Discount" }
];
const ROLES = [
  { id: 1, name: "Admin" },
  { id: 2, name: "Employee" }
];

const state = { token: localStorage.getItem("gz_token"), user: null, page: "dashboard" };
const loginView = document.getElementById("login-view");
const shellView = document.getElementById("shell-view");
const content = document.getElementById("content");
const pageTitle = document.getElementById("page-title");
const pageError = document.getElementById("page-error");
const userChip = document.getElementById("user-chip");

const titles = {
  dashboard: "Dashboard",
  sessions: "Active Sessions",
  clients: "Clients",
  payments: "Payments",
  more: "More",
  today: "Today's Clients",
  items: "Gaming Items",
  reports: "Reports",
  users: "User Management"
};

function isAdmin() {
  return Boolean(state.user?.isAdmin || state.user?.role === 1 || state.user?.role === "Admin");
}

function money(value) {
  return `Rs ${Number(value || 0).toFixed(2)}`;
}

function when(value) {
  return value ? new Date(value).toLocaleString() : "-";
}

function methodName(id) {
  return METHODS.find(m => m.id === Number(id))?.name || id || "-";
}

function roleName(id) {
  return ROLES.find(r => r.id === Number(id))?.name || id || "-";
}

function slotRates(session, items) {
  const item = items.find(i => i.id === session.gamingItemId) || {};
  const twoPlus = (session.playerCount || 1) >= 2;
  let hourRate = Number(twoPlus ? item.rateOneHourTwoPlayers : item.rateOneHourOnePlayer) || 0;
  let halfRate = Number(twoPlus ? item.rate30MinTwoPlayers : item.rate30MinOnePlayer) || 0;
  if (!(hourRate > 0) && item.ratePerHour > 0) hourRate = Number(item.ratePerHour);
  if (!(halfRate > 0) && hourRate > 0) halfRate = Math.round((hourRate / 2) * 100) / 100;
  const billed = billedSlotMinutes(session.entryTime);
  const fullHours = Math.floor(billed / 60);
  const extraHalf = billed % 60 >= 30;
  const full = Math.round(((fullHours * hourRate) + (extraHalf ? halfRate : 0)) * 100) / 100;
  return { full, half: halfRate };
}

const DISCOUNT_PERCENTS = [5, 10, 15, 20, 25, 30, 40, 50, 75, 100];

function checkoutMethods() {
  return [
    { id: 1, name: "Cash" },
    { id: 2, name: "UPI" },
    { id: 6, name: "Free" },
    { id: 7, name: "Discount" }
  ];
}

function checkoutTotals(method, bill, percent) {
  const pct = Math.max(0, Math.min(100, Number(percent) || 0));
  if (method === 6)
    return { amount: 0, discount: bill.full };
  if (method === 7) {
    const discount = Math.round(bill.full * pct) / 100;
    return { amount: Math.max(0, Math.round((bill.full - discount) * 100) / 100), discount };
  }
  return { amount: bill.full, discount: 0 };
}

function paintCheckout(id, session, items) {
  const method = Number(document.getElementById(`pay-method-${id}`).value);
  const percentEl = document.getElementById(`pay-pct-${id}`);
  const percentWrap = document.getElementById(`pay-pct-wrap-${id}`);
  const amountWrap = document.getElementById(`pay-amt-wrap-${id}`);
  if (percentWrap) percentWrap.style.display = method === 7 ? "" : "none";
  if (amountWrap) amountWrap.style.display = method === 6 ? "none" : "";
  const totals = checkoutTotals(method, slotRates(session, items), percentEl?.value);
  const amountEl = document.getElementById(`pay-amt-${id}`);
  const discountEl = document.getElementById(`pay-disc-${id}`);
  if (amountEl) amountEl.textContent = money(totals.amount);
  if (discountEl) discountEl.textContent = money(totals.discount);
}

function billedSlotMinutes(entryTime) {
  const actual = Math.max(0, Math.ceil((Date.now() - new Date(entryTime).getTime()) / 60000));
  const chargeable = Math.max(1, actual - 5);
  return Math.max(30, Math.ceil(chargeable / 30) * 30);
}

function options(list, valueKey, labelKey, selected) {
  return list.map(item => `<option value="${item[valueKey]}" ${String(item[valueKey]) === String(selected) ? "selected" : ""}>${item[labelKey]}</option>`).join("");
}

async function closeSession(id) {
  try {
    return await api(`/api/sessions/${id}/end`, { method: "POST", body: "{}" });
  } catch {
    const session = await api(`/api/sessions/${id}`);
    return { session, amount: session.amount || 0 };
  }
}

function methodSelect(id, selected = 1) {
  return `<select id="${id}">${options(METHODS, "id", "name", selected)}</select>`;
}

function friendlyError(text, status) {
  const raw = String(text || "");
  if (/1033|Cloudflare Tunnel error|trycloudflare|cf-error/i.test(raw) || raw.includes("<html") || raw.includes("<!DOCTYPE"))
    return "Cannot reach GameZONE. Keep this PC on with the public tunnel running, then try Login again.";
  return (raw.split("\n")[0] || `Request failed (${status || "error"})`).slice(0, 160);
}

async function api(path, options = {}) {
  const headers = { "Content-Type": "application/json", ...(options.headers || {}) };
  if (state.token) headers.Authorization = `Bearer ${state.token}`;
  const ctrl = new AbortController();
  const timer = setTimeout(() => ctrl.abort(), 20000);
  let response;
  try {
    response = await fetch(path, { ...options, headers, signal: ctrl.signal });
  } catch (ex) {
    if (ex.name === "AbortError")
      throw new Error("Server is slow or the public tunnel dropped. Try again.");
    throw new Error("Cannot reach the server. Check the public URL and try again.");
  } finally {
    clearTimeout(timer);
  }
  const text = await response.text();
  let data = null;
  try { data = text ? JSON.parse(text) : null; } catch { data = null; }
  if (!response.ok)
    throw new Error(friendlyError(data?.error || text, response.status));
  return data;
}

async function safe(action) {
  pageError.textContent = "";
  try {
    await action();
  } catch (ex) {
    pageError.textContent = ex.message;
  }
}

function showLogin() {
  loginView.classList.remove("hidden");
  shellView.classList.add("hidden");
}

function showShell() {
  loginView.classList.add("hidden");
  shellView.classList.remove("hidden");
  userChip.textContent = state.user?.name || "User";
  buildNav();
}

function buildNav() {
  document.getElementById("main-nav").innerHTML = ["dashboard", "sessions", "clients", "payments", "more"]
    .map(page => `<button data-page="${page}" class="${state.page === page || (page === "more" && ["today", "items", "reports", "users"].includes(state.page)) ? "active" : ""}">${titles[page]}</button>`)
    .join("");
  document.querySelectorAll("#main-nav button").forEach(button => {
    button.onclick = () => render(button.dataset.page);
  });
}

document.getElementById("login-form").addEventListener("submit", async event => {
  event.preventDefault();
  const error = document.getElementById("login-error");
  const button = document.getElementById("login-btn");
  error.textContent = "";
  button.disabled = true;
  button.textContent = "Signing in…";
  try {
    const result = await api("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({
        username: document.getElementById("username").value,
        password: document.getElementById("password").value
      })
    });
    state.token = result.token;
    state.user = result.user;
    localStorage.setItem("gz_token", result.token);
    showShell();
    await render("dashboard");
  } catch (ex) {
    error.textContent = ex.message;
  } finally {
    button.disabled = false;
    button.textContent = "Login";
  }
});

function togglePasswordVisibility() {
  const input = document.getElementById("password");
  const toggle = document.getElementById("toggle-password");
  if (!input || !toggle) return;
  const hidden = input.type === "password";
  input.type = hidden ? "text" : "password";
  toggle.classList.toggle("is-visible", hidden);
  toggle.setAttribute("aria-label", hidden ? "Hide password" : "Show password");
  toggle.querySelector(".eye-open")?.classList.toggle("hidden", hidden);
  toggle.querySelector(".eye-off")?.classList.toggle("hidden", !hidden);
}

document.getElementById("toggle-password")?.addEventListener("click", togglePasswordVisibility);
document.getElementById("toggle-password")?.addEventListener("keydown", event => {
  if (event.key === "Enter" || event.key === " ") {
    event.preventDefault();
    togglePasswordVisibility();
  }
});

document.getElementById("logout-btn").onclick = () => {
  state.token = null;
  state.user = null;
  localStorage.removeItem("gz_token");
  showLogin();
};

async function render(page) {
  state.page = page;
  pageError.textContent = "";
  pageTitle.textContent = titles[page] || page;
  buildNav();
  await safe(async () => {
    if (page === "dashboard") await renderDashboard();
    else if (page === "sessions") await renderSessions();
    else if (page === "clients") await renderClients();
    else if (page === "payments") await renderPayments();
    else if (page === "today") await renderToday();
    else if (page === "items") await renderItems();
    else if (page === "reports") await renderReports();
    else if (page === "users") await renderUsers();
    else renderMore();
  });
}

async function renderDashboard() {
  const stats = await api("/api/dashboard");
  content.innerHTML = `
    <p class="muted">Today at the floor</p>
    <div class="grid">
      <div class="card" data-go="today"><div class="muted">Today's clients</div><div class="stat">${stats.todaysClientsCount}</div></div>
      <div class="card" data-go="sessions"><div class="muted">Active sessions</div><div class="stat">${stats.activeSessions}</div></div>
      <div class="card" data-go="payments"><div class="muted">Today's total</div><div class="stat">${money(stats.todaysRevenue)}</div></div>
      <div class="card" data-go="payments"><div class="muted">Pending</div><div class="stat">${stats.pendingPayments}</div></div>
      ${isAdmin() ? `<div class="card wide" data-go="reports"><div class="muted">This month · ${stats.monthLabel || ""}</div><div class="stat">${money(stats.monthlyRevenue)}</div></div>` : ""}
    </div>
    <button data-go="sessions">Start a session</button>
    <button class="ghost" data-go="clients">Add a client</button>`;
  content.querySelectorAll("[data-go]").forEach(el => el.onclick = () => render(el.dataset.go));
}

async function renderSessions() {
  const [sessions, clients, items] = await Promise.all([
    api("/api/sessions/active"),
    api("/api/clients"),
    api("/api/items/active")
  ]);
  content.innerHTML = `
    ${state.notice ? `<p class="ok">${state.notice}</p>` : ""}
    ${sessions.map(s => `
      <div class="card">
        <h3>${s.clientName}</h3>
        <p class="muted">${s.phoneNumber}</p>
        <p>${s.gamingItemName} · ${s.playersText || "1 player"}</p>
        <p class="muted">Started ${when(s.entryTime)}</p>
        ${s.notes ? `<p class="muted">${s.notes}</p>` : ""}
        <label>Payment method</label>
        <select id="pay-method-${s.id}">${options(checkoutMethods(), "id", "name")}</select>
        <div id="pay-pct-wrap-${s.id}" style="display:none">
          <label>Discount %</label>
          <select id="pay-pct-${s.id}">${DISCOUNT_PERCENTS.map(p => `<option value="${p}">${p}%</option>`).join("")}</select>
        </div>
        <p id="pay-amt-wrap-${s.id}">Amount <span id="pay-amt-${s.id}" class="ok"></span></p>
        <p>Discount <span id="pay-disc-${s.id}" class="ok"></span></p>
        <label>Reference</label>
        <input id="pay-ref-${s.id}" placeholder="UPI / card ref" />
        <div class="row">
          <button data-collect="${s.id}" data-entry="${s.entryTime}" data-paid="1">Collect paid</button>
          <button class="ghost" data-collect="${s.id}" data-entry="${s.entryTime}" data-paid="0">Mark pending</button>
        </div>
      </div>`).join("") || `<p class="muted">No active sessions.</p>`}
    <div class="card stack">
      <h3>Start session</h3>
      <label>Search client</label>
      <input id="client-search" placeholder="Name or phone" />
      <label>Client</label>
      <select id="start-client">${options(clients, "id", "displayLabel")}</select>
      <label>Gaming item</label>
      <select id="start-item">${options(items, "id", "displayLabel")}</select>
      <label>Players on this machine</label>
      <select id="start-players">${[1,2,3,4,5,6,7,8].map(n => `<option value="${n}">${n === 1 ? "1 player" : n + " players"}</option>`).join("")}</select>
      <label>Notes</label>
      <textarea id="start-notes" placeholder="Optional"></textarea>
      <button id="start-btn">Start session</button>
    </div>`;

  document.getElementById("client-search").oninput = async e => {
    const list = await api(`/api/clients?search=${encodeURIComponent(e.target.value)}`);
    document.getElementById("start-client").innerHTML = options(list, "id", "displayLabel");
  };
  document.getElementById("start-btn").onclick = () => safe(async () => {
    await api("/api/sessions", {
      method: "POST",
      body: JSON.stringify({
        clientId: Number(document.getElementById("start-client").value),
        gamingItemId: Number(document.getElementById("start-item").value),
        playerCount: Number(document.getElementById("start-players").value || 1),
        notes: document.getElementById("start-notes").value
      })
    });
    await renderSessions();
  });
  content.querySelectorAll("[data-collect]").forEach(btn => {
    btn.onclick = () => safe(async () => {
      const id = btn.dataset.collect;
      const method = Number(document.getElementById(`pay-method-${id}`).value);
      const percent = Number(document.getElementById(`pay-pct-${id}`)?.value || 0);
      const session = sessions.find(x => x.id === Number(id));
      const totals = checkoutTotals(method, slotRates(session, items), percent);
      const paid = btn.dataset.paid === "1";
      const saved = await api(`/api/sessions/${id}/collect`, {
        method: "POST",
        body: JSON.stringify({
          paymentMethod: method,
          discountPercent: percent,
          transactionReference: document.getElementById(`pay-ref-${id}`).value,
          paymentStatus: paid ? 2 : 1
        })
      });
      const clientName = saved.clientName || session?.clientName || "";
      state.notice = `${paid ? "Paid" : "Pending"} ${clientName} · Amount ${money(totals.amount)} · Discount ${money(totals.discount)}`;
      await renderSessions();
      state.notice = "";
    });
  });
  sessions.forEach(s => {
    const select = document.getElementById(`pay-method-${s.id}`);
    if (!select) return;
    paintCheckout(s.id, s, items);
    select.onchange = () => paintCheckout(s.id, s, items);
    const percent = document.getElementById(`pay-pct-${s.id}`);
    if (percent) percent.onchange = () => paintCheckout(s.id, s, items);
  });
}

async function renderClients(term = "") {
  const clients = await api(term ? `/api/clients?search=${encodeURIComponent(term)}` : "/api/clients");
  content.innerHTML = `
    <input id="search-clients" placeholder="Search name or phone" value="${term}" />
    <div class="card stack">
      <h3>Add / edit client</h3>
      <input id="c-id" type="hidden" />
      <input id="c-name" placeholder="Name" />
      <input id="c-phone" placeholder="Phone" />
      <input id="c-address" placeholder="Address" />
      <button id="save-client">Save client</button>
    </div>
    ${clients.map(c => `
      <div class="card">
        <div class="card-head">
          <div>
            <h3>${c.name}</h3>
            <p>${c.phoneNumber}</p>
          </div>
          <div class="client-counts">
            <div class="client-count"><span class="n">${c.totalVisitCount || 0}</span><span class="l">Visits</span></div>
            <div class="client-count"><span class="n">${c.freeUseCount || 0}</span><span class="l">Free</span></div>
          </div>
        </div>
        <p class="muted">${c.address || "No address"} · ${money(c.totalAmountSpent)}</p>
        <p class="muted">Last visit ${c.lastVisitDate ? when(c.lastVisitDate) : "-"}</p>
        <div class="row">
          <button class="tiny ghost" data-edit='${JSON.stringify(c)}'>Edit</button>
          <button class="tiny ghost" data-history="${c.id}">History</button>
          <button class="tiny danger" data-del="${c.id}">Delete</button>
        </div>
        <div id="hist-${c.id}"></div>
      </div>`).join("") || `<p class="muted">No clients yet.</p>`}`;

  document.getElementById("search-clients").onchange = e => renderClients(e.target.value);
  document.getElementById("save-client").onclick = () => safe(async () => {
    const id = document.getElementById("c-id").value;
    const body = {
      name: document.getElementById("c-name").value,
      phoneNumber: document.getElementById("c-phone").value,
      address: document.getElementById("c-address").value
    };
    if (id) await api(`/api/clients/${id}`, { method: "PUT", body: JSON.stringify(body) });
    else await api("/api/clients", { method: "POST", body: JSON.stringify(body) });
    await renderClients();
  });
  content.querySelectorAll("[data-edit]").forEach(btn => {
    btn.onclick = () => {
      const c = JSON.parse(btn.dataset.edit);
      document.getElementById("c-id").value = c.id;
      document.getElementById("c-name").value = c.name;
      document.getElementById("c-phone").value = c.phoneNumber;
      document.getElementById("c-address").value = c.address || "";
      window.scrollTo({ top: 0, behavior: "smooth" });
    };
  });
  content.querySelectorAll("[data-del]").forEach(btn => {
    btn.onclick = () => safe(async () => {
      await api(`/api/clients/${btn.dataset.del}/delete`, { method: "POST" });
      await renderClients(document.getElementById("search-clients").value);
    });
  });
  content.querySelectorAll("[data-history]").forEach(btn => {
    btn.onclick = () => safe(async () => {
      const history = await api(`/api/clients/${btn.dataset.history}/history`);
      document.getElementById(`hist-${btn.dataset.history}`).innerHTML =
        history.visits.map(v => `
          <p class="muted">${when(v.entryTime)} · ${v.gamingItem} · ${v.duration} · ${v.amount == null ? "-" : money(v.amount)} · ${v.paymentStatus}</p>
        `).join("") || `<p class="muted">No visits yet.</p>`;
    });
  });
}

async function renderToday() {
  const sessions = await api("/api/sessions/today");
  content.innerHTML = sessions.map(s => `
    <div class="card">
      <h3>${s.clientName}</h3>
      <p>${s.gamingItemName} · ${s.playersText || "1 player"}</p>
      <p class="muted">${when(s.entryTime)}${s.exitTime ? ` → ${when(s.exitTime)}` : " · in play"}</p>
      <p>${s.statusText} · ${s.durationDisplay} · ${s.amount == null ? "-" : money(s.amount)} · Pay ${s.paymentStatus}</p>
    </div>`).join("") || `<p class="muted">No clients today yet.</p>`;
}

async function renderPayments() {
  const pendingOnly = content.dataset.pending === "1";
  const payments = await api(pendingOnly ? "/api/payments/pending" : "/api/payments");
  content.dataset.pending = pendingOnly ? "1" : "0";
  content.innerHTML = `
    <div class="tabs">
      <button class="${pendingOnly ? "" : "active"}" data-filter="all">All</button>
      <button class="${pendingOnly ? "active" : ""}" data-filter="pending">Pending</button>
    </div>
    ${payments.map(p => `
      <div class="card">
        <h3>${p.clientName}</h3>
        <p>${p.gamingItemName}</p>
        <p>${money(p.amount)} · ${p.methodText || methodName(p.paymentMethod)} · <span class="chip">${p.statusText}</span></p>
        <p class="muted">${when(p.paymentDate)} · ${p.receivedBy}</p>
        ${p.paymentStatus === 1 || p.statusText === "Pending" ? `
          <label>Method</label>
          ${methodSelect(`m-${p.id}`, p.paymentMethod)}
          <input id="r-${p.id}" placeholder="Reference" value="${p.transactionReference || ""}" />
          <button data-paid="${p.id}" data-session="${p.sessionId}" data-amount="${p.amount}">Mark paid</button>` : ""}
      </div>`).join("") || `<p class="muted">No payments.</p>`}`;
  content.querySelectorAll("[data-filter]").forEach(btn => {
    btn.onclick = () => {
      content.dataset.pending = btn.dataset.filter === "pending" ? "1" : "0";
      renderPayments();
    };
  });
  content.querySelectorAll("[data-paid]").forEach(btn => {
    btn.onclick = () => safe(async () => {
      await api(`/api/payments/${btn.dataset.paid}/paid`, {
        method: "POST",
        body: JSON.stringify({
          sessionId: Number(btn.dataset.session),
          amount: Number(btn.dataset.amount),
          paymentMethod: Number(document.getElementById(`m-${btn.dataset.paid}`).value),
          transactionReference: document.getElementById(`r-${btn.dataset.paid}`).value,
          paymentStatus: 2
        })
      });
      await renderPayments();
    });
  });
}

async function renderItems() {
  if (!isAdmin()) {
    content.innerHTML = `<p class="muted">Only administrators can manage gaming items.</p>`;
    return;
  }
  const items = await api("/api/items");
  content.innerHTML = `
    <div class="card stack">
      <h3>Add / edit item</h3>
      <input id="i-id" type="hidden" />
      <input id="i-name" placeholder="Item name" />
      <label>30 min · 1 player</label>
      <input id="i-30-1" type="number" value="80" />
      <label>30 min · 2+ players</label>
      <input id="i-30-2" type="number" value="120" />
      <label>1 hour · 1 player</label>
      <input id="i-60-1" type="number" value="150" />
      <label>1 hour · 2+ players</label>
      <input id="i-60-2" type="number" value="200" />
      <p class="muted">2 or more players use the 2-player prices on the same machine.</p>
      <button id="save-item">Save item</button>
    </div>
    ${items.map(i => `
      <div class="card">
        <h3>${i.name}</h3>
        <p>${i.priceSummary || `${money(i.rate30MinOnePlayer)} / 30m · ${money(i.rateOneHourOnePlayer)} / 1h`}</p>
        <p><span class="chip">${i.statusText}</span></p>
        <div class="row">
          <button class="tiny ghost" data-edit='${JSON.stringify(i)}'>Edit</button>
          <button class="tiny ghost" data-toggle="${i.id}" data-active="${i.isActive}">${i.isActive ? "Deactivate" : "Activate"}</button>
          <button class="tiny danger" data-del="${i.id}">Delete</button>
        </div>
      </div>`).join("")}`;
  document.getElementById("save-item").onclick = () => safe(async () => {
    const id = document.getElementById("i-id").value;
    const body = {
      id: Number(id || 0),
      name: document.getElementById("i-name").value,
      rate30MinOnePlayer: Number(document.getElementById("i-30-1").value),
      rate30MinTwoPlayers: Number(document.getElementById("i-30-2").value),
      rateOneHourOnePlayer: Number(document.getElementById("i-60-1").value),
      rateOneHourTwoPlayers: Number(document.getElementById("i-60-2").value),
      ratePerHour: Number(document.getElementById("i-60-1").value)
    };
    if (id) await api(`/api/items/${id}`, { method: "PUT", body: JSON.stringify(body) });
    else await api("/api/items", { method: "POST", body: JSON.stringify(body) });
    await renderItems();
  });
  content.querySelectorAll("[data-edit]").forEach(btn => {
    btn.onclick = () => {
      const i = JSON.parse(btn.dataset.edit);
      document.getElementById("i-id").value = i.id;
      document.getElementById("i-name").value = i.name;
      document.getElementById("i-30-1").value = i.rate30MinOnePlayer ?? 80;
      document.getElementById("i-30-2").value = i.rate30MinTwoPlayers ?? 120;
      document.getElementById("i-60-1").value = i.rateOneHourOnePlayer || i.ratePerHour || 150;
      document.getElementById("i-60-2").value = i.rateOneHourTwoPlayers ?? 200;
    };
  });
  content.querySelectorAll("[data-toggle]").forEach(btn => {
    btn.onclick = () => safe(async () => {
      const active = btn.dataset.active === "true";
      await api(`/api/items/${btn.dataset.toggle}/${active ? "deactivate" : "activate"}`, { method: "POST" });
      await renderItems();
    });
  });
  content.querySelectorAll("[data-del]").forEach(btn => {
    btn.onclick = () => safe(async () => {
      await api(`/api/items/${btn.dataset.del}/delete`, { method: "POST" });
      await renderItems();
    });
  });
}

async function renderReports() {
  if (!isAdmin()) {
    content.innerHTML = `<p class="muted">Only administrators can view reports.</p>`;
    return;
  }
  const to = new Date();
  const from = new Date();
  from.setDate(to.getDate() - 6);
  const fromValue = from.toISOString().slice(0, 10);
  const toValue = to.toISOString().slice(0, 10);
  content.innerHTML = `
    <div class="row">
      <input id="from" type="date" value="${fromValue}" />
      <input id="to" type="date" value="${toValue}" />
    </div>
    <button id="refresh-reports">Refresh</button>
    <div id="report-body"></div>`;
  const load = () => safe(async () => {
    const f = document.getElementById("from").value;
    const t = document.getElementById("to").value;
    const [daily, items, top, pending] = await Promise.all([
      api(`/api/reports/daily?from=${f}&to=${t}`),
      api(`/api/reports/items?from=${f}&to=${t}`),
      api("/api/reports/top-clients"),
      api("/api/reports/pending")
    ]);
    document.getElementById("report-body").innerHTML = `
      <h3>Daily revenue</h3>
      ${daily.map(d => `<div class="card"><h3>${new Date(d.date).toLocaleDateString()}</h3><p>${money(d.totalRevenue)} · ${d.totalClients} clients</p></div>`).join("") || `<p class="muted">No daily data.</p>`}
      <h3>Revenue by item</h3>
      ${items.map(i => `<div class="card"><h3>${i.itemName}</h3><p>${i.totalSessions} sessions · ${money(i.revenue)}</p></div>`).join("") || `<p class="muted">No item revenue.</p>`}
      <h3>Top clients</h3>
      ${top.map(c => `<div class="card"><h3>${c.clientName}</h3><p>${c.phoneNumber} · ${c.totalVisits} visits · ${money(c.totalAmountSpent)}</p></div>`).join("") || `<p class="muted">No top clients yet.</p>`}
      <h3>Pending dues</h3>
      ${pending.map(p => `<div class="card"><h3>${p.clientName}</h3><p>${p.phoneNumber} · ${p.gamingItem} · ${money(p.amount)}</p></div>`).join("") || `<p class="muted">No pending dues.</p>`}`;
  });
  document.getElementById("refresh-reports").onclick = load;
  await load();
}

async function renderUsers() {
  if (!isAdmin()) {
    content.innerHTML = `<p class="muted">Only administrators can manage users.</p>`;
    return;
  }
  const users = await api("/api/users");
  content.innerHTML = `
    <div class="card stack">
      <h3>Add / edit user</h3>
      <input id="u-id" type="hidden" />
      <input id="u-name" placeholder="Name" />
      <input id="u-username" placeholder="Username" />
      <input id="u-password" type="password" placeholder="Password (new users)" />
      <label>Role</label>
      <select id="u-role">${options(ROLES, "id", "name", 2)}</select>
      <button id="save-user">Save user</button>
    </div>
    ${users.map(u => `
      <div class="card">
        <h3>${u.name}</h3>
        <p>${u.username} · ${u.roleText || roleName(u.role)} · <span class="chip">${u.statusText}</span></p>
        <div class="row">
          <button class="tiny ghost" data-edit='${JSON.stringify(u)}'>Edit</button>
          <button class="tiny ghost" data-toggle="${u.id}" data-active="${u.isActive}">${u.isActive ? "Deactivate" : "Activate"}</button>
          <button class="tiny ghost" data-reset="${u.id}">Reset password</button>
        </div>
        <div id="reset-${u.id}"></div>
      </div>`).join("")}`;
  document.getElementById("save-user").onclick = () => safe(async () => {
    const id = document.getElementById("u-id").value;
    const body = {
      name: document.getElementById("u-name").value,
      username: document.getElementById("u-username").value,
      password: document.getElementById("u-password").value,
      role: Number(document.getElementById("u-role").value)
    };
    if (id) await api(`/api/users/${id}`, { method: "PUT", body: JSON.stringify({ ...body, id: Number(id) }) });
    else await api("/api/users", { method: "POST", body: JSON.stringify(body) });
    await renderUsers();
  });
  content.querySelectorAll("[data-edit]").forEach(btn => {
    btn.onclick = () => {
      const u = JSON.parse(btn.dataset.edit);
      document.getElementById("u-id").value = u.id;
      document.getElementById("u-name").value = u.name;
      document.getElementById("u-username").value = u.username;
      document.getElementById("u-role").value = u.role;
    };
  });
  content.querySelectorAll("[data-toggle]").forEach(btn => {
    btn.onclick = () => safe(async () => {
      const active = btn.dataset.active === "true";
      await api(`/api/users/${btn.dataset.toggle}/${active ? "deactivate" : "activate"}`, { method: "POST" });
      await renderUsers();
    });
  });
  content.querySelectorAll("[data-reset]").forEach(btn => {
    btn.onclick = () => {
      const box = document.getElementById(`reset-${btn.dataset.reset}`);
      box.innerHTML = `<input id="new-pass-${btn.dataset.reset}" type="password" placeholder="New password" /><button id="do-reset-${btn.dataset.reset}">Save password</button>`;
      document.getElementById(`do-reset-${btn.dataset.reset}`).onclick = () => safe(async () => {
        await api(`/api/users/${btn.dataset.reset}/reset-password`, {
          method: "POST",
          body: JSON.stringify({ newPassword: document.getElementById(`new-pass-${btn.dataset.reset}`).value })
        });
        pageError.textContent = "";
        box.innerHTML = `<p class="ok">Password updated.</p>`;
      });
    };
  });
}

function renderMore() {
  content.innerHTML = `
    <button class="menu-link ghost" data-go="today">Today's Clients</button>
    ${isAdmin() ? `<button class="menu-link ghost" data-go="items">Gaming Items</button>` : ""}
    ${isAdmin() ? `<button class="menu-link ghost" data-go="reports">Reports</button>` : ""}
    ${isAdmin() ? `<button class="menu-link ghost" data-go="users">User Management</button>` : ""}
    <div class="card">
      <h3>${state.user?.name || ""}</h3>
      <p class="muted">${state.user?.username || ""} · ${roleName(state.user?.role)}</p>
    </div>`;
  content.querySelectorAll("[data-go]").forEach(btn => btn.onclick = () => render(btn.dataset.go));
}

if (state.token) {
  api("/api/auth/me").then(user => {
    state.user = user;
    showShell();
    render("dashboard");
  }).catch(showLogin);
} else {
  showLogin();
}

if ("serviceWorker" in navigator) {
  navigator.serviceWorker.register("/sw.js").catch(() => {});
}
