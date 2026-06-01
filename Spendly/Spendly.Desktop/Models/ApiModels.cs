namespace Spendly.Desktop.Models;

public record ApiLoginResponse(ApiUser User, string Token);
public record ApiUser(int Id, string FirstName, string LastName, string Email, string Username, ApiUserGroup[] Groups);
public record ApiUserGroup(int Id, int UserId, string? Username, int GroupId, string? GroupName, DateTime JoinedAt, bool IsPersonal = false);
public record ApiCost(int Id, decimal Amount, DateTime TransactionDate, string? Notes, int UserId, int Currency, int CostTypeId, string? CostTypeName, int GroupId, string? GroupName);
public record ApiRevenue(int Id, decimal Amount, DateTime TransactionDate, string? Notes, int UserId, int Currency, int RevenueTypeId, string? RevenueTypeName, int GroupId, string? GroupName);
public record ApiCostType(int Id, string? Name, int GroupId);
public record ApiRevenueType(int Id, string? Name, int GroupId);
public record ApiBudget(int Id, decimal Amount, int Year, int Month, int UserGroupId, int Currency);
public record ApiTypeOption(string Name, int TypeId, bool IsRevenue, int GroupId);
