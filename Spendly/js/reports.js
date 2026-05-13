function renderReports() {
  const now = new Date();

  if (state.reportMonth === null) {
    state.reportMonth = now.getMonth() + 1;
    state.reportYear  = now.getFullYear();
  }
  const m = state.reportMonth, y = state.reportYear;

  // Month tabs — last 6 relative to TODAY
  const tabsEl = document.getElementById('rep-month-tabs');
  if (tabsEl) {
    let html = '';
    for (let i = 5; i >= 0; i--) {
      const d  = new Date(now.getFullYear(), now.getMonth() - i, 1);
      const tm = d.getMonth() + 1, ty = d.getFullYear();
      const active = tm === m && ty === y ? ' active' : '';
      const lbl = MONTHS_S[d.getMonth()] + (i === 0 ? ' ' + ty + '.' : '');
      html += `<div class="m-tab${active}" onclick="setReportMonth(${tm},${ty})">${lbl}</div>`;
    }
    tabsEl.innerHTML = html;
  }

  const mCosts = state.costs.filter(c    => { const d = new Date(c.transactionDate); return d.getMonth() + 1 === m && d.getFullYear() === y; });
  const mRevs  = state.revenues.filter(r => { const d = new Date(r.transactionDate); return d.getMonth() + 1 === m && d.getFullYear() === y; });

  let pm = m - 1, py = y;
  if (pm < 1) { pm = 12; py--; }
  const pmCosts = state.costs.filter(c    => { const d = new Date(c.transactionDate); return d.getMonth() + 1 === pm && d.getFullYear() === py; });
  const pmRevs  = state.revenues.filter(r => { const d = new Date(r.transactionDate); return d.getMonth() + 1 === pm && d.getFullYear() === py; });

  const totalInc   = mRevs.reduce((s, r) => s + r.amount, 0);
  const totalCost  = mCosts.reduce((s, c) => s + c.amount, 0);
  const netBalance = totalInc - totalCost;
  const prevInc    = pmRevs.reduce((s, r) => s + r.amount, 0);
  const prevCost   = pmCosts.reduce((s, c) => s + c.amount, 0);
  const savingsRate = totalInc > 0 ? Math.round(netBalance / totalInc * 100) : 0;
  const prevName   = MONTHS_S[pm - 1];

  setEl('rep-page-sub', MONTHS_CAP[m - 1] + ' ' + y + ' — Financijski pregled');

  // Summary cards
  const incDiff  = totalInc  - prevInc;
  const costDiff = totalCost - prevCost;
  setEl('rep-sum-grid', `
    <div class="sum-card">
      <div class="sum-label">Ukupni prihodi</div>
      <div class="sum-val" style="color:#22C55E">${fmt(totalInc)}</div>
      <div class="sum-vs">${incDiff >= 0 ? '↑ +' + fmt(incDiff) : '↓ -' + fmt(Math.abs(incDiff))} vs ${prevName}</div>
    </div>
    <div class="sum-card">
      <div class="sum-label">Ukupni troškovi</div>
      <div class="sum-val" style="color:#EF4444">${fmt(totalCost)}</div>
      <div class="sum-vs">${costDiff >= 0 ? '↑ +' + fmt(costDiff) : '↓ -' + fmt(Math.abs(costDiff))} vs ${prevName}</div>
    </div>
    <div class="sum-card">
      <div class="sum-label">Neto stanje</div>
      <div class="sum-val" style="color:${netBalance >= 0 ? '#3B82F6' : '#EF4444'}">${fmt(netBalance)}</div>
      <div class="sum-vs">Stopa štednje: ${savingsRate}%</div>
    </div>`);

  // Category totals
  const byType = {};
  mCosts.forEach(c => { byType[c.costTypeId] = (byType[c.costTypeId] || 0) + c.amount; });
  const catEntries = Object.entries(byType)
    .map(([id, amt]) => {
      const ct = state.costTypes.find(x => x.id === parseInt(id));
      return { id: parseInt(id), name: ct ? ct.name : 'Ostalo', amt };
    })
    .sort((a, b) => b.amt - a.amt);

  // Horizontal bars
  setEl('rep-hbar-sub', 'Raščlamba za ' + MONTHS_CAP[m - 1] + ' ' + y + '.');
  const hbarsEl = document.getElementById('rep-hbars');
  if (hbarsEl) {
    if (!catEntries.length) {
      hbarsEl.innerHTML = '<div style="text-align:center;color:#64748B;padding:20px;font-size:12px">Nema troškova ovaj mj.</div>';
    } else {
      const maxAmt = catEntries[0].amt;
      hbarsEl.innerHTML = catEntries.slice(0, 6).map((e, i) => {
        const pct  = Math.round(e.amt / totalCost * 100);
        const barW = Math.max(8, Math.round(e.amt / maxAmt * 100));
        return `<div class="hbar-row">
          <div class="hbar-lbl">${e.name}</div>
          <div class="hbar-track"><div class="hbar-fill" style="width:${barW}%;background:${COLORS[i % COLORS.length]}">${pct}%</div></div>
          <div class="hbar-amt">${fmt(e.amt)}</div>
        </div>`;
      }).join('');
    }
  }

  // Trend chart (net per month, last 6 ending at selected)
  const trendData = [];
  for (let i = 5; i >= 0; i--) {
    const d   = new Date(y, m - 1 - i, 1);
    const tm  = d.getMonth() + 1, ty = d.getFullYear();
    const tInc  = state.revenues.filter(r => { const rd = new Date(r.transactionDate); return rd.getMonth() + 1 === tm && rd.getFullYear() === ty; }).reduce((s, r) => s + r.amount, 0);
    const tCost = state.costs.filter(c    => { const cd = new Date(c.transactionDate);  return cd.getMonth() + 1 === tm && cd.getFullYear() === ty; }).reduce((s, c) => s + c.amount, 0);
    trendData.push({ net: tInc - tCost, lbl: MONTHS_S[tm - 1] });
  }
  const maxAbs  = Math.max(...trendData.map(t => Math.abs(t.net)), 1);
  const trendEl = document.getElementById('rep-trend');
  if (trendEl) {
    trendEl.innerHTML = trendData.map((t, i) => {
      const h   = Math.max(6, Math.round(Math.abs(t.net) / maxAbs * 88));
      const col = t.net >= 0 ? '#22C55E' : '#EF4444';
      const cur = i === 5;
      return `<div class="tr-col">
        <div class="tr-bar" style="height:${h}px;background:${col}${cur ? ';box-shadow:0 0 8px ' + col + '55' : ''}"></div>
        <div class="tr-lbl" style="${cur ? 'color:#3B82F6;font-weight:700' : ''}">${t.lbl}</div>
      </div>`;
    }).join('');
  }

  const topEl = document.getElementById('rep-trend-top');
  if (topEl) {
    if (catEntries.length) {
      const top    = catEntries[0];
      const topPct = Math.round(top.amt / totalCost * 100);
      topEl.innerHTML = `
        <div style="font-size:11px;color:#64748B;margin-bottom:5px">Vodeća kategorija troškova</div>
        <div style="display:flex;align-items:center;gap:7px">
          <div style="width:26px;height:26px;border-radius:6px;background:#3a1200;display:flex;align-items:center;justify-content:center;font-size:13px">💸</div>
          <div>
            <div style="font-size:12px;font-weight:600">${top.name}</div>
            <div style="font-size:11px;color:#EF4444">${fmt(top.amt)} · ${topPct}% od ukupnog</div>
          </div>
        </div>`;
    } else {
      topEl.innerHTML = '<div style="font-size:11px;color:#64748B">Nema troškova ovaj mj.</div>';
    }
  }

  // Detail table
  const tbodyEl = document.getElementById('rep-table-body');
  if (tbodyEl) {
    if (!catEntries.length) {
      tbodyEl.innerHTML = '<tr><td colspan="4" style="text-align:center;color:#64748B;padding:16px">Nema troškova ovaj mj.</td></tr>';
    } else {
      const rows = catEntries.map((e, i) => {
        const cnt = mCosts.filter(c => c.costTypeId === e.id).length;
        const pct = Math.round(e.amt / totalCost * 100);
        return `<tr>
          <td><span class="dot" style="background:${COLORS[i % COLORS.length]}"></span><span style="color:#fff">${e.name}</span></td>
          <td style="color:#64748B;text-align:right">${cnt}</td>
          <td style="color:#EF4444;text-align:right">${fmt(e.amt)}</td>
          <td style="color:#94a3b8;text-align:right">${pct}%</td>
        </tr>`;
      }).join('');
      const total = `<tr style="background:#0a1628">
        <td style="padding-left:6px;font-weight:700">UKUPNO</td>
        <td style="text-align:right">${mCosts.length}</td>
        <td style="color:#EF4444;text-align:right">${fmt(totalCost)}</td>
        <td style="text-align:right">100%</td>
      </tr>`;
      tbodyEl.innerHTML = rows + total;
    }
  }
}

function setReportMonth(m, y) {
  state.reportMonth = m;
  state.reportYear  = y;
  renderReports();
}
