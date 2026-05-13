// ── Helpers ───────────────────────────────────────────────────────────────────

function allTxnSorted() {
  return [
    ...state.costs.map(c => ({ ...c, _type: 'cost' })),
    ...state.revenues.map(r => ({ ...r, _type: 'revenue' })),
  ].sort((a, b) => new Date(b.transactionDate) - new Date(a.transactionDate));
}

function resetTxnDropdowns() {
  const monthSel = document.getElementById('txn-month-filter');
  const catSel   = document.getElementById('txn-cat-filter');
  if (monthSel) { while (monthSel.options.length > 1) monthSel.remove(1); }
  if (catSel)   { while (catSel.options.length   > 1) catSel.remove(1); }
}

// ── Render ────────────────────────────────────────────────────────────────────

function renderTransactions() {
  const monthSel = document.getElementById('txn-month-filter');
  const catSel   = document.getElementById('txn-cat-filter');

  // Populate month dropdown (idempotent)
  if (monthSel && monthSel.options.length <= 1) {
    const seen = new Set();
    allTxnSorted().forEach(t => {
      const d   = new Date(t.transactionDate);
      const key = d.getFullYear() + '-' + (d.getMonth() + 1);
      if (!seen.has(key)) {
        seen.add(key);
        const opt = document.createElement('option');
        opt.value       = key;
        opt.textContent = '📅 ' + MONTHS_CAP[d.getMonth()] + ' ' + d.getFullYear() + '.';
        monthSel.appendChild(opt);
      }
    });
  }

  // Populate category dropdown (idempotent)
  if (catSel && catSel.options.length <= 1) {
    state.costTypes.forEach(ct => {
      const opt = document.createElement('option');
      opt.value       = 'cost-' + ct.id;
      opt.textContent = '💸 ' + ct.name;
      catSel.appendChild(opt);
    });
    state.revenueTypes.forEach(rt => {
      const opt = document.createElement('option');
      opt.value       = 'rev-' + rt.id;
      opt.textContent = '💰 ' + rt.name;
      catSel.appendChild(opt);
    });
  }

  // Apply filters
  let all = allTxnSorted();
  if (state.txnFilter === 'inc') all = all.filter(t => t._type === 'revenue');
  if (state.txnFilter === 'exp') all = all.filter(t => t._type === 'cost');

  const search = (document.getElementById('txn-search')?.value || '').toLowerCase().trim();
  if (search) {
    all = all.filter(t => {
      const cat = t._type === 'cost' ? (t.costTypeName || '') : (t.revenueTypeName || '');
      return (t.notes || '').toLowerCase().includes(search) || cat.toLowerCase().includes(search);
    });
  }

  const monthVal = document.getElementById('txn-month-filter')?.value;
  if (monthVal) {
    const [fy, fm] = monthVal.split('-').map(Number);
    all = all.filter(t => {
      const d = new Date(t.transactionDate);
      return d.getFullYear() === fy && d.getMonth() + 1 === fm;
    });
  }

  const catVal = document.getElementById('txn-cat-filter')?.value;
  if (catVal) {
    const [kind, idStr] = catVal.split('-');
    const catId = parseInt(idStr);
    if (kind === 'cost') all = all.filter(t => t._type === 'cost'    && t.costTypeId    === catId);
    if (kind === 'rev')  all = all.filter(t => t._type === 'revenue' && t.revenueTypeId === catId);
  }

  const tbody = document.getElementById('txn-tbody');
  if (tbody) tbody.innerHTML = all.length
    ? all.map(t => txnRowFull(t)).join('')
    : '<tr><td colspan="6" style="text-align:center;color:#64748B;padding:20px">Nema transakcija.</td></tr>';
  setEl('txn-count-info', 'Prikazano ' + all.length + ' transakcija');
}

function txnRowSimple(t) {
  const isCost = t._type === 'cost';
  const cat  = isCost ? (t.costTypeName || 'Trošak') : (t.revenueTypeName || 'Prihod');
  const amt  = (isCost ? '-' : '+') + fmt(t.amount);
  const col  = isCost ? '#EF4444' : '#22C55E';
  const bg   = isCost ? '#3a0f0f' : '#0a3320';
  const icon = isCost ? '💸' : '💰';
  const date = new Date(t.transactionDate).toLocaleDateString('hr-HR', { day: 'numeric', month: 'short' });
  return `<tr onclick="showPage('transactions')">
    <td><div class="txn-name-cell"><div class="txn-icon-wrap" style="background:${bg}">${icon}</div><span>${t.notes || cat}</span></div></td>
    <td><span class="badge badge-cat">${cat}</span></td>
    <td style="color:#64748B;font-size:12px">${date}</td>
    <td style="color:${col};font-weight:700">${amt}</td>
  </tr>`;
}

function txnRowFull(t) {
  const isCost   = t._type === 'cost';
  const cat      = isCost ? (t.costTypeName || 'Trošak') : (t.revenueTypeName || 'Prihod');
  const amt      = (isCost ? '-' : '+') + fmt(t.amount);
  const col      = isCost ? '#EF4444' : '#22C55E';
  const bg       = isCost ? '#3a0f0f' : '#0a3320';
  const icon     = isCost ? '💸' : '💰';
  const badgeCls = isCost ? 'badge-exp' : 'badge-inc';
  const badgeTxt = isCost ? 'Trošak' : 'Prihod';
  const date     = new Date(t.transactionDate).toLocaleDateString('hr-HR', { day: 'numeric', month: 'short', year: 'numeric' });
  return `<tr>
    <td style="padding-left:14px">
      <div class="txn-name-cell">
        <div class="txn-icon-wrap" style="background:${bg}">${icon}</div>
        <span>${t.notes || cat}</span>
      </div>
    </td>
    <td><span class="badge badge-cat">${cat}</span></td>
    <td style="color:#64748B;font-size:12px">${date}</td>
    <td><span class="badge ${badgeCls}">${badgeTxt}</span></td>
    <td style="text-align:right;color:${col};font-weight:700">${amt}</td>
    <td style="text-align:center">
      <button class="act-btn" onclick="openEditTxnModal('${t._type}',${t.id})" title="Uredi">✏️</button>
      <button class="act-btn" onclick="deleteTxn('${t._type}',${t.id})" title="Obriši">🗑</button>
    </td>
  </tr>`;
}

async function deleteTxn(type, id) {
  if (!confirm('Obrisati ovu transakciju?')) return;
  try {
    if (type === 'cost') {
      await api('DELETE', '/Cost/' + id);
      state.costs = state.costs.filter(c => c.id !== id);
    } else {
      await api('DELETE', '/Revenue/DeleteRevenue/' + id);
      state.revenues = state.revenues.filter(r => r.id !== id);
    }
    resetTxnDropdowns();
    renderDashboard();
    renderTransactions();
    pruneBudgetAlerts();
    checkBudgetAlerts();
  } catch (e) { alert('Greška pri brisanju: ' + e.message); }
}

// ── Transaction modal ─────────────────────────────────────────────────────────

function openModal() {
  state.editingTxnId   = null;
  state.editingTxnType = null;
  document.getElementById('modal-title').textContent     = 'Dodaj transakciju';
  document.getElementById('modal-amt').value             = '';
  document.getElementById('modal-date').value            = new Date().toISOString().split('T')[0];
  document.getElementById('modal-notes').value           = '';
  showMsg('modal-error', '', false);
  setModalType('exp');
  updateModalCategories();
  document.getElementById('modal').style.display = 'flex';
}

function openEditTxnModal(type, id) {
  const txn = type === 'cost'
    ? state.costs.find(c => c.id === id)
    : state.revenues.find(r => r.id === id);
  if (!txn) return;
  state.editingTxnId   = id;
  state.editingTxnType = type;
  document.getElementById('modal-title').textContent    = 'Uredi transakciju';
  setModalType(type === 'cost' ? 'exp' : 'inc');
  // disable type toggle while editing
  document.getElementById('tt-exp').style.pointerEvents = 'none';
  document.getElementById('tt-inc').style.pointerEvents = 'none';
  document.getElementById('modal-amt').value   = txn.amount;
  document.getElementById('modal-date').value  = new Date(txn.transactionDate).toISOString().split('T')[0];
  document.getElementById('modal-notes').value = txn.notes || '';
  updateModalCategories();
  const sel = document.getElementById('modal-category');
  if (sel) sel.value = type === 'cost' ? txn.costTypeId : txn.revenueTypeId;
  showMsg('modal-error', '', false);
  document.getElementById('modal').style.display = 'flex';
}

function closeModal() {
  document.getElementById('modal').style.display         = 'none';
  document.getElementById('tt-exp').style.pointerEvents  = '';
  document.getElementById('tt-inc').style.pointerEvents  = '';
  state.editingTxnId   = null;
  state.editingTxnType = null;
}

function closeModalOutside(e) {
  if (e.target === document.getElementById('modal')) closeModal();
}

function setModalType(type) {
  state.modalType = type;
  const expBtn  = document.getElementById('tt-exp');
  const incBtn  = document.getElementById('tt-inc');
  const amt     = document.getElementById('modal-amt');
  const saveBtn = document.getElementById('modal-save-btn');
  if (type === 'exp') {
    expBtn.className = 'tt-btn active-exp'; incBtn.className = 'tt-btn';
    amt.style.color  = '#EF4444';
    saveBtn.className   = 'modal-save exp';
    saveBtn.textContent = 'Spremi trošak';
  } else {
    incBtn.className = 'tt-btn active-inc'; expBtn.className = 'tt-btn';
    amt.style.color  = '#22C55E';
    saveBtn.className   = 'modal-save inc';
    saveBtn.textContent = 'Spremi prihod';
  }
  updateModalCategories();
}

function updateModalCategories() {
  const sel = document.getElementById('modal-category');
  if (!sel) return;
  const types = state.modalType === 'exp' ? state.costTypes : state.revenueTypes;
  sel.innerHTML = types.length
    ? types.map(t => `<option value="${t.id}">${t.name}</option>`).join('')
    : '<option value="">— Nema kategorija, dodajte ih prvo —</option>';
}

async function saveTransaction() {
  const amount = parseFloat(document.getElementById('modal-amt').value);
  const date   = document.getElementById('modal-date').value;
  const notes  = document.getElementById('modal-notes').value.trim();
  const catId  = parseInt(document.getElementById('modal-category').value);
  showMsg('modal-error', '', false);
  if (!amount || amount <= 0) { showMsg('modal-error', 'Unesite ispravan iznos.', true); return; }
  if (!date)                  { showMsg('modal-error', 'Odaberite datum.', true); return; }
  if (!catId)                 { showMsg('modal-error', 'Odaberite kategoriju.', true); return; }
  const body = { amount, transactionDate: new Date(date).toISOString(), notes: notes || null, currency: 0 };

  try {
    if (state.editingTxnId !== null) {
      // Edit mode
      if (state.editingTxnType === 'cost') {
        await api('PUT', '/Cost/EditCostType/' + state.editingTxnId, { ...body, costTypeId: catId });
        const idx = state.costs.findIndex(c => c.id === state.editingTxnId);
        if (idx !== -1) {
          const ct = state.costTypes.find(x => x.id === catId);
          state.costs[idx] = { ...state.costs[idx], amount, transactionDate: body.transactionDate,
            notes: body.notes, costTypeId: catId, costTypeName: ct ? ct.name : '' };
        }
      } else {
        await api('PUT', '/Revenue/EditRevenueType/' + state.editingTxnId, { ...body, revenueTypeId: catId });
        const idx = state.revenues.findIndex(r => r.id === state.editingTxnId);
        if (idx !== -1) {
          const rt = state.revenueTypes.find(x => x.id === catId);
          state.revenues[idx] = { ...state.revenues[idx], amount, transactionDate: body.transactionDate,
            notes: body.notes, revenueTypeId: catId, revenueTypeName: rt ? rt.name : '' };
        }
      }
    } else {
      // Create mode
      if (!state.personalGroupId) {
        showMsg('modal-error', 'Greška: nema grupe. Pokušajte se odjaviti i prijaviti.', true);
        return;
      }
      if (state.modalType === 'exp') {
        const c = await api('POST', '/Cost/CreateNewCost/' + state.personalGroupId, { ...body, costTypeId: catId });
        if (c) state.costs.unshift(c);
        else state.costs = (await api('GET', '/Cost/GetAllCosts')) || [];
      } else {
        const r = await api('POST', '/Revenue/CreateNewRevenue/' + state.personalGroupId, { ...body, revenueTypeId: catId });
        if (r) state.revenues.unshift(r);
        else state.revenues = (await api('GET', '/Revenue/GetAllRevenues')) || [];
      }
    }
    closeModal();
    resetTxnDropdowns();
    renderDashboard();
    renderTransactions();
    pruneBudgetAlerts();
    checkBudgetAlerts();
  } catch (e) { showMsg('modal-error', 'Greška pri spremanju: ' + e.message, true); }
}
