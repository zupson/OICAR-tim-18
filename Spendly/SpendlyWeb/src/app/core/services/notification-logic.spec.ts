import { describe, it, expect } from 'vitest';
import { processSharedCosts, processMemberActivity } from './notification-logic';
import type { CostLike, MemberLike } from './notification-logic';

// A trivial money formatter so the tests don't depend on locale/currency state.
const fmt = (n: number) => `${n} €`;

describe('processSharedCosts', () => {
  const members: MemberLike[] = [
    { userId: 2, username: 'Ana' },
    { userId: 3, username: 'Marko' },
  ];

  // TEST 1: First run for an account (seenIds === null) must seed silently —
  // remember every cost, but raise no notifications.
  it('seeds silently on the first run and raises nothing', () => {
    const costs: CostLike[] = [
      { id: 1, amount: 5, userId: 2 },
      { id: 2, amount: 7, userId: 3 },
    ];

    const result = processSharedCosts({
      costs,
      seenIds: null, // first run for this account
      enabled: true,
      members,
      myUserId: 1,
      timestamp: '2026-01-01T00:00:00.000Z',
      fmt,
    });

    expect(result.notifications).toHaveLength(0);
    expect(result.nextSeen).toEqual([1, 2]); // every cost is now remembered
  });

  // TEST 2: The core rule — one notification per not-yet-seen cost recorded by
  // ANOTHER family member. Your own costs and already-seen costs are ignored.
  it('notifies once per unseen cost from another member, never your own', () => {
    const costs: CostLike[] = [
      { id: 10, amount: 50, userId: 1, notes: 'Moj trošak' }, // mine -> ignored
      { id: 11, amount: 30, userId: 2, notes: 'Namirnice' }, // Ana, new -> notify
      { id: 12, amount: 20, userId: 3, notes: 'Staro' }, // already seen -> ignored
    ];

    const result = processSharedCosts({
      costs,
      seenIds: [12],
      enabled: true,
      members,
      myUserId: 1,
      timestamp: '2026-01-01T00:00:00.000Z',
      fmt,
    });

    expect(result.notifications).toHaveLength(1);
    const n = result.notifications[0];
    expect(n.id).toBe('cost-11');
    expect(n.type).toBe('shared_cost');
    expect(n.desc).toContain('Ana'); // attributed to the right member
    expect(n.desc).toContain('Namirnice'); // includes the note
    expect(n.desc).toContain('30 €'); // includes the formatted amount
    expect(result.nextSeen).toEqual([10, 11, 12]);
  });

  // TEST 3: While the toggle is OFF we must still advance the seen-list, so that
  // turning it ON later never floods the user with costs added while it was OFF.
  it('advances the seen-list but raises nothing while disabled', () => {
    const costs: CostLike[] = [
      { id: 1, amount: 5, userId: 1 },
      { id: 2, amount: 9, userId: 2, notes: 'Novo' }, // would notify if enabled
    ];

    const result = processSharedCosts({
      costs,
      seenIds: [1], // not a first run
      enabled: false, // notifications turned off
      members,
      myUserId: 1,
      timestamp: '2026-01-01T00:00:00.000Z',
      fmt,
    });

    expect(result.notifications).toHaveLength(0);
    expect(result.nextSeen).toEqual([1, 2]); // cost #2 is now remembered anyway
  });
});

describe('processMemberActivity', () => {
  // TEST 4: A member appearing in the live list but not in the seen list is a
  // join — and you are never notified about yourself joining.
  it('reports a new member joining (but never yourself)', () => {
    const seen: MemberLike[] = [
      { userId: 1, username: 'Ja' },
      { userId: 2, username: 'Ana' },
    ];
    const members: MemberLike[] = [
      { userId: 1, username: 'Ja' },
      { userId: 2, username: 'Ana' },
      { userId: 3, username: 'Marko' }, // newcomer
    ];

    const result = processMemberActivity({
      members,
      seen,
      enabled: true,
      myUserId: 1,
      timestamp: '2026-01-01T00:00:00.000Z',
    });

    expect(result.notifications).toHaveLength(1);
    const n = result.notifications[0];
    expect(n.id).toBe('member-join-3');
    expect(n.type).toBe('member_join');
    expect(n.desc).toContain('Marko');
  });

  // TEST 5: A member in the seen list but gone from the live list is a leave.
  // Their name is recovered from the seen list, since they are already gone.
  it('reports a member leaving, naming them from the previously seen list', () => {
    const seen: MemberLike[] = [
      { userId: 1, username: 'Ja' },
      { userId: 2, username: 'Ana' },
      { userId: 3, username: 'Marko' },
    ];
    const members: MemberLike[] = [
      { userId: 1, username: 'Ja' },
      { userId: 2, username: 'Ana' },
      // Marko is gone
    ];

    const result = processMemberActivity({
      members,
      seen,
      enabled: true,
      myUserId: 1,
      timestamp: '2026-01-01T00:00:00.000Z',
    });

    expect(result.notifications).toHaveLength(1);
    const n = result.notifications[0];
    expect(n.id).toBe('member-leave-3');
    expect(n.type).toBe('member_leave');
    expect(n.desc).toContain('Marko'); // name recovered from the seen list
  });
});
