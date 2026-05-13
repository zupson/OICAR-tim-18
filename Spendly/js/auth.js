function friendlyErr(e) {
  if (e.message && e.message.toLowerCase().includes('failed to fetch'))
    return 'Ne mogu se spojiti na server. Provjerite je li backend pokrenut na http://localhost:5153.';
  try {
    const obj = JSON.parse(e.message);
    if (obj && obj.errors) return Object.values(obj.errors).flat().join(' ');
    if (typeof obj === 'string') return obj;
  } catch {}
  return e.message || 'Nepoznata greška.';
}

async function loginUser() {
  const username = document.getElementById('login-username').value.trim();
  const password = document.getElementById('login-password').value;
  showMsg('login-error', '', false);
  if (!username || !password) {
    showMsg('login-error', 'Unesite korisničko ime i lozinku.', true);
    return;
  }
  try {
    const data = await apiAnon('POST', '/User/LoginUser', { username, password });
    state.token = data.token;
    state.user  = data.user;
    localStorage.setItem('spendly_token', data.token);
    localStorage.setItem('spendly_user', JSON.stringify(data.user));
    await loadUserData();
    goTo('dashboard');
  } catch (e) {
    showMsg('login-error', friendlyErr(e), true);
  }
}

async function registerUser() {
  const firstName = document.getElementById('reg-firstname').value.trim();
  const lastName  = document.getElementById('reg-lastname').value.trim();
  const email     = document.getElementById('reg-email').value.trim();
  const username  = document.getElementById('reg-username').value.trim();
  const password  = document.getElementById('reg-password').value;
  const password2 = document.getElementById('reg-password2').value;
  showMsg('reg-error', '', false);
  if (!firstName || !lastName || !email || !username || !password) {
    showMsg('reg-error', 'Sva polja su obavezna.', true);
    return;
  }
  if (password !== password2) { showMsg('reg-error', 'Lozinke se ne podudaraju.', true); return; }
  if (password.length < 8)    { showMsg('reg-error', 'Lozinka mora imati najmanje 8 znakova.', true); return; }
  try {
    const data = await apiAnon('POST', '/User/RegisterUser', { firstName, lastName, email, username, password });
    state.token = data.token;
    state.user  = data.user;
    localStorage.setItem('spendly_token', data.token);
    localStorage.setItem('spendly_user', JSON.stringify(data.user));
    await loadUserData();
    goTo('dashboard');
  } catch (e) {
    showMsg('reg-error', friendlyErr(e), true);
  }
}

function logout() {
  state = {
    token: null, user: null, personalGroupId: null, personalUserGroupId: null,
    userGroups: [], costs: [], revenues: [], costTypes: [], revenueTypes: [], budgets: [],
    txnFilter: 'all', modalType: 'exp', catModalType: 'exp',
    dashMonth: null, dashYear: null, reportMonth: null, reportYear: null,
    editingTxnId: null, editingTxnType: null,
    editingCatId: null, editingCatKind: null,
    budgetMonth: null, budgetYear: null,
  };
  localStorage.removeItem('spendly_token');
  localStorage.removeItem('spendly_user');
  goTo('login');
}

async function loadUserData() {
  // Snapshot token so we can detect account switch mid-fetch
  const loadToken = state.token;

  // Wipe stale data so no old values are rendered
  state.personalGroupId = null; state.personalUserGroupId = null;
  state.userGroups = []; state.costs = []; state.revenues = [];
  state.costTypes = []; state.revenueTypes = []; state.budgets = [];
  state.dashMonth = null; state.dashYear = null;
  state.reportMonth = null; state.reportYear = null;

  const [groups, costs, revenues, costTypes, revenueTypes, budgets] = await Promise.all([
    api('GET', '/UserGroup/GetAllUserGroups'),
    api('GET', '/Cost/GetAllCosts'),
    api('GET', '/Revenue/GetAllRevenues'),
    api('GET', '/CostType/GetAllCostTypes'),
    api('GET', '/RevenueType/GetAllRevenueTypes'),
    api('GET', '/Budget/GetAllBudgets'),
  ]);

  // Discard results if user switched accounts while fetching
  if (state.token !== loadToken) return;

  state.userGroups   = groups        || [];
  state.costs        = costs        || [];
  state.revenues     = revenues     || [];
  state.costTypes    = costTypes    || [];
  state.revenueTypes = revenueTypes || [];
  state.budgets      = budgets      || [];

  if (groups && groups.length > 0) {
    state.personalGroupId     = groups[0].groupId;
    state.personalUserGroupId = groups[0].id;
  }

  resetTxnDropdowns();
  updateSidebar();
  renderDashboard();
  renderTransactions();
  renderCategories();
  checkBudgetAlerts();
  updateNotifBadge();
}

function updateSidebar() {
  if (!state.user) return;
  const name = state.user.firstName + ' ' + state.user.lastName.charAt(0) + '.';
  setEl('sb-username', name);
  const av = document.getElementById('sb-avatar');
  if (av) av.textContent = state.user.firstName.charAt(0).toUpperCase();
}

window.addEventListener('load', () => {
  const tok = localStorage.getItem('spendly_token');
  const usr = localStorage.getItem('spendly_user');
  if (tok && usr) {
    state.token = tok;
    state.user  = JSON.parse(usr);
    loadUserData()
      .then(() => goTo('dashboard'))
      .catch(() => { localStorage.clear(); goTo('login'); });
  }
});
