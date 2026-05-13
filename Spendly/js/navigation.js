function goTo(screen) {
  document.querySelectorAll('.screen').forEach(s => s.style.display = 'none');
  const appPages = ['dashboard','transactions','categories','reports','family','budget','notifications','settings'];
  if (appPages.includes(screen)) {
    document.getElementById('app-shell').style.display = 'block';
    showPage(screen);
  } else {
    const el = document.getElementById('scr-' + screen);
    if (el) el.style.display = 'block';
  }
}

function showPage(page) {
  document.querySelectorAll('.app-screen').forEach(s => s.classList.remove('active'));
  document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));

  const pg  = document.getElementById('page-' + page);
  const nav = document.getElementById('nav-' + page);
  if (pg)  pg.classList.add('active');
  if (nav) nav.classList.add('active');

  updateBottomNav(page);

  if (page === 'reports')       renderReports();
  if (page === 'family')        renderFamily();
  if (page === 'budget')        renderBudget();
  if (page === 'settings')      renderSettings();
  if (page === 'notifications') renderNotifications();
}

// Maps page names to the closest bottom-nav item
function updateBottomNav(page) {
  document.querySelectorAll('.bn-item').forEach(n => n.classList.remove('active'));
  const map = {
    dashboard: 'bn-dashboard',
    transactions: 'bn-transactions',
    categories: 'bn-transactions',
    reports: 'bn-reports',
    family: 'bn-family',
    budget: 'bn-family',
    notifications: 'bn-settings',
    settings: 'bn-settings',
  };
  const id = map[page];
  const el = id && document.getElementById(id);
  if (el) el.classList.add('active');
}

function setTab(el) {
  el.closest('.type-tabs').querySelectorAll('.t-tab').forEach(t => t.classList.remove('active'));
  el.classList.add('active');
  const txt = el.textContent.trim();
  state.txnFilter = txt === 'Prihodi' ? 'inc' : txt === 'Troškovi' ? 'exp' : 'all';
  const catSel = document.getElementById('txn-cat-filter');
  if (catSel) catSel.value = '';
  resetTxnDropdowns();
  renderTransactions();
}

function setMTab(el) {
  el.closest('.month-tabs').querySelectorAll('.m-tab').forEach(t => t.classList.remove('active'));
  el.classList.add('active');
}

function setSTab(el, panelId) {
  el.closest('.s-nav').querySelectorAll('.s-nav-item').forEach(i => i.classList.remove('active'));
  el.classList.add('active');
  document.querySelectorAll('#s-profile,#s-security,#s-notifpref,#s-currency').forEach(p => p.style.display = 'none');
  const panel = document.getElementById(panelId);
  if (panel) panel.style.display = 'block';
  if (panelId === 's-notifpref') renderNotifPrefs();
  if (panelId === 's-currency')  renderCurrencyOptions();
}

function setScopeBtn(el) {
  el.closest('.scope-btns').querySelectorAll('.scope-btn').forEach(b => b.classList.remove('active'));
  el.classList.add('active');
}

