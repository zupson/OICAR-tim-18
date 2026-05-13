const AVATAR_COLORS = ['#3B82F6','#22C55E','#F59E0B','#8B5CF6','#06B6D4','#EF4444','#F97316','#EC4899'];

async function renderFamily() {
  if (!state.user) return;
  const u         = state.user;
  const groupName = state.userGroups.length > 0 ? state.userGroups[0].groupName : 'Vaša grupa';
  const avatar    = u.firstName.charAt(0).toUpperCase();
  const now       = new Date();
  const m = now.getMonth() + 1, y = now.getFullYear();

  setEl('family-page-sub', groupName + ' — ' + MONTHS_CAP[m - 1] + ' ' + y + '.');

  const mCostAmt = state.costs
    .filter(c => { const d = new Date(c.transactionDate); return d.getMonth() + 1 === m && d.getFullYear() === y; })
    .reduce((s, c) => s + c.amount, 0);

  const membersEl = document.getElementById('family-members');
  if (membersEl) {
    // Try the GetMembersByGroup endpoint (available once backend colleague adds it)
    let allMembers = null;
    if (state.personalGroupId) {
      try {
        allMembers = await api('GET', '/UserGroup/GetMembersByGroup/' + state.personalGroupId);
      } catch (_) { allMembers = null; }
    }

    if (allMembers && Array.isArray(allMembers) && allMembers.length > 0) {
      membersEl.innerHTML = allMembers.map((mem, i) => {
        const av    = (mem.username || mem.firstName || '?').charAt(0).toUpperCase();
        const col   = AVATAR_COLORS[i % AVATAR_COLORS.length];
        const name  = (mem.firstName && mem.lastName) ? `${mem.firstName} ${mem.lastName}` : mem.username;
        const isMe  = mem.userId === u.id || mem.username === u.username;
        return `
          <div class="member-row">
            <div class="m-avatar" style="background:${col}">${av}</div>
            <div class="m-info">
              <div class="m-name">${name}${isMe ? ' <span style="font-size:10px;color:#64748B">(Vi)</span>' : ''}</div>
              <div class="m-email">${mem.email || mem.username || ''}</div>
            </div>
            ${isMe ? '<span class="role-badge rb-owner">Vlasnik</span>' : '<span class="role-badge" style="background:#1e3a5f;color:#94a3b8">Član</span>'}
            ${isMe ? `<div class="m-spend"><div class="m-spend-val">${fmt(mCostAmt)}</div><div class="m-spend-lbl">ovaj mj.</div></div>` : '<div class="m-spend"></div>'}
          </div>`;
      }).join('');
    } else {
      // Fallback: current user only + info about missing endpoint
      membersEl.innerHTML = `
        <div class="member-row">
          <div class="m-avatar" style="background:#3B82F6">${avatar}</div>
          <div class="m-info">
            <div class="m-name">${u.firstName} ${u.lastName}</div>
            <div class="m-email">${u.email || u.username}</div>
          </div>
          <span class="role-badge rb-owner">Vlasnik</span>
          <div class="m-spend">
            <div class="m-spend-val">${fmt(mCostAmt)}</div>
            <div class="m-spend-lbl">ovaj mj.</div>
          </div>
        </div>
        <div style="margin-top:10px;font-size:11px;color:#64748B">API needed: GET /api/UserGroup/GetMembersByGroup/{groupId}</div>`;
    }
  }

  const splitsEl = document.getElementById('family-splits');
  if (splitsEl) {
    splitsEl.innerHTML = `
      <div class="split-row">
        <div class="sp-av" style="background:#3B82F6">${avatar}</div>
        <div class="sp-bar-track"><div class="sp-bar-fill" style="width:100%;background:#3B82F6">${u.firstName}</div></div>
        <div class="sp-amt">${fmt(mCostAmt)}</div>
      </div>`;
  }
}

async function claimInvitation() {
  const tokenEl = document.getElementById('family-claim-token');
  const token   = tokenEl ? tokenEl.value.trim() : '';
  showMsg('family-claim-msg', '', false);
  if (!token) { showMsg('family-claim-msg', 'Unesite token pozivnice.', true); return; }
  try {
    const ug = await api('POST', '/Invitation/ClaimInvitation?token=' + encodeURIComponent(token));
    showMsg('family-claim-msg', '✓ Uspješno ste se pridružili grupi: ' + (ug?.groupName || ''), false);
    if (tokenEl) tokenEl.value = '';
    const groups = await api('GET', '/UserGroup/GetAllUserGroups');
    state.userGroups = groups || [];
    if (groups && groups.length > 0) {
      state.personalGroupId     = groups[0].groupId;
      state.personalUserGroupId = groups[0].id;
    }
    renderFamily();
  } catch (e) {
    showMsg('family-claim-msg', 'Greška: ' + friendlyErr(e), true);
  }
}

async function sendInvitation() {
  const emailEl = document.getElementById('family-invite-email');
  const email   = emailEl ? emailEl.value.trim() : '';
  showMsg('family-invite-msg', '', false);
  if (!email) { showMsg('family-invite-msg', 'Unesite e-mail adresu.', true); return; }
  if (!state.personalGroupId) { showMsg('family-invite-msg', 'Greška: nema grupe.', true); return; }
  try {
    await api('POST', '/Invitation/CreateNewInvitation/' + state.personalGroupId, { email });
    showMsg('family-invite-msg', '✓ Pozivnica je poslana na ' + email, false);
    if (emailEl) emailEl.value = '';
  } catch (e) {
    showMsg('family-invite-msg', 'Greška: ' + friendlyErr(e), true);
  }
}
