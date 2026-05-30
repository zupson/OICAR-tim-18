import { Injectable, signal, computed } from '@angular/core';
import { Budget, Cost, CostType, Currency, Revenue, RevenueType, User, UserGroup } from '../../models';

export const CURRENCIES: Currency[] = [
  { code: 'EUR', symbol: '€',   name: 'Euro' },
  { code: 'USD', symbol: '$',   name: 'Američki dolar' },
  { code: 'GBP', symbol: '£',   name: 'Britanska funta' },
  { code: 'CHF', symbol: 'Fr',  name: 'Švicarski franak' },
  { code: 'JPY', symbol: '¥',   name: 'Japanski jen' },
  { code: 'CAD', symbol: 'C$',  name: 'Kanadski dolar' },
  { code: 'AUD', symbol: 'A$',  name: 'Australski dolar' },
  { code: 'NZD', symbol: 'NZ$', name: 'Novozelandski dolar' },
  { code: 'SEK', symbol: 'kr',  name: 'Švedska kruna' },
  { code: 'NOK', symbol: 'kr',  name: 'Norveška kruna' },
  { code: 'DKK', symbol: 'kr',  name: 'Danska kruna' },
  { code: 'HUF', symbol: 'Ft',  name: 'Mađarska forinta' },
  { code: 'CZK', symbol: 'Kč',  name: 'Češka kruna' },
  { code: 'PLN', symbol: 'zł',  name: 'Poljski zlot' },
  { code: 'RON', symbol: 'lei', name: 'Rumunjski leu' },
  { code: 'BGN', symbol: 'лв',  name: 'Bugarski lev' },
  { code: 'RSD', symbol: 'din', name: 'Srpski dinar' },
  { code: 'BAM', symbol: 'KM',  name: 'Bosanska marka' },
  { code: 'MKD', symbol: 'ден', name: 'Makedonski denar' },
  { code: 'ALL', symbol: 'L',   name: 'Albanski lek' },
  { code: 'TRY', symbol: '₺',   name: 'Turska lira' },
  { code: 'RUB', symbol: '₽',   name: 'Ruski rubalj' },
  { code: 'UAH', symbol: '₴',   name: 'Ukrajinska hrivnja' },
  { code: 'CNY', symbol: '¥',   name: 'Kineski yuan' },
  { code: 'KRW', symbol: '₩',   name: 'Južnokorejski von' },
  { code: 'INR', symbol: '₹',   name: 'Indijska rupija' },
  { code: 'SGD', symbol: 'S$',  name: 'Singapurski dolar' },
  { code: 'HKD', symbol: 'HK$', name: 'Hongkonški dolar' },
  { code: 'MYR', symbol: 'RM',  name: 'Malezijski ringgit' },
  { code: 'THB', symbol: '฿',   name: 'Tajlandski baht' },
  { code: 'IDR', symbol: 'Rp',  name: 'Indonezijska rupija' },
  { code: 'PHP', symbol: '₱',   name: 'Filipinski peso' },
  { code: 'VND', symbol: '₫',   name: 'Vijetnamski dong' },
  { code: 'BRL', symbol: 'R$',  name: 'Brazilski real' },
  { code: 'MXN', symbol: 'MX$', name: 'Meksički peso' },
  { code: 'ARS', symbol: '$',   name: 'Argentinski peso' },
  { code: 'CLP', symbol: '$',   name: 'Čileanski peso' },
  { code: 'ZAR', symbol: 'R',   name: 'Južnoafrički rand' },
  { code: 'NGN', symbol: '₦',   name: 'Nigerijska naira' },
  { code: 'EGP', symbol: '£',   name: 'Egipatska funta' },
  { code: 'AED', symbol: 'د.إ', name: 'UAE dirham' },
  { code: 'SAR', symbol: '﷼',   name: 'Saudijski rijal' },
  { code: 'QAR', symbol: '﷼',   name: 'Katarski rijal' },
  { code: 'ILS', symbol: '₪',   name: 'Izraelski novi šekel' },
];

export const MONTHS_CAP = ['Siječanj','Veljača','Ožujak','Travanj','Svibanj','Lipanj','Srpanj','Kolovoz','Rujan','Listopad','Studeni','Prosinac'];
export const MONTHS_S   = ['Sij','Velj','Ožu','Tra','Svi','Lip','Srp','Kol','Ruj','Lis','Stu','Pro'];
export const COLORS     = ['#3B82F6','#22C55E','#F59E0B','#8B5CF6','#06B6D4','#EF4444','#F97316','#EC4899'];

function loadCurrency(): Currency {
  try {
    const c = JSON.parse(localStorage.getItem('spendly_currency') || 'null');
    return c?.symbol ? c : CURRENCIES[0];
  } catch { return CURRENCIES[0]; }
}

@Injectable({ providedIn: 'root' })
export class StateService {
  readonly token    = signal<string | null>(null);
  readonly user     = signal<User | null>(null);
  readonly costs    = signal<Cost[]>([]);
  readonly revenues = signal<Revenue[]>([]);
  readonly costTypes    = signal<CostType[]>([]);
  readonly revenueTypes = signal<RevenueType[]>([]);
  readonly budgets   = signal<Budget[]>([]);
  readonly userGroups = signal<UserGroup[]>([]);
  readonly personalGroupId     = signal<number | null>(null);
  readonly personalUserGroupId = signal<number | null>(null);
  readonly currency = signal<Currency>(loadCurrency());

  readonly currencySymbol = computed(() => this.currency().symbol);

  fmtNum(n: number): string {
    return Number(n).toFixed(2).replace('.', ',').replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  }

  fmt(n: number): string {
    return this.currencySymbol() + this.fmtNum(n);
  }

  setCurrency(c: Currency): void {
    this.currency.set(c);
    localStorage.setItem('spendly_currency', JSON.stringify(c));
  }

  clearAll(): void {
    this.token.set(null);
    this.user.set(null);
    this.costs.set([]);
    this.revenues.set([]);
    this.costTypes.set([]);
    this.revenueTypes.set([]);
    this.budgets.set([]);
    this.userGroups.set([]);
    this.personalGroupId.set(null);
    this.personalUserGroupId.set(null);
  }
}
