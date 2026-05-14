# SaigonRide — Distributed Vehicle Rental System
**SE Final Project | Tier 1 — C# WinForms + SQL Server | Semester 2 (2025–2026)**

| Member | Student ID | Role |
|---|---|---|
| Vu Van Minh Hieu | 524K0005 | Scrum Master + Developer (UC05 Vehicles + Revenue Report) |
| Nguyen Gia Bao | 524K0001 | Product Owner + Developer (UC04 Rentals + Station Report) |

---

## Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| Visual Studio | 2022 (17.x) | With **.NET desktop development** workload |
| .NET SDK | 8.0 (Windows) | Included with VS 2022 |
| SQL Server | Any edition | Express, Developer, or Standard |
| SQL Server Management Studio | 19+ / 22 | Used to run the setup script |

---

## Step 1 — Set Up the Database

1. Open **SQL Server Management Studio (SSMS)**.
2. Connect to your SQL Server instance (default: `.` or `localhost`).
3. Open the file:
   ```
   SaigonRide\Database\SaigonRide_Setup.sql
   ```
4. Click **Execute** (F5).
5. Verify the message: `SaigonRide database setup complete.`

The script creates the `SaigonRideDB` database with all tables and seed data (5 stations, 8 vehicles, 1 admin account).

---

## Step 2 — Configure the Connection String

Open `SaigonRide\Database\DatabaseHelper.cs` and verify the connection string:

```csharp
private const string ConnectionString =
    @"Server=.;Database=SaigonRideDB;Trusted_Connection=True;TrustServerCertificate=True;";
```

**If your SQL Server has a named instance** (e.g., `SQLEXPRESS`), change it to:
```csharp
@"Server=.\SQLEXPRESS;Database=SaigonRideDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

---

## Step 3 — Build and Run

1. Open **`SaigonRide.sln`** in Visual Studio 2022.
2. Right-click the solution → **Restore NuGet Packages**.
3. Press **F5** (or **Ctrl+F5** to run without debugger).
4. The Login screen will appear.

---

## Step 4 — Log In

### Default Admin Account
| Field | Value |
|---|---|
| Username | `admin` |
| Password | `Admin@123` |
| User Type | `Admin` |

### Register New Users
Click **Register** on the login screen to create a Local or Tourist account.

---

## System Features

### As System Admin
| Feature | Location | Owner |
|---|---|---|
| Manage Vehicles (CRUD) | Dashboard → Manage Vehicles | Hieu (UC05) |
| Revenue Report by Category | Dashboard → Revenue Report | Hieu (UC06) |
| All Rental Records (CRUD) | Dashboard → My Rentals | Bao (UC04) |
| Station Inventory Report | Dashboard → Station Report | Bao (UC07) |

### As Local Commuter / Foreign Tourist
| Feature | Payment Methods |
|---|---|
| Start & End Rentals | Local: MoMo, VNPay, Cash |
| View own rental history | Tourist: Apple Pay, PayPal, Cash |

### Business Rules
- **Standard Bike**: 500 VND/min
- **E-Scooter**: 1,500 VND/min
- **15% Discount**: Applied automatically when return station capacity < 20%
- **Block In-Transit delete**: Cannot delete a vehicle currently being rented

---

## Project Structure (3-Tier Architecture)

```
SaigonRide/
├── SaigonRide.sln
└── SaigonRide/
    ├── Program.cs                  ← Entry point
    ├── SaigonRide.csproj
    ├── Database/
    │   ├── DatabaseHelper.cs       ← Connection string (edit here)
    │   └── SaigonRide_Setup.sql    ← Run this in SSMS first
    ├── Models/
    │   ├── User.cs                 ← User, LocalCommuter, ForeignTourist
    │   └── Models.cs               ← Vehicle, Station, Rental
    ├── DAL/  (Data Access Layer)
    │   ├── UserDAL.cs
    │   ├── VehicleDAL.cs           ← Hieu
    │   └── StationRentalDAL.cs     ← Bao
    ├── BLL/  (Business Logic Layer)
    │   └── BusinessLogic.cs        ← UserBLL, VehicleBLL, RentalBLL, ReportBLL
    └── Forms/  (Presentation Layer)
        ├── LoginRegisterForms.cs
        ├── MainDashboard.cs
        ├── VehicleManagementForm.cs ← Hieu (UC05)
        ├── RentalCheckoutForms.cs   ← Bao (UC04)
        └── ReportForms.cs           ← Hieu (UC06) + Bao (UC07)
```

---

## Troubleshooting

| Problem | Solution |
|---|---|
| `Cannot connect to SQL Server` | Change `Server=.` to your instance name in `DatabaseHelper.cs` |
| `Login failed for user` | Ensure Windows Authentication is enabled on your SQL Server |
| `Database already exists` | The script drops and recreates `SaigonRideDB` — this is expected |
| Build errors | Right-click solution → Restore NuGet Packages, then rebuild |

---

## GitHub Repository
> https://github.com/RenoDespair/SaigonRide.git

Both team members must have **>10 meaningful commits** each.
