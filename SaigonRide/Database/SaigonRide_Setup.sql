-- ============================================================
-- SaigonRide Database Setup Script
-- Run this in SQL Server Management Studio (SSMS)
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'SaigonRideDB')
    DROP DATABASE SaigonRideDB;
GO

CREATE DATABASE SaigonRideDB;
GO

USE SaigonRideDB;
GO

-- ============================================================
-- TABLE: Users
-- ============================================================
CREATE TABLE Users (
    UserID       INT IDENTITY(1,1) PRIMARY KEY,
    Username     VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(256) NOT NULL,
    UserType     VARCHAR(10)  NOT NULL CHECK (UserType IN ('Local','Tourist','Admin')),
    PassportID   VARCHAR(50)  NULL
);
GO

-- ============================================================
-- TABLE: Stations
-- ============================================================
CREATE TABLE Stations (
    StationID    INT IDENTITY(1,1) PRIMARY KEY,
    StationName  VARCHAR(100) NOT NULL,
    Capacity     INT          NOT NULL CHECK (Capacity > 0),
    CurrentCount INT          NOT NULL DEFAULT 0 CHECK (CurrentCount >= 0)
);
GO

-- ============================================================
-- TABLE: Vehicles
-- ============================================================
CREATE TABLE Vehicles (
    VehicleID  INT IDENTITY(1,1) PRIMARY KEY,
    Category   VARCHAR(20) NOT NULL CHECK (Category IN ('StandardBike','EScooter')),
    Status     VARCHAR(15) NOT NULL CHECK (Status IN ('Available','InTransit','Maintenance')),
    StationID  INT NULL FOREIGN KEY REFERENCES Stations(StationID)
);
GO

-- ============================================================
-- TABLE: Rentals
-- ============================================================
CREATE TABLE Rentals (
    RentalID        INT IDENTITY(1,1) PRIMARY KEY,
    UserID          INT            NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    VehicleID       INT            NOT NULL FOREIGN KEY REFERENCES Vehicles(VehicleID),
    StartStationID  INT            NOT NULL FOREIGN KEY REFERENCES Stations(StationID),
    EndStationID    INT            NULL     FOREIGN KEY REFERENCES Stations(StationID),
    StartTime       DATETIME       NOT NULL,
    EndTime         DATETIME       NULL,
    BaseFare        DECIMAL(18,2)  NULL,
    DiscountApplied BIT            NOT NULL DEFAULT 0,
    FinalFare       DECIMAL(18,2)  NULL,
    PaymentMethod   VARCHAR(50)    NULL
);
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Admin user (password: Admin@123)
INSERT INTO Users (Username, PasswordHash, UserType)
VALUES ('admin', 'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855', 'Admin');
-- Actual SHA-256 of 'Admin@123':
UPDATE Users SET PasswordHash = CONVERT(VARCHAR(256),
    HASHBYTES('SHA2_256', 'Admin@123'), 2) WHERE Username = 'admin';
GO

-- Sample stations
INSERT INTO Stations (StationName, Capacity, CurrentCount) VALUES
    ('Ben Thanh',    10, 3),
    ('Thao Dien',    8,  7),
    ('Bui Vien',     6,  0),
    ('Thu Duc',      10, 2),
    ('District 7',   8,  8);
GO

-- Sample vehicles
INSERT INTO Vehicles (Category, Status, StationID) VALUES
    ('StandardBike', 'Available', 1),
    ('StandardBike', 'Available', 1),
    ('EScooter',     'Available', 1),
    ('StandardBike', 'Available', 2),
    ('EScooter',     'Available', 2),
    ('EScooter',     'Maintenance', NULL),
    ('StandardBike', 'Available', 4),
    ('EScooter',     'Available', 4);
GO

-- Update station CurrentCount to match vehicles
UPDATE Stations SET CurrentCount = (
    SELECT COUNT(*) FROM Vehicles
    WHERE Vehicles.StationID = Stations.StationID
    AND Vehicles.Status = 'Available'
);
GO

PRINT 'SaigonRide database setup complete.';
GO
