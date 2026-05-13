const MONTHS     = ['siječanj','veljača','ožujak','travanj','svibanj','lipanj','srpanj','kolovoz','rujan','listopad','studeni','prosinac'];
const MONTHS_CAP = ['Siječanj','Veljača','Ožujak','Travanj','Svibanj','Lipanj','Srpanj','Kolovoz','Rujan','Listopad','Studeni','Prosinac'];
const MONTHS_S   = ['Sij','Velj','Ožu','Tra','Svi','Lip','Srp','Kol','Ruj','Lis','Stu','Pro'];
const COLORS     = ['#EF4444','#F59E0B','#3B82F6','#22C55E','#8B5CF6','#06B6D4','#F97316','#EC4899'];

function renderDashboard() {
  const now = new Date();
  const m = state.dashMonth !== null ? state.dashMonth : now.getMonth() + 1;
  const y = state.dashYear  !== null ? state.dashYear  : now.getFullYear();
  const inMonth = arr => arr.filter(t => {
    const d = new Date(t.transactionDate);
    return d.getMonth() + 1 === m && d.getFullYear() === y;
  });
  const mCosts = inMonth(state.costs);
  const mRevs  = inMonth(state.revenues);
  const totalInc  = mRevs.reduce((s, r) => s + r.amount, 0);
  const totalCost = mCosts.reduce((s, c) => s + c.amount, 0);
  const balance   = totalInc - totalCost;

  if (state.user) setEl('dashboard-greeting', 'Dobro jutro, ' + state.user.firstName + ' 👋');
  setEl('dashboard-sub',       'Financijski pregled za ' + MONTHS[m - 1] + ' ' + y + '.');
  setEl('month-picker-label',  MONTHS_CAP[m - 1] + ' ' + y + '.');
  setEl('stat-income',         fmt(totalInc));
  setEl('stat-costs',          fmt(totalCost));
  setEl('stat-balance',        fmt(balance));
  setEl('stat-txn-count',      String(mCosts.length + mRevs.length));

  renderBarChart();
  renderDonutChart();

  const all   = allTxnSorted().slice(0, 5);
  const tbody = document.getElementById('dashboard-txn-tbody');
  if (tbody) tbody.innerHTML = all.length
    ? all.map(t => txnRowSimple(t)).join('')
    : '<tr><td colspan="4" style="text-align:center;color:#64748B;padding:20px">Nema transakcija ovaj mjesec.</td></tr>';
}

function renderBarChart() {
  const bars = document.getElementById('chart-bars-monthly');
  if (!bars) return;
  const now  = new Date();
  const selM = state.dashMonth !== null ? state.dashMonth : now.getMonth() + 1;
  const selY = state.dashYear  !== null ? state.dashYear  : now.getFullYear();
  const last6 = [];
  for (let i = 5; i >= 0; i--) {
    const d = new Date(selY, selM - 1 - i, 1);
    last6.push({ y: d.getFullYear(), m: d.getMonth() + 1, lbl: MONTHS_S[d.getMonth()] });
  }
  const sum = (arr, y, m) => arr
    .filter(t => { const d = new Date(t.transactionDate); return d.getFullYear() === y && d.getMonth() + 1 === m; })
    .reduce((s, t) => s + t.amount, 0);
  const maxVal = Math.max(...last6.flatMap(mo => [sum(state.revenues, mo.y, mo.m), sum(state.costs, mo.y, mo.m)]), 1);
  const scale  = v => Math.max(4, Math.round((v / maxVal) * 90));
  bars.innerHTML = last6.map((mo, i) => {
    const inc = sum(state.revenues, mo.y, mo.m);
    const exp = sum(state.costs,    mo.y, mo.m);
    const cur = i === 5;
    return `<div class="chart-col">
      <div class="chart-pair">
        <div class="b-inc" style="height:${scale(inc)}px${cur ? ';box-shadow:0 0 7px #22C55E44' : ''}"></div>
        <div class="b-exp" style="height:${scale(exp)}px${cur ? ';box-shadow:0 0 7px #EF444444' : ''}"></div>
      </div>
      <div class="chart-lbl" style="${cur ? 'color:#3B82F6;font-weight:700' : ''}">${mo.lbl}</div>
    </div>`;
  }).join('');
}

function renderDonutChart() {
  const donutEl  = document.getElementById('donut-chart');
  const legendEl = document.getElementById('donut-legend');
  if (!donutEl || !legendEl) return;
  const now = new Date();
  const m = state.dashMonth !== null ? state.dashMonth : now.getMonth() + 1;
  const y = state.dashYear  !== null ? state.dashYear  : now.getFullYear();
  setEl('donut-sub', MONTHS_CAP[m - 1] + ' ' + y + '.');

  const monthCosts = state.costs.filter(c => {
    const d = new Date(c.transactionDate);
    return d.getMonth() + 1 === m && d.getFullYear() === y;
  });
  const total = monthCosts.reduce((s, c) => s + c.amount, 0);

  if (total === 0) {
    donutEl.style.background = '#1e3a5f';
    legendEl.innerHTML = '<div style="font-size:11px;color:#64748B">Nema troškova ovaj mj.</div>';
    return;
  }

  const byType = {};
  monthCosts.forEach(c => { byType[c.costTypeId] = (byType[c.costTypeId] || 0) + c.amount; });
  const entries = Object.entries(byType)
    .map(([id, amt]) => {
      const ct = state.costTypes.find(x => x.id === parseInt(id));
      return { name: ct ? ct.name : 'Ostalo', pct: Math.round(amt / total * 100) };
    })
    .sort((a, b) => b.pct - a.pct)
    .slice(0, 5);

  let deg = 0;
  const grad = entries.map((e, i) => {
    const d = Math.round(e.pct / 100 * 360);
    const s = `${COLORS[i % COLORS.length]} ${deg}deg ${deg + d}deg`;
    deg += d;
    return s;
  });
  if (deg < 360) grad.push(`#1e3a5f ${deg}deg 360deg`);
  donutEl.style.background = `conic-gradient(${grad.join(',')})`;

  legendEl.innerHTML = entries.map((e, i) =>
    `<div class="dl-item">
      <div class="dl-dot" style="background:${COLORS[i % COLORS.length]}"></div>
      <span class="dl-name">${e.name}</span>
      <span class="dl-pct">${e.pct}%</span>
    </div>`
  ).join('');
}

function changeMonth(delta) {
  const now = new Date();
  let m = state.dashMonth !== null ? state.dashMonth : now.getMonth() + 1;
  let y = state.dashYear  !== null ? state.dashYear  : now.getFullYear();
  m += delta;
  if (m > 12) { m = 1; y++; }
  if (m < 1)  { m = 12; y--; }
  state.dashMonth = m;
  state.dashYear  = y;
  renderDashboard();
}
