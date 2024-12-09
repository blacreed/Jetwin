--CREATE DATABASE Jetwin

CREATE TABLE Status (
    StatusID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    StatusName VARCHAR(20) NOT NULL UNIQUE
);
INSERT INTO Status VALUES ('Active'), ('Inactive'), ('Archived');

--USER
CREATE TABLE Staff (
    UserID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL, -- SHOULD HASH PASSWORD??
    EmployeeName VARCHAR(50) NOT NULL UNIQUE,
    ContactNum VARCHAR(15),
    RoleName VARCHAR(20) NOT NULL,
    StatusID INT NOT NULL FOREIGN KEY REFERENCES Status(StatusID) DEFAULT 1,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
    LastLogin DATETIME
);
INSERT INTO Staff (Username, Password, EmployeeName, ContactNum, RoleName, StatusID)
VALUES ('admin', 'admin', 'Administrator', NULL, 'admin', 1); --DEFAULT FOR ADMIN

--SUPPLIER
CREATE TABLE SupplierContactInfo (
    ContactID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	ContactPerson VARCHAR(50) NOT NULL,
    ContactNum VARCHAR(15) NOT NULL,
);

CREATE TABLE Supplier (
    SupplierID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    Supplier VARCHAR(50) NOT NULL UNIQUE,
    ContactID INT FOREIGN KEY REFERENCES SupplierContactInfo(ContactID),
    Remarks TEXT,
    LastOrderDate DATETIME,
    StatusID INT FOREIGN KEY REFERENCES Status(StatusID),
);

--PRODUCT AND RELATED ENTITIES
CREATE TABLE Category (
    CategoryID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    CategoryName VARCHAR(50) NOT NULL UNIQUE,
    StatusID INT NOT NULL FOREIGN KEY REFERENCES Status(StatusID),
);

CREATE TABLE Brand (
	BrandID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	BrandName VARCHAR(50) NOT NULL UNIQUE,
	StatusID INT NOT NULL FOREIGN KEY REFERENCES Status(StatusID),
);

CREATE TABLE AttributeType (
    AttributeTypeID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    AttributeTypeName VARCHAR(50) NOT NULL UNIQUE, -- E.g., "Color", "Material"
	StatusID INT NOT NULL FOREIGN KEY REFERENCES Status(StatusID),
);

CREATE TABLE AttributeValue (
    AttributeValueID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    AttributeTypeID INT NOT NULL FOREIGN KEY REFERENCES AttributeType(AttributeTypeID),
    AttributeValueName VARCHAR(50) NOT NULL UNIQUE,
	StatusID INT NOT NULL FOREIGN KEY REFERENCES Status(StatusID),
	UNIQUE (AttributeTypeID, AttributeValueName)
);

CREATE TABLE UnitOfMeasurement (
    UoMID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    UoMName VARCHAR(20) NOT NULL UNIQUE
);
INSERT INTO UnitOfMeasurement VALUES ('Pcs'),('Box'),('Set'),('Unit'),('Liter'),('Kg'),('Ton');

CREATE TABLE Product (
	ProductCode INT NOT NULL PRIMARY KEY IDENTITY(1,1), --should not be identity(1, 1)
	ProductName VARCHAR(50) NOT NULL,
	CategoryID INT NOT NULL FOREIGN KEY REFERENCES Category(CategoryID),
    BrandID INT NOT NULL FOREIGN KEY REFERENCES Brand(BrandID),
	UnitPrice DECIMAL(10,2) NOT NULL,
	UoMID INT NOT NULL FOREIGN KEY REFERENCES UnitOfMeasurement(UoMID),
    SupplierID INT NOT NULL FOREIGN KEY REFERENCES Supplier(SupplierID),
	StatusID INT NOT NULL FOREIGN KEY REFERENCES Status(StatusID),
);

CREATE TABLE ProductAttributes (
    ProductAttributeID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    ProductCode INT NOT NULL FOREIGN KEY REFERENCES Product(ProductCode),
	AttributeTypeID INT NOT NULL FOREIGN KEY REFERENCES AttributeType(AttributeTypeID),
    AttributeValueID INT NOT NULL FOREIGN KEY REFERENCES AttributeValue(AttributeValueID),
	UNIQUE (ProductCode, AttributeValueID)
);

--INVENTORY (edited, made attributecombinationid not null > null
CREATE TABLE Inventory (
    InventoryID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    ProductCode INT NOT NULL FOREIGN KEY REFERENCES Product(ProductCode),
	AttributeCombinationID INT NULL FOREIGN KEY REFERENCES ProductAttributes(ProductAttributeID),
    Quantity INT NOT NULL DEFAULT 0,
	MinimumStockLevel INT NULL,
	MaximumStockLevel INT NULL,
    ReorderPoint INT NULL,
    Location VARCHAR(50),
);

--AUDIT TRAIL
CREATE TABLE AuditTrail (
    AuditID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL FOREIGN KEY REFERENCES Staff(UserID),
    Action NVARCHAR(255) NOT NULL,
    ActionDate DATETIME DEFAULT GETDATE(),
    Details NVARCHAR(255)
);
--SALES AREA
CREATE TABLE SaleStatus (
	SaleStatusID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	SaleStatusName VARCHAR(20) NOT NULL UNIQUE
)
INSERT INTO SaleStatus VALUES ('Completed'), ('Returned');

CREATE TABLE Sales (
    SalesID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	SaleStatusID INT NOT NULL FOREIGN KEY REFERENCES SaleStatus(SaleStatusID),
    SalesDate DATETIME NOT NULL DEFAULT GETDATE(),
    StaffID INT NOT NULL FOREIGN KEY REFERENCES Staff(UserID),
);

CREATE TABLE SalesDetails (
    SalesDetailID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    SalesID INT NOT NULL FOREIGN KEY REFERENCES Sales(SalesID),
    ProductCode INT NOT NULL FOREIGN KEY REFERENCES Product(ProductCode),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
	Discount DECIMAL(10,2) DEFAULT 0,
	TotalAmount AS (Quantity * UnitPrice - Discount) PERSISTED
);

CREATE TABLE Payment (
    PaymentID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	PaymentType VARCHAR(10) NOT NULL, --from combobox, user can only select cash and gcash
    SalesID INT NOT NULL FOREIGN KEY REFERENCES Sales(SalesID),
    AmountPaid DECIMAL(10,2) NOT NULL,
    PaymentReference VARCHAR(50)-- For gcash reference number only
);
--RETURNS
CREATE TABLE Returns (
    ReturnID INT PRIMARY KEY IDENTITY(1,1),
    SaleDetailID INT NOT NULL FOREIGN KEY REFERENCES SalesDetails(SalesDetailID),
    ReturnDate DATETIME DEFAULT GETDATE(),
    Quantity INT NOT NULL,
    Reason NVARCHAR(255)
);
CREATE TABLE Adjustments (
    AdjustmentID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    ProductCode INT NOT NULL FOREIGN KEY REFERENCES Product(ProductCode),
    AdjustmentReason NVARCHAR(255) NOT NULL,
    QuantityAdjusted INT NOT NULL,
    AdjustmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    UserID INT NOT NULL FOREIGN KEY REFERENCES Staff(UserID),
    Remarks NVARCHAR(255)
);
CREATE TABLE Restocking (
    RestockingID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    ProductCode INT NOT NULL FOREIGN KEY REFERENCES Product(ProductCode),
    SupplierID INT NOT NULL FOREIGN KEY REFERENCES Supplier(SupplierID),
    QuantityRestocked INT NOT NULL,
    RestockDate DATETIME NOT NULL DEFAULT GETDATE(),
    UserID INT NOT NULL FOREIGN KEY REFERENCES Staff(UserID),
    Remarks NVARCHAR(255)
);
--PURCHASE ORDER
CREATE TABLE PurchaseOrderStatus (
    POStatusID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    POStatusName VARCHAR(20) NOT NULL UNIQUE
);
INSERT INTO PurchaseOrderStatus VALUES ('Pending'), ('Received');

CREATE TABLE PurchaseOrder (
    POID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    SupplierID INT NOT NULL FOREIGN KEY REFERENCES Supplier(SupplierID),
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    DeliveryDate DATETIME NOT NULL, -- EXPECTED DELIVERY DATE
    POStatusID INT NOT NULL FOREIGN KEY REFERENCES PurchaseOrderStatus(POStatusID),
    TotalCost DECIMAL(10,2),
    Remarks TEXT,
);

CREATE TABLE PurchaseOrderDetail (
    PODetailID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    POID INT NOT NULL FOREIGN KEY REFERENCES PurchaseOrder(POID),
    ProductCode INT NOT NULL FOREIGN KEY REFERENCES Product(ProductCode),
    Quantity INT NOT NULL,
    UnitCost DECIMAL(10,2) NOT NULL,
    TotalCost AS (Quantity * UnitCost) PERSISTED -- AUTO CALCULATED
);

--INDEXES
CREATE INDEX IX_Product_CategoryID ON Product(CategoryID);
CREATE INDEX IX_Sales_SalesDate ON Sales(SalesDate);
CREATE INDEX IX_Inventory_ProductID ON Inventory(ProductCode);