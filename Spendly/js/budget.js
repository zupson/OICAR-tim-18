function _budgetMonth() {
  const now = new Date();
  return state.budgetMonth !== null ? state.budgetMonth : now.getMonth() + 1;
}
function _budgetYear() {
  const now = new Date();
  return state.budgetYear !== null ? state.budgetYear : now.getFullYear();
}

function changeBudgetMonth(delta) {
  let m = _budgetMonth(), y = _budgetYear();
  m += delta;
  if (m > 12) { m = 1; y++; }
  if (m < 1)  { m = 12; y--; }
  state.budgetMonth = m;
  state.budgetYear  = y;
  renderBudget();
}

function renderBudget() {
  const m = _budgetMonth(), y = _budgetYear();
  const groupName = state.userGroups.length > 0 ? state.userGroups[0].groupName : 'Vaša grupa';

  setEl('budget-page-sub', groupName + ' · ' + MONTHS_CAP[m - 1] + ' ' + y + '.');
  setEl('budget-month-label', MONTHS_CAP[m - 1] + ' ' + y + '.');

  const budget  = state.budgets.find(b => b.userGroupId === state.personalUserGroupId && b.month === m && b.year === y);
  const mCosts  = state.costs.filter(c => { const d = new Date(c.transactionDate); return d.getMonth() + 1 === m && d.getFullYear() === y; });
  const spent   = mCosts.reduce((s, c) => s + c.amount, 0);

  const container = document.getElementById('budget-hero-container');
  const breakdown = document.getElementById('budget-breakdown');
  if (!container) return;

  if (!budget) {
    container.innerHTML = `
      <div style="text-align:center;padding:28px 16px">
        <div style="font-size:36px;margin-bottom:8px">💰</div>
        <div style="font-size:15px;font-weight:700;color:#fff;margin-bottom:4px">Nema budžeta za ${MONTHS_CAP[m - 1]} ${y}.</div>
        <div style="font-size:12px;color:#64748B;margin-bottom:18px">Postavite budžet za praćenje potrošnje</div>
      </div>
      <div class="card">
        <div class="card-title">Postavi budžet — ${MONTHS_CAP[m - 1]} ${y}.</div>
        <div class="card-sub">Odredite limit potrošnje za ovaj mjesec</div>
        <div style="display:flex;gap:12px;align-items:flex-end;margin-top:12px;flex-wrap:wrap">
          <div style="flex:1;min-width:150px">
            <label class="modal-label">Iznos budžeta</label>
            <input class="modal-fi" id="budget-amount-input" type="number" min="1" step="0.01" placeholder="npr. 3000.00">
          </div>
          <button class="btn-sm" onclick="createBudget()">Spremi budžet</button>
        </div>
        <div id="budget-create-msg" style="display:none;margin-top:8px;font-size:12px"></div>
      </div>`;
    if (breakdown) breakdown.innerHTML = '';
    return;
  }

  const limit     = Number(budget.amount);
  const remaining = limit - spent;
  const pct       = Math.min(100, Math.round(spent / limit * 100));
  const barColor  = pct < 60 ? '#22C55E' : pct < 85 ? '#F59E0B' : '#EF4444';
  const now2      = new Date();
  const daysIn    = new Date(y, m, 0).getDate();
  const today     = (y === now2.getFullYear() && m === now2.getMonth() + 1) ? now2.getDate() : daysIn;
  const daysLeft  = daysIn - today;
  const dailyAvg  = today > 0 ? spent / today : 0;

  container.innerHTML = `
    <div class="budget-hero">
      <div class="bh-top">
        <div>
          <div class="bh-limit">Miesečni budžet — ${groupName}</div>
          <div class="bh-val">${fmt(limit)} <span style="font-size:13px;color:#64748B;font-weight:400">limit</span></div>
        </div>
        <div style="text-align:right">
          <div style="font-size:12px;color:#64748B;margin-bottom:3px">Dosad potrošeno</div>
          <div style="font-size:22px;font-weight:700;color:${barColor}">${fmt(spent)}</div>
          <div style="font-size:11px;color:#64748B">${pct}% iskorišteno</div>
        </div>
      </div>
      <div class="prog-bg" style="height:11px">
        <div class="prog-fill" style="width:${pct}%;background:linear-gradient(90deg,#22C55E,${barColor})"></div>
      </div>
      <div style="display:flex;justify-content:space-between;margin-top:5px;font-size:10px;color:#64748B">
        <span>0</span>
        <span style="color:${barColor};font-weight:600">${fmt(spent)} potrošeno</span>
        <span>${fmt(limit)}</span>
      </div>
      <div class="bh-stats">
        <div class="bh-stat"><div class="bh-stat-lbl">Preostalo</div><div class="bh-stat-val" style="color:${remaining >= 0 ? '#22C55E' : '#EF4444'}">${fmt(remaining)}</div></div>
        <div class="bh-stat"><div class="bh-stat-lbl">Dana do kraja</div><div class="bh-stat-val" style="color:#3B82F6">${daysLeft}</div></div>
        <div class="bh-stat"><div class="bh-stat-lbl">Dnevni prosjek</div><div class="bh-stat-val">${fmt(dailyAvg)}</div></div>
        <div class="bh-stat"><div class="bh-stat-lbl">Transakcija</div><div class="bh-stat-val">${mCosts.length}</div></div>
      </div>
    </div>
    <div style="display:flex;justify-content:flex-end;gap:8px;margin-top:6px">
      <button class="btn-ghost" onclick="showBudgetEdit(${budget.id},${limit})">✏️ Uredi</button>
      <button class="btn-ghost" onclick="deleteBudget(${budget.id})">🗑 Obriši</button>
    </div>
    <div id="budget-edit-row" style="display:none;margin-top:10px" class="card">
      <div class="card-title" style="margin-bottom:10px">Uredi budžet — ${MONTHS_CAP[m - 1]} ${y}.</div>
      <div style="display:flex;gap:12px;align-items:flex-end;flex-wrap:wrap">
        <div style="flex:1;min-width:150px">
          <label class="modal-label">Novi iznos</label>
          <input class="modal-fi" id="budget-edit-input" type="number" min="1" step="0.01" placeholder="npr. 3000.00">
        </div>
        <button class="btn-sm" onclick="updateBudget(${budget.id})">Spremi</button>
        <button class="btn-ghost" onclick="document.getElementById('budget-edit-row').style.display='none'">Odustani</button>
      </div>
      <div id="budget-edit-msg" style="display:none;margin-top:8px;font-size:12px"></div>
    </div>`;

  if (breakdown) {
    const byType = {};
    mCosts.forEach(c => { byType[c.costTypeId] = (byType[c.costTypeId] || 0) + c.amount; });
    const catRows = Object.entries(byType)
      .map(([id, amt]) => ({ name: (state.costTypes.find(x => x.id === parseInt(id)) || { name: 'Ostalo' }).name, amt }))
      .sort((a, b) => b.amt - a.amt);
    const maxCat = catRows.length ? catRows[0].amt : 1;

    const recentRows = [...mCosts]
      .sort((a, b) => new Date(b.transactionDate) - new Date(a.transactionDate))
      .slice(0, 5)
      .map(c => {
        const cat  = (state.costTypes.find(x => x.id === c.costTypeId) || { name: '—' }).name;
        const date = new Date(c.transactionDate).toLocaleDateString('hr-HR', { day: 'numeric', month: 'short' });
        return `<div style="display:flex;align-items:center;gap:9px;padding:8px 0;border-bottom:1px solid #0a1628">
          <div style="width:30px;height:30px;border-radius:7px;background:#3a0f0f;display:flex;align-items:center;justify-content:center;font-size:13px;flex-shrink:0">💸</div>
          <div style="flex:1">
            <div style="font-size:12px;font-weight:500;color:#fff">${c.notes || cat}</div>
            <div style="font-size:10px;color:#64748B">${cat}</div>
          </div>
          <div style="text-align:right;flex-shrink:0">
            <div style="font-size:12px;font-weight:700;color:#EF4444">-${fmt(c.amount)}</div>
            <div style="font-size:10px;color:#64748B">${date}</div>
          </div>
        </div>`;
      }).join('');

    breakdown.innerHTML = `
      <div class="card">
        <div class="card-title">Troškovi po kategoriji</div>
        <div class="card-sub">Ovaj mj.</div>
        ${catRows.length ? catRows.slice(0, 5).map((e, i) => `
          <div class="hbar-row">
            <div class="hbar-lbl">${e.name}</div>
            <div class="hbar-track"><div class="hbar-fill" style="width:${Math.max(8, Math.round(e.amt / maxCat * 100))}%;background:${COLORS[i % COLORS.length]}">${Math.round(e.amt / spent * 100)}%</div></div>
            <div class="hbar-amt">${fmt(e.amt)}</div>
          </div>`).join('') : '<div style="color:#64748B;font-size:12px;padding:12px 0">Nema troškova.</div>'}
      </div>
      <div class="card">
        <div class="card-title">Nedavni troškovi</div>
        <div class="card-sub">Zadnjih 5 ovaj mj.</div>
        ${recentRows || '<div style="color:#64748B;font-size:12px;padding:12px 0">Nema troškova.</div>'}
      </div>`;
  }
}

async function createBudget() {
  const amtInput = document.getElementById('budget-amount-input');
  const amount   = parseFloat(amtInput ? amtInput.value : '');
  const msgEl    = document.getElementById('budget-create-msg');
  if (!amount || amount <= 0) { _budgetMsg('Unesite ispravan iznos.', true, msgEl); return; }
  if (!state.personalUserGroupId) { _budgetMsg('Greška: nema korisničke grupe.', true, msgEl); return; }
  const m = _budgetMonth(), y = _budgetYear();
  try {
    const b = await api('POST', '/Budget/CreateBudget/' + state.personalUserGroupId, {
      amount, year: y, month: m, currency: 0,
    });
    if (b) state.budgets.push(b);
    renderBudget();
    checkBudgetAlerts();
  } catch (e) { _budgetMsg('Greška: ' + friendlyErr(e), true, msgEl); }
}

async function deleteBudget(id) {
  if (!confirm('Obrisati budžet?')) return;
  try {
    await api('DELETE', '/Budget/DeleteBudget/' + id);
    state.budgets = state.budgets.filter(b => b.id !== id);
    renderBudget();
  } catch (e) {
    alert('Greška pri brisanju budžeta: ' + friendlyErr(e));
  }
}

function showBudgetEdit(id, currentAmount) {
  const row = document.getElementById('budget-edit-row');
  const inp = document.getElementById('budget-edit-input');
  if (!row) return;
  if (inp) inp.value = currentAmount;
  row.style.display = 'block';
  if (inp) inp.focus();
}

async function updateBudget(id) {
  const inp    = document.getElementById('budget-edit-input');
  const msgEl  = document.getElementById('budget-edit-msg');
  const amount = parseFloat(inp ? inp.value : '');
  if (!amount || amount <= 0) { _budgetMsg('Unesite ispravan iznos.', true, msgEl); return; }
  const m = _budgetMonth(), y = _budgetYear();
  try {
    await api('PUT', '/Budget/UpdateBudget/' + id, {
      amount, year: y, month: m, currency: 0,
    });
    const b = state.budgets.find(x => x.id === id);
    if (b) b.amount = amount;
    renderBudget();
    checkBudgetAlerts();
  } catch (e) { _budgetMsg('Greška: ' + friendlyErr(e), true, msgEl); }
}

function _budgetMsg(text, isError, el) {
  if (!el) return;
  el.textContent   = text;
  el.style.display = 'block';
  el.style.color   = isError ? '#EF4444' : '#22C55E';
}
