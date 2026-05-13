// ── Currency list ─────────────────────────────────────────────────────────────
const CURRENCIES = [
  { code: 'EUR', symbol: '€',   name: 'Euro' },
  { code: 'USD', symbol: '$',   name: 'Američki dolar' },
  { code: 'GBP', symbol: '£',   name: 'Britanska funta' },
  { code: 'CHF', symbol: 'Fr',  name: 'Švicarski franak' },
  { code: 'JPY', symbol: '¥',   name: 'Japanski jen' },
  { code: 'CAD', symbol: 'C$',  name: 'Kanadski dolar' },
  { code: 'AUD', symbol: 'A$',  name: 'Australski dolar' },
  { code: 'NZD', symbol: 'NZ$', name: 'Novozelandski dolar' },
  { code: 'SEK', symbol: 'kr',  name: 'Švedska kruna' },
  { code: 'NOK', symbol: 'kr',  name: 'Norveška kruna' },
  { code: 'DKK', symbol: 'kr',  name: 'Danska kruna' },
  { code: 'HUF', symbol: 'Ft',  name: 'Mađarska forinta' },
  { code: 'CZK', symbol: 'Kč',  name: 'Češka kruna' },
  { code: 'PLN', symbol: 'zł',  name: 'Poljski zlot' },
  { code: 'RON', symbol: 'lei', name: 'Rumunjski leu' },
  { code: 'BGN', symbol: 'лв',  name: 'Bugarski lev' },
  { code: 'RSD', symbol: 'din', name: 'Srpski dinar' },
  { code: 'BAM', symbol: 'KM',  name: 'Bosanska marka' },
  { code: 'MKD', symbol: 'ден', name: 'Makedonski denar' },
  { code: 'ALL', symbol: 'L',   name: 'Albanski lek' },
  { code: 'TRY', symbol: '₺',   name: 'Turska lira' },
  { code: 'RUB', symbol: '₽',   name: 'Ruski rubalj' },
  { code: 'UAH', symbol: '₴',   name: 'Ukrajinska hrivnja' },
  { code: 'CNY', symbol: '¥',   name: 'Kineski yuan' },
  { code: 'KRW', symbol: '₩',   name: 'Južnokorejski von' },
  { code: 'INR', symbol: '₹',   name: 'Indijska rupija' },
  { code: 'SGD', symbol: 'S$',  name: 'Singapurski dolar' },
  { code: 'HKD', symbol: 'HK$', name: 'Hongkonški dolar' },
  { code: 'MYR', symbol: 'RM',  name: 'Malezijski ringgit' },
  { code: 'THB', symbol: '฿',   name: 'Tajlandski baht' },
  { code: 'IDR', symbol: 'Rp',  name: 'Indonezijska rupija' },
  { code: 'PHP', symbol: '₱',   name: 'Filipinski peso' },
  { code: 'VND', symbol: '₫',   name: 'Vijetnamski dong' },
  { code: 'BRL', symbol: 'R$',  name: 'Brazilski real' },
  { code: 'MXN', symbol: 'MX$', name: 'Meksički peso' },
  { code: 'ARS', symbol: '$',   name: 'Argentinski peso' },
  { code: 'CLP', symbol: '$',   name: 'Čileanski peso' },
  { code: 'ZAR', symbol: 'R',   name: 'Južnoafrički rand' },
  { code: 'NGN', symbol: '₦',   name: 'Nigerijska naira' },
  { code: 'EGP', symbol: '£',   name: 'Egipatska funta' },
  { code: 'AED', symbol: 'د.إ', name: 'UAE dirham' },
  { code: 'SAR', symbol: '﷼',   name: 'Saudijski rijal' },
  { code: 'QAR', symbol: '﷼',   name: 'Katarski rijal' },
  { code: 'ILS', symbol: '₪',   name: 'Izraelski novi šekel' },
];

function getCurrentCurrency() {
  try {
    return JSON.parse(localStorage.getItem('spendly_currency') || 'null') || CURRENCIES[0];
  } catch { return CURRENCIES[0]; }
}

function renderCurrencyOptions() {
  const sel = document.getElementById('currency-select');
  if (!sel) return;
  const current = getCurrentCurrency();
  sel.innerHTML = CURRENCIES.map(c =>
    `<option value="${c.code}" ${c.code === current.code ? 'selected' : ''}>${c.symbol} ${c.name} (${c.code})</option>`
  ).join('');
}

function saveCurrency() {
  const sel = document.getElementById('currency-select');
  if (!sel) return;
  const chosen = CURRENCIES.find(c => c.code === sel.value) || CURRENCIES[0];
  localStorage.setItem('spendly_currency', JSON.stringify(chosen));
  showMsg('settings-currency-msg', '✓ Valuta postavljena na ' + chosen.symbol + ' ' + chosen.name, false);
  // Re-render all pages that display amounts
  renderDashboard();
  renderTransactions();
  renderCategories();
  renderBudget();
  renderReports();
}

// ── Notification preferences ──────────────────────────────────────────────────

const NOTIF_DEFAULTS = {
  budget_alert:    true,
  shared_costs:    true,
  report_ready:    true,
  member_activity: false,
};

function getNotifPrefs() {
  try {
    return { ...NOTIF_DEFAULTS, ...(JSON.parse(localStorage.getItem('spendly_notif_prefs') || 'null') || {}) };
  } catch { return { ...NOTIF_DEFAULTS }; }
}

function renderNotifPrefs() {
  const prefs = getNotifPrefs();
  const map = {
    'notif-budget_alert':    'budget_alert',
    'notif-shared_costs':    'shared_costs',
    'notif-report_ready':    'report_ready',
    'notif-member_activity': 'member_activity',
  };
  Object.entries(map).forEach(([elId, key]) => {
    const el = document.getElementById(elId);
    if (!el) return;
    const on = prefs[key] !== false;
    el.classList.remove('on', 'off');
    el.classList.add(on ? 'on' : 'off');
    el.style.justifyContent = on ? 'flex-end' : 'flex-start';
  });
}

function toggleNotif(el, key) {
  if (el.classList.contains('on')) {
    el.classList.replace('on', 'off');
    el.style.justifyContent = 'flex-start';
  } else {
    el.classList.replace('off', 'on');
    el.style.justifyContent = 'flex-end';
  }
  const prefs = getNotifPrefs();
  prefs[key] = el.classList.contains('on');
  localStorage.setItem('spendly_notif_prefs', JSON.stringify(prefs));
}

// Generic toggle (used for switches outside of notifications)
function toggleSwitch(el) {
  if (el.classList.contains('on')) {
    el.classList.replace('on', 'off');
    el.style.justifyContent = 'flex-start';
  } else {
    el.classList.replace('off', 'on');
    el.style.justifyContent = 'flex-end';
  }
}

// ── Profile ───────────────────────────────────────────────────────────────────

function renderSettings() {
  if (!state.user) return;
  const u        = state.user;
  const initials = u.firstName.charAt(0).toUpperCase();
  const fullName = u.firstName + ' ' + u.lastName;

  const avEl = document.getElementById('settings-avatar');
  if (avEl) avEl.textContent = initials;
  setEl('settings-fullname',       fullName);
  setEl('settings-email-display',  u.email || u.username);

  const fnEl = document.getElementById('settings-firstname');
  const lnEl = document.getElementById('settings-lastname');
  const emEl = document.getElementById('settings-email');
  if (fnEl) fnEl.value = u.firstName || '';
  if (lnEl) lnEl.value = u.lastName  || '';
  if (emEl) emEl.value = u.email     || '';

  showMsg('settings-profile-msg', '', false);
}

async function saveProfileChanges() {
  const firstName = document.getElementById('settings-firstname')?.value.trim();
  const lastName  = document.getElementById('settings-lastname')?.value.trim();
  showMsg('settings-profile-msg', '', false);
  if (!firstName || !lastName) {
    showMsg('settings-profile-msg', 'Ime i prezime su obavezni.', true);
    return;
  }
  try {
    await api('PUT', '/User/EditMyProfile', { firstName, lastName });
    state.user.firstName = firstName;
    state.user.lastName  = lastName;
    localStorage.setItem('spendly_user', JSON.stringify(state.user));
    updateSidebar();
    setEl('settings-fullname', firstName + ' ' + lastName);
    const avEl = document.getElementById('settings-avatar');
    if (avEl) avEl.textContent = firstName.charAt(0).toUpperCase();
    showMsg('settings-profile-msg', '✓ Profil uspješno ažuriran!', false);
  } catch (e) {
    const isNotFound = e.message.includes('Not Found') || e.message.includes('404') || e.message.includes('not found');
    showMsg('settings-profile-msg',
      isNotFound
        ? 'Greška: API treba endpoint PUT /api/User/EditMyProfile'
        : 'Greška: ' + friendlyErr(e),
      true);
  }
}

// ── Password ──────────────────────────────────────────────────────────────────

async function changePassword() {
  const currPw = document.getElementById('settings-curr-pw')?.value;
  const newPw  = document.getElementById('settings-new-pw')?.value;
  const confPw = document.getElementById('settings-confirm-pw')?.value;
  showMsg('settings-pw-msg', '', false);
  if (!currPw || !newPw || !confPw) { showMsg('settings-pw-msg', 'Sva polja su obavezna.', true); return; }
  if (newPw !== confPw)             { showMsg('settings-pw-msg', 'Nove lozinke se ne podudaraju.', true); return; }
  if (newPw.length < 8)            { showMsg('settings-pw-msg', 'Lozinka mora imati najmanje 8 znakova.', true); return; }
  try {
    await api('PUT', '/User/ChangePassword', { currentPassword: currPw, password: newPw });
    showMsg('settings-pw-msg', '✓ Lozinka uspješno promijenjena!', false);
    ['settings-curr-pw', 'settings-new-pw', 'settings-confirm-pw'].forEach(id => {
      const el = document.getElementById(id);
      if (el) el.value = '';
    });
  } catch (e) {
    showMsg('settings-pw-msg', 'Greška: ' + friendlyErr(e), true);
  }
}
