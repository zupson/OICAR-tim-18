const NOTIF_STORE_KEY = 'spendly_notifications';

// ── Storage ───────────────────────────────────────────────────────────────────

function getNotifications() {
  try { return JSON.parse(localStorage.getItem(NOTIF_STORE_KEY) || '[]'); }
  catch { return []; }
}

function saveNotifications(notifs) {
  localStorage.setItem(NOTIF_STORE_KEY, JSON.stringify(notifs));
}

// Only adds if an entry with the same id doesn't already exist
function addNotification(notif) {
  const notifs = getNotifications();
  if (notifs.find(n => n.id === notif.id)) return;
  notifs.unshift(notif);
  saveNotifications(notifs);
  updateNotifBadge();
}

function markNotifRead(id) {
  const notifs = getNotifications();
  const n = notifs.find(n => n.id === id);
  if (n && !n.read) { n.read = true; saveNotifications(notifs); updateNotifBadge(); }
}

// ── Badge ─────────────────────────────────────────────────────────────────────

function updateNotifBadge() {
  const count = getNotifications().filter(n => !n.read).length;
  const badge = document.getElementById('notif-badge');
  if (!badge) return;
  if (count > 0) {
    badge.textContent = count;
    badge.style.display = '';
  } else {
    badge.style.display = 'none';
  }
}

// ── Budget alerts ─────────────────────────────────────────────────────────────

// Removes notifications whose thresholds are no longer met (call after deleting/editing costs)
function pruneBudgetAlerts() {
  const now = new Date();
  const m = now.getMonth() + 1, y = now.getFullYear();
  if (!state.personalUserGroupId) return;

  const id80  = `budget-80-${state.personalUserGroupId}-${m}-${y}`;
  const id100 = `budget-100-${state.personalUserGroupId}-${m}-${y}`;

  const budget = state.budgets.find(b =>
    b.userGroupId === state.personalUserGroupId && b.month === m && b.year === y
  );

  let notifs = getNotifications();
  const before = notifs.length;

  if (!budget) {
    notifs = notifs.filter(n => n.id !== id80 && n.id !== id100);
  } else {
    const spent = state.costs
      .filter(c => { const d = new Date(c.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; })
      .reduce((s,c) => s + c.amount, 0);
    const pct = Number(budget.amount) > 0 ? (spent / Number(budget.amount) * 100) : 0;
    if (pct < 100) notifs = notifs.filter(n => n.id !== id100);
    if (pct < 80)  notifs = notifs.filter(n => n.id !== id80);
  }

  if (notifs.length !== before) { saveNotifications(notifs); updateNotifBadge(); }
}


function checkBudgetAlerts() {
  const prefs = getNotifPrefs();
  if (!prefs.budget_alert) return;

  const now = new Date();
  const m = now.getMonth() + 1, y = now.getFullYear();

  const budget = state.budgets.find(b =>
    b.userGroupId === state.personalUserGroupId && b.month === m && b.year === y
  );
  if (!budget) return;

  const spent = state.costs
    .filter(c => { const d = new Date(c.transactionDate); return d.getMonth() + 1 === m && d.getFullYear() === y; })
    .reduce((s, c) => s + c.amount, 0);

  const limit = Number(budget.amount);
  const pct   = limit > 0 ? (spent / limit * 100) : 0;

  if (pct >= 100) {
    addNotification({
      id:        `budget-100-${budget.userGroupId}-${m}-${y}`,
      type:      'budget_100',
      title:     'Budžet prekoračen!',
      desc:      `Potrošnja za ${MONTHS_CAP[m - 1]} ${y}. premašila je postavljeni limit. Potrošeno ${fmt(spent)} od ${fmt(limit)}.`,
      timestamp: new Date().toISOString(),
      read:      false,
    });
  } else if (pct >= 80) {
    addNotification({
      id:        `budget-80-${budget.userGroupId}-${m}-${y}`,
      type:      'budget_80',
      title:     'Limit budžeta se približava!',
      desc:      `Budžet za ${MONTHS_CAP[m - 1]} ${y}. dostigao je ${Math.round(pct)}% — potrošeno ${fmt(spent)} od ${fmt(limit)}.`,
      timestamp: new Date().toISOString(),
      read:      false,
    });
  }
}

// ── Render ────────────────────────────────────────────────────────────────────

const NOTIF_ICONS = {
  budget_80:  { bg: '#3a1200', icon: '⚠️' },
  budget_100: { bg: '#3a0000', icon: '🚨' },
};

function formatNotifTime(iso) {
  if (!iso) return '';
  const d       = new Date(iso);
  const diffMs  = Date.now() - d;
  const diffMin = Math.floor(diffMs / 60000);
  const diffHr  = Math.floor(diffMs / 3600000);
  const diffDay = Math.floor(diffMs / 86400000);
  if (diffMin < 1)  return 'Upravo';
  if (diffMin < 60) return diffMin + 'm';
  if (diffHr  < 24) return diffHr + 'h';
  if (diffDay === 1) return 'Jučer';
  return d.toLocaleDateString('hr-HR', { day: 'numeric', month: 'short' });
}

function renderNotifications() {
  const notifs  = getNotifications();
  const unread  = notifs.filter(n => !n.read).length;

  // Update subtitle
  const sub = document.querySelector('#page-notifications .page-sub');
  if (sub) sub.textContent = unread > 0 ? unread + ' nepročitanih obavijesti' : 'Sve obavijesti su pročitane';

  const list = document.getElementById('notif-list');
  if (!list) return;

  if (!notifs.length) {
    list.innerHTML = '<div style="text-align:center;padding:32px;color:#64748B;font-size:13px">Nema obavijesti.</div>';
    return;
  }

  list.innerHTML = notifs.map(n => {
    const ic   = NOTIF_ICONS[n.type] || { bg: '#1e3a5f', icon: '🔔' };
    const time = formatNotifTime(n.timestamp);
    return `
      <div class="notif-item ${n.read ? 'read' : 'unread'}" onclick="markNotifRead('${n.id}');this.classList.replace('unread','read');this.querySelector('.n-dot')?.remove();updateNotifBadge();renderNotifications()">
        <div style="display:flex;gap:9px;align-items:flex-start;width:100%">
          <div class="n-icon" style="background:${ic.bg}">${ic.icon}</div>
          <div class="n-body">
            <div class="n-title">${n.title}</div>
            <div class="n-desc">${n.desc}</div>
          </div>
          <div style="display:flex;flex-direction:column;align-items:flex-end;gap:5px">
            <div class="n-time">${time}</div>
            ${n.read ? '' : '<div class="n-dot"></div>'}
          </div>
        </div>
      </div>`;
  }).join('');
}

function markAllRead() {
  const notifs = getNotifications().map(n => ({ ...n, read: true }));
  saveNotifications(notifs);
  updateNotifBadge();
  renderNotifications();
}
