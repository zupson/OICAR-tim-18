CREATE DATABASE Spendly
GO
USE Spendly
GO

CREATE TABLE [Role](
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[Name] NVARCHAR(50) NOT NULL
)
GO

CREATE TABLE [User](
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	FistName NVARCHAR(100) NOT NULL,
	LastName NVARCHAR(100) NOT NULL,
	Email NVARCHAR(150) NOT NULL,
	Username NVARCHAR(100) NOT NULL,
	PasswordHash NVARCHAR(200) NOT NULL,
	PasswordSalt NVARCHAR(200) NOT NULL
)
GO

--junction table USER <->ROLE 
CREATE TABLE UserRole(
	UserId INT NOT NULL,
	RoleId INT NOT NULL,

	CONSTRAINT PK_UserRole PRIMARY KEY(UserId, RoleId),
	CONSTRAINT FK_UserRole_Person FOREIGN KEY(UserId) REFERENCES [User](Id),
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleId) REFERENCES [Role](Id)
)
GO


CREATE TABLE [Group](
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[Name] NVARCHAR(50) NOT NULL,
	IsPersonal BIT NOT NULL DEFAULT 0
)
GO

CREATE TABLE Invitation(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Token NVARCHAR(150) NOT NULL,
	ClaimedAt DATETIME NULL,
	ExpiredAt DATETIME NOT NULL,

	GroupId INT NOT NULL,
	CONSTRAINT FK_Invitation_Group FOREIGN KEY (GroupId) REFERENCES [Group](Id),
	
	CreatedByUserId INT NOT NULL,
	CONSTRAINT FK_Invitation_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES [User](Id)
)
GO


--junction table USER <->GROUP 

CREATE TABLE UserGroup(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	UserId INT NOT NULL,
	GroupId INT NOT NULL,
	
	CONSTRAINT FK_UserGroup_User FOREIGN KEY (UserId) REFERENCES [User](Id),
	CONSTRAINT FK_UserGroup_Group FOREIGN KEY (GroupId) REFERENCES [Group](Id),

	JoinedAt DATETIME NOT NULL DEFAULT SYSDATETIME(),
	InvitationId INT UNIQUE NULL, --jer ako je user koji je samostalan ili je kretor grupe,
									--ne treba mu toke za uæi u grupu

	CONSTRAINT FK_UserGroup_Invitation FOREIGN KEY (InvitationId) REFERENCES Invitation(Id)
)
GO




CREATE TABLE Currency(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[Name] NVARCHAR(50) NOT NULL,
	CurrencyExcangeRate DECIMAL NOT NULL
)
GO

CREATE TABLE CostType(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[Name] NVARCHAR(50) NOT NULL,
)
GO

CREATE TABLE RevenueType(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[Name] NVARCHAR(50) NOT NULL,
)
GO

CREATE TABLE Budget(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Amount DECIMAL NOT NULL,
	[Year] INT NOT NULL,
	[Month] INT NOT NULL,

	
	UserGroupId INT NOT NULL,
	CurrencyId INT NOT NULL,
	CostTypeId INT NOT NULL,
	RevenueTypeId INT NOT NULL,

	
    CONSTRAINT FK_Budget_UserGroup FOREIGN KEY (UserGroupId) REFERENCES UserGroup(Id),
	CONSTRAINT FK_Budget_Currency FOREIGN KEY (CurrencyId) REFERENCES Currency(Id),
	CONSTRAINT FK_Budget_CostType FOREIGN KEY (CostTypeId) REFERENCES CostType(Id),
	CONSTRAINT FK_Budget_RevenueType FOREIGN KEY (RevenueTypeId) REFERENCES RevenueType(Id),
	CONSTRAINT UQ_Budget UNIQUE (UserGroupId, CostTypeId, RevenueTypeId, [Month], [Year])
)
GO



CREATE TABLE Cost(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Amount DECIMAL NOT NULL,
	TransactionDate DATETIME NOT NULL DEFAULT SYSDATETIME(),
	Notes NVARCHAR(MAX) NULL,

	UserId INT NOT NULL,
	CurrencyId INT NOT NULL,
	CostTypeId INT NOT NULL,

	CONSTRAINT FK_Cost_UserGroup FOREIGN KEY (UserId) REFERENCES [User](Id),
	CONSTRAINT FK_Cost_Currency FOREIGN KEY (CurrencyId) REFERENCES Currency(Id),
	CONSTRAINT FK_Cost_CostType FOREIGN KEY (CostTypeId) REFERENCES CostType(Id),
)
GO



--junction table GROUP <-> COST

CREATE TABLE GroupCost(
	GroupId INT NOT NULL,
	CostId INT NOT NULL,	
	UserId INT NOT NULL,

	CONSTRAINT PK_GroupCost PRIMARY KEY(GroupId, CostId),
	CONSTRAINT FK_GroupCost_Group FOREIGN KEY (GroupId) REFERENCES [Group](Id),
	CONSTRAINT FK_GroupCost_Cost FOREIGN KEY (CostId) REFERENCES Cost(Id),
	CONSTRAINT FK_GroupCost_User FOREIGN KEY (UserId) REFERENCES [User](Id)
)
GO


CREATE TABLE Revenue(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Amount DECIMAL NOT NULL,
	TransactionDate DATETIME NOT NULL DEFAULT SYSDATETIME(),
	Notes NVARCHAR(MAX) NULL,

	UserId INT NOT NULL,
	CurrencyId INT NOT NULL,
	RevenueTypeId INT NOT NULL,

	CONSTRAINT FK_Revenue_UserGroup FOREIGN KEY (UserId) REFERENCES [User](Id),
	CONSTRAINT FK_Revenue_Currency FOREIGN KEY (CurrencyId) REFERENCES Currency(Id),
	CONSTRAINT FK_Revenue_RevenueType FOREIGN KEY (RevenueTypeId) REFERENCES RevenueType(Id),
)
GO

--junction table GROUP <-> REVENUE

CREATE TABLE GroupRevenue(
	GroupId INT NOT NULL,
	RevenueId INT NOT NULL,	
	UserId INT NOT NULL,

	CONSTRAINT PK_GroupRevenue PRIMARY KEY(GroupId, RevenueId),
	CONSTRAINT FK_GroupRevenue_Group FOREIGN KEY (GroupId) REFERENCES [Group](Id),
	CONSTRAINT FK_GroupRevenue_Cost FOREIGN KEY (RevenueId) REFERENCES Revenue(Id),
	CONSTRAINT FK_GroupRevenue_User FOREIGN KEY (UserId) REFERENCES [User](Id)
)
GO