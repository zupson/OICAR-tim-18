export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  username: string;
}

export interface Cost {
  id: number;
  amount: number;
  notes: string;
  transactionDate: string;
  costTypeId: number;
  costTypeName?: string;
  groupId?: number;
  groupName?: string;
  userId?: number;
  currency?: number;
}

export interface Revenue {
  id: number;
  amount: number;
  notes: string;
  transactionDate: string;
  revenueTypeId: number;
  revenueTypeName?: string;
  groupId?: number;
  groupName?: string;
  userId?: number;
  currency?: number;
}

export interface CostType {
  id: number;
  name: string;
  groupId: number;
}

export interface RevenueType {
  id: number;
  name: string;
  groupId: number;
}

export interface Budget {
  id: number;
  amount: number;
  year: number;
  month: number;
  currency: number;
  userGroupId: number;
}

export interface UserGroup {
  id: number;
  groupId: number;
  groupName: string;
  userId: number;
}

export interface Currency {
  code: string;
  symbol: string;
  name: string;
}

export interface Notification {
  id: string;
  type: string;
  title: string;
  desc: string;
  timestamp: string;
  read: boolean;
}

export interface NotifPrefs {
  budget_alert: boolean;
  shared_costs: boolean;
  member_activity: boolean;
}
