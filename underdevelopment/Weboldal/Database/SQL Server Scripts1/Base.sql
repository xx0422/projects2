CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE 
	CONSTRAINT CHK_Email CHECK (Email LIKE '%@%'),
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(30),
    Address NVARCHAR(MAX),
    Role NVARCHAR(20) CHECK (Role IN ('customer', 'admin')) DEFAULT 'customer',
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX)
);

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(10, 2) NOT NULL,
    Stock INT DEFAULT 0,
    CategoryId INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
        ON DELETE SET NULL
);

CREATE TABLE ProductImages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(255) NOT NULL,
    IsMain BIT DEFAULT 0,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
        ON DELETE CASCADE
);

CREATE TABLE Cart (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
        ON DELETE CASCADE
);

CREATE TABLE CartItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    FOREIGN KEY (CartId) REFERENCES Cart(Id)
        ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
        ON DELETE CASCADE
);

CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Total DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('pending', 'paid', 'shipped', 'completed', 'cancelled')) 
        DEFAULT 'pending',
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
        ON DELETE CASCADE
);

CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Quantity INT NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
        ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
        ON DELETE CASCADE
);

CREATE TABLE ProductImages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,

    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);


