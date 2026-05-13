function renderCategories() {
  const grid = document.getElementById('cat-grid');
  if (!grid) return;

  const CAT_COLORS = ['#EF4444','#F59E0B','#3B82F6','#22C55E','#8B5CF6','#06B6D4','#F97316','#EC4899'];
  const CAT_BGS    = ['#3a1200','#3a2200','#0a1f40','#0a3320','#1a0a3a','#0a2a2a','#2a1a00','#2a0a2a'];

  const cTotals = {}, rTotals = {};
  state.costs.forEach(c    => { cTotals[c.costTypeId]    = (cTotals[c.costTypeId]    || 0) + c.amount; });
  state.revenues.forEach(r => { rTotals[r.revenueTypeId] = (rTotals[r.revenueTypeId] || 0) + r.amount; });

  const cards = [
    ...state.costTypes.map((ct, i) => ({
      ...ct, kind: 'cost',
      color: CAT_COLORS[i % CAT_COLORS.length],
      bg: CAT_BGS[i % CAT_BGS.length],
      icon: '💸',
    })),
    ...state.revenueTypes.map((rt) => ({
      ...rt, kind: 'revenue',
      color: '#22C55E', bg: '#0a3320', icon: '💰',
    })),
  ].map(t => {
    const total = t.kind === 'cost' ? (cTotals[t.id] || 0) : (rTotals[t.id] || 0);
    const cnt   = t.kind === 'cost'
      ? state.costs.filter(c    => c.costTypeId    === t.id).length
      : state.revenues.filter(r => r.revenueTypeId === t.id).length;
    const sign = t.kind === 'cost' ? '-' : '+';
    const col  = t.kind === 'cost' ? '#EF4444' : '#22C55E';
    const pct  = total > 0 ? Math.min(100, 20 + Math.round(total / 50)) : 10;
    return `<div class="cat-card">
      <div class="cat-icon-wrap" style="background:${t.bg}">${t.icon}</div>
      <div class="cat-info">
        <div class="cat-name">${t.name}</div>
        <div class="cat-cnt">${cnt} transakcija · <span style="font-size:10px;color:#64748B">${t.kind === 'cost' ? 'Trošak' : 'Prihod'}</span></div>
        <div class="cat-amt" style="color:${col}">${sign}${fmt(total)}</div>
        <div class="cat-bar" style="background:linear-gradient(90deg,${t.color},${t.color}44);width:${pct}%"></div>
      </div>
      <div class="cat-acts">
        <button class="cat-act" onclick="openEditCatModal('${t.kind}',${t.id},'${t.name.replace(/'/g, "\\'")}')" title="Preimenuj">✏️</button>
        <button class="cat-act" onclick="deleteCat('${t.kind}',${t.id})" title="Obriši">🗑</button>
      </div>
    </div>`;
  });

  cards.push(`<div class="cat-add" onclick="openCatModal()"><span style="font-size:20px">＋</span> Dodaj kategoriju</div>`);
  grid.innerHTML = cards.join('');
  const total = state.costTypes.length + state.revenueTypes.length;
  setEl('cat-sub', total + ' kategorija');
}

async function deleteCat(kind, id) {
  if (!confirm('Obrisati ovu kategoriju?')) return;
  try {
    if (kind === 'cost') {
      await api('DELETE', '/CostType/DeleteCostType/' + id);
      state.costTypes = state.costTypes.filter(c => c.id !== id);
    } else {
      await api('DELETE', '/RevenueType/DeleteRevenueType/' + id);
      state.revenueTypes = state.revenueTypes.filter(r => r.id !== id);
    }
    resetTxnDropdowns();
    renderCategories();
    updateModalCategories();
  } catch (e) { alert('Greška: ' + e.message); }
}

// ── Category modal ────────────────────────────────────────────────────────────

function openCatModal() {
  state.editingCatId   = null;
  state.editingCatKind = null;
  document.getElementById('cat-name-input').value          = '';
  document.getElementById('cat-modal-title').textContent   = 'Nova kategorija';
  document.getElementById('cat-type-row').style.display    = 'block';
  document.getElementById('cat-save-btn').textContent      = 'Spremi kategoriju';
  showMsg('cat-modal-error', '', false);
  setCatModalType('exp');
  document.getElementById('cat-modal').style.display = 'flex';
}

function openEditCatModal(kind, id, name) {
  state.editingCatId   = id;
  state.editingCatKind = kind;
  document.getElementById('cat-name-input').value          = name;
  document.getElementById('cat-modal-title').textContent   = 'Preimenuj kategoriju';
  document.getElementById('cat-type-row').style.display    = 'none';
  document.getElementById('cat-save-btn').textContent      = 'Spremi';
  showMsg('cat-modal-error', '', false);
  document.getElementById('cat-modal').style.display = 'flex';
}

function closeCatModal() {
  state.editingCatId   = null;
  state.editingCatKind = null;
  document.getElementById('cat-type-row').style.display = 'block';
  document.getElementById('cat-modal').style.display    = 'none';
}

function closeCatModalOutside(e) {
  if (e.target === document.getElementById('cat-modal')) closeCatModal();
}

function setCatModalType(type) {
  state.catModalType = type;
  document.getElementById('cat-type-exp').classList.toggle('active', type === 'exp');
  document.getElementById('cat-type-inc').classList.toggle('active', type === 'inc');
}

async function saveCategory() {
  const name = document.getElementById('cat-name-input').value.trim();
  if (!name) { showMsg('cat-modal-error', 'Unesite naziv kategorije.', true); return; }
  try {
    if (state.editingCatId !== null) {
      // Edit mode
      if (state.editingCatKind === 'cost') {
        await api('PUT', '/CostType/EditCostType/' + state.editingCatId, { name });
        const ct = state.costTypes.find(c => c.id === state.editingCatId);
        if (ct) ct.name = name;
      } else {
        await api('PUT', '/RevenueType/EditRevenueType/' + state.editingCatId, { name });
        const rt = state.revenueTypes.find(r => r.id === state.editingCatId);
        if (rt) rt.name = name;
      }
    } else {
      // Create mode
      if (state.catModalType === 'exp') {
        const ct = await api('POST', '/CostType/CreateNewCostType', { name });
        if (ct) state.costTypes.push(ct);
      } else {
        const rt = await api('POST', '/RevenueType/CreateNewRevenueType', { name });
        if (rt) state.revenueTypes.push(rt);
      }
    }
    closeCatModal();
    resetTxnDropdowns();
    renderCategories();
    updateModalCategories();
  } catch (e) { showMsg('cat-modal-error', 'Greška: ' + e.message, true); }
}
