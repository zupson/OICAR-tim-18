// Pure, framework-free notification detection logic.
// Kept separate from NotificationService so it can be unit-tested without
// Angular, localStorage, or Date.now().

export interface SimpleNotification {
  id: string;
  type: string;
  title: string;
  desc: string;
  timestamp: string;
  read: boolean;
}

export interface CostLike {
  id: number;
  amount: number;
  notes?: string;
  costTypeName?: string;
  userId?: number;
}

export interface MemberLike {
  userId: number;
  username?: string;
}

/**
 * Decide which "new shared cost" notifications to raise.
 *
 *  - seenIds === null  -> first run for this account: seed silently, raise nothing.
 *  - enabled === false -> still advance the seen-list, but raise nothing. This is
 *    what makes a toggle behave correctly: turning it ON later never floods you
 *    with costs that were added while it was OFF.
 *  - otherwise         -> one notification per not-yet-seen cost recorded by
 *    ANOTHER family member (never your own).
 */
export function processSharedCosts(input: {
  costs: CostLike[];
  seenIds: number[] | null;
  enabled: boolean;
  members: MemberLike[];
  myUserId: number;
  timestamp: string;
  fmt: (amount: number) => string;
}): { notifications: SimpleNotification[]; nextSeen: number[] } {
  const nextSeen = input.costs.map(c => c.id);
  if (input.seenIds === null || !input.enabled) return { notifications: [], nextSeen };

  const seen = new Set(input.seenIds);
  const nameOf = (uid?: number) =>
    input.members.find(m => m.userId === uid)?.username || 'Član obitelji';

  const notifications = input.costs
    .filter(c => !seen.has(c.id) && c.userId != null && c.userId !== input.myUserId)
    .map(c => ({
      id: `cost-${c.id}`,
      type: 'shared_cost',
      title: 'Novi zajednički trošak',
      desc: `${nameOf(c.userId)} je dodao/la: ${c.notes || c.costTypeName || 'trošak'} • ${input.fmt(c.amount)}`,
      timestamp: input.timestamp,
      read: false,
    }));

  return { notifications, nextSeen };
}

/**
 * Decide which "member activity" notifications to raise (joins and leaves).
 * Same seed/enabled semantics as processSharedCosts. Your own join/leave is
 * never reported. A leaver's name is taken from the previous (seen) list, since
 * they are already gone from the live one.
 */
export function processMemberActivity(input: {
  members: MemberLike[];
  seen: MemberLike[] | null;
  enabled: boolean;
  myUserId: number;
  timestamp: string;
}): { notifications: SimpleNotification[]; nextSeen: MemberLike[] } {
  const current: MemberLike[] = input.members
    .filter(m => m.userId != null)
    .map(m => ({ userId: m.userId, username: m.username ?? '' }));
  const nextSeen = current;
  if (input.seen === null || !input.enabled) return { notifications: [], nextSeen };

  const seenIds = new Set(input.seen.map(m => m.userId));
  const currentIds = new Set(current.map(m => m.userId));
  const notifications: SimpleNotification[] = [];

  for (const m of current) {
    if (!seenIds.has(m.userId) && m.userId !== input.myUserId) {
      notifications.push({
        id: `member-join-${m.userId}`,
        type: 'member_join',
        title: 'Novi član',
        desc: `${m.username || 'Novi član'} se pridružio/la grupi`,
        timestamp: input.timestamp,
        read: false,
      });
    }
  }
  for (const m of input.seen) {
    if (!currentIds.has(m.userId) && m.userId !== input.myUserId) {
      notifications.push({
        id: `member-leave-${m.userId}`,
        type: 'member_leave',
        title: 'Član otišao',
        desc: `${m.username || 'Član'} više nije u grupi`,
        timestamp: input.timestamp,
        read: false,
      });
    }
  }
  return { notifications, nextSeen };
}
