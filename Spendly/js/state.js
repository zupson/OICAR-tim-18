// Relative to origin — works when served from http://localhost:5153
const API_BASE = '/api';

let state = {
  token: null,
  user: null,
  personalGroupId: null,      // Group.Id  (for creating transactions)
  personalUserGroupId: null,  // UserGroup.Id (for budget linking)
  userGroups: [],
  costs: [],
  revenues: [],
  costTypes: [],
  revenueTypes: [],
  budgets: [],
  txnFilter: 'all',
  modalType: 'exp',
  catModalType: 'exp',
  dashMonth: null,
  dashYear: null,
  reportMonth: null,
  reportYear: null,
  editingTxnId: null,
  editingTxnType: null,
  editingCatId: null,
  editingCatKind: null,
  budgetMonth: null,
  budgetYear: null,
};

function authHdrs() {
  return { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + state.token };
}

async function api(method, path, body) {
  const opts = { method, headers: authHdrs() };
  if (body !== undefined) opts.body = JSON.stringify(body);
  const res = await fetch(API_BASE + path, opts);
  if (res.status === 204) return null;
  const text = await res.text();
  if (!res.ok) throw new Error(text || res.statusText);
  return text ? JSON.parse(text) : null;
}

async function apiAnon(method, path, body) {
  const res = await fetch(API_BASE + path, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  const text = await res.text();
  if (!res.ok) throw new Error(text || res.statusText);
  return JSON.parse(text);
}

function getCurrencySymbol() {
  try {
    const c = JSON.parse(localStorage.getItem('spendly_currency') || 'null');
    return c && c.symbol ? c.symbol : '€';
  } catch { return '€'; }
}

// Pure number formatter (no currency symbol)
function fmtNum(n) {
  return Number(n).toFixed(2).replace('.', ',').replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

// Number + currency symbol
function fmt(n) {
  return getCurrencySymbol() + fmtNum(n);
}

function setEl(id, html) {
  const e = document.getElementById(id);
  if (e) e.innerHTML = html;
}

function showMsg(id, text, isError) {
  const e = document.getElementById(id);
  if (!e) return;
  e.textContent = text;
  e.style.display = text ? 'block' : 'none';
  e.style.color = isError ? '#EF4444' : '#22C55E';
}
