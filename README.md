# StarterApp - Library of Things Rental App

## Project Overview

This project is a **Library of Things rental marketplace mobile application** built for the **SET09102 Software Engineering coursework**.

The app allows users to:

- Create item listings
- Browse available items
- Search for nearby items
- Request rentals
- Manage rental requests
- Leave reviews
- View average owner ratings

The main implemented features are:

- User registration and login using the provided backend API
- JWT token storage and bearer token authentication
- Item creation, listing, detail view, and editing
- Location-based item discovery using GPS and PostGIS
- Adjustable nearby search radius slider
- Rental request creation
- Incoming and outgoing rental request management
- Rental workflow:
  - Requested
  - Approved
  - Rejected
  - Out For Rent
  - Returned
  - Completed
- Review creation after completed rentals
- Item review display
- Average owner rating displayed on the profile page
- xUnit automated tests
- GitHub Actions CI/CD pipeline

The project focuses mainly on **Tier 1** and **Tier 2** requirements.

Some Tier 3 ideas are partially reflected through the rental state workflow and adjustable radius slider, but full Tier 3 features such as MediatR/CQRS, formal State Pattern classes, overdue automation, map integration, and SonarCloud were left out as time proved difficult to manage alongside other demanding modules.

---

## Required Technologies

| Technology | Purpose |
|---|---|
| .NET 10 | Main framework version |
| .NET MAUI | Cross-platform mobile UI framework |
| PostgreSQL 16 with PostGIS | Database and spatial/location support |
| Entity Framework Core | ORM and migrations |
| NetTopologySuite | Spatial point support for PostGIS queries |
| xUnit | Testing framework |
| GitHub Actions | CI/CD pipeline |
| Docker Compose | Development database environment |

---

## Repository Structure

The project follows a layered structure based on the StarterApp solution.

```text
StarterApp/
├── .github/
│   └── workflows/
│       └── build.yml              # GitHub Actions CI/CD workflow
├── StarterApp/                    # Main .NET MAUI app
│   ├── Views/                     # XAML pages
│   ├── ViewModels/                # MVVM ViewModels
│   ├── Services/                  # Business logic and service abstractions
│   ├── Platforms/Android/         # Android configuration
│   └── MauiProgram.cs             # Dependency injection setup
├── StarterApp.Database/           # Shared database library
│   ├── Data/
│   │   ├── AppDbContext.cs        # EF Core database context
│   │   └── Repositories/          # Repository pattern classes/interfaces
│   └── Models/                    # Entity models
├── StarterApp.Migrations/         # EF Core migrations project
├── StarterApp.Test/               # xUnit tests
│   ├── Services/                  # Service tests
│   ├── ViewModels/                # ViewModel tests
│   ├── Repositories/              # Repository/integration tests
│   └── Fixtures/                  # DatabaseFixture
├── docker-compose.yml             # PostgreSQL/PostGIS development container
├── .gitignore
├── README.md
└── StarterApp.sln
```

---

## Setup Instructions

### Prerequisites

Install the following before running the project:

- Docker Desktop
- Visual Studio Code
- .NET 10 SDK
- Android SDK / Android emulator
- Git
- ADB command line tools

The project was developed using a Docker/Dev Container setup with PostgreSQL/PostGIS.

---

## Docker and Database Setup

From the project root folder, start the containers:

```bash
docker compose up -d
```

This starts the PostgreSQL/PostGIS database container.

To reset the containers and database volumes:

```bash
docker compose down -v
docker compose up -d
```

The local development database uses PostgreSQL 16 with PostGIS support.

Example development connection string:

```text
Host=localhost;Port=5432;Username=app_user;Password=app_password;Database=appdb
```

Inside the Dev Container, the app may use the Docker host address depending on the environment.

---

## Apply Database Migrations

From the project root folder, run:

```bash
dotnet ef database update --project StarterApp.Database --startup-project StarterApp.Migrations
```

This applies the Entity Framework Core migrations and creates or updates the database schema.

---

## How to Build the Application

From the project root folder, run:

```bash
dotnet clean
dotnet build -c Debug
```

It will take ~6 minutes to fully build the required APK.

For Android, the APK is generated at:

```text
StarterApp/bin/Debug/net10.0-android/com.companyname.starterapp-Signed.apk
```

---

## How to Install the APK on the Android Emulator

Make sure the Android emulator is running first.

Then run this from Windows Command Prompt or PowerShell:

```powershell
adb uninstall com.companyname.starterapp
adb install -r "C:\Users\gameb\Desktop\starterapp\StarterApp\bin\Debug\net10.0-android\com.companyname.starterapp-Signed.apk"
```

Uninstalling first avoids Android signing conflicts if an older version of the app is already installed.

---

## How to Run the Application

1. Start Docker containers:

```bash
docker compose up -d
```

2. Build the project:

```bash
dotnet build -c Debug
```

3. Start the Android emulator.

4. Install the APK using ADB:

```powershell
adb install -r "C:\(whereever your starterapp is stored)\starterapp\StarterApp\bin\Debug\net10.0-android\com.companyname.starterapp-Signed.apk"
```

5. Open the app on the emulator.

---

## How to Run Tests

Run all xUnit tests:

```bash
dotnet test StarterApp.Test/StarterApp.Test.csproj
```

Current expected result:

```text
38 tests
0 failed
38 succeeded
```

---

## How to Run Tests with Coverage

Run:

```bash
dotnet test StarterApp.Test/StarterApp.Test.csproj --settings coverlet.runsettings --collect:"XPlat Code Coverage"
```

Then run the coverage summary script:

```bash
python3 - <<'PY'
import glob
import os
import xml.etree.ElementTree as ET

files = glob.glob("StarterApp.Test/TestResults/**/coverage.cobertura.xml", recursive=True)

if not files:
    print("No coverage.cobertura.xml file found.")
    raise SystemExit

latest = max(files, key=os.path.getmtime)

tree = ET.parse(latest)
root = tree.getroot()

line_rate = float(root.attrib.get("line-rate", 0)) * 100
branch_rate = float(root.attrib.get("branch-rate", 0)) * 100
lines_covered = root.attrib.get("lines-covered", "unknown")
lines_valid = root.attrib.get("lines-valid", "unknown")
branches_covered = root.attrib.get("branches-covered", "unknown")
branches_valid = root.attrib.get("branches-valid", "unknown")

print(f"Coverage file: {latest}")
print(f"Line coverage: {line_rate:.2f}%")
print(f"Branch coverage: {branch_rate:.2f}%")
print(f"Lines covered: {lines_covered}/{lines_valid}")
print(f"Branches covered: {branches_covered}/{branches_valid}")
PY
```

Latest focused coverage result:

```text
Line coverage: 98.55%
Branch coverage: 75.00%
```

---

## API Endpoint Documentation

The application integrates with the provided StarterApp backend authentication API.

API base URL:

```text
https://set09102-api.b-davison.workers.dev/
```

Main authentication endpoints used:

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/auth/token` | Logs in a user and returns a JWT token |
| POST | `/auth/register` | Registers a new user |
| GET | `/users/me` | Gets the current authenticated user profile |

The app obtains a JWT token from the API, stores it during the session, and attaches it to authenticated API requests using the bearer token header.

Token expiry is handled by checking the expiry time and clearing the authentication state when the token expires.

A full automatic refresh token flow was not implemented because the provided API response looks like it only supplies an access token and expiry time.

---

## Architecture Overview

The app follows an **MVVM** and layered architecture.

```text
View
↓
ViewModel
↓
Service
↓
Repository
↓
Database
```

---

## Views

Views are MAUI XAML pages.

They display data and bind to ViewModel properties and commands.

Examples:

- `ItemsListPage.xaml`
- `ItemDetailPage.xaml`
- `CreateItemPage.xaml`
- `NearbyItemsPage.xaml`
- `RentalsPage.xaml`
- `CreateReviewPage.xaml`
- `ProfilePage.xaml`

---

## ViewModels

ViewModels expose bindable properties and commands.

Examples:

- `ItemsListViewModel`
- `ItemDetailViewModel`
- `CreateItemViewModel`
- `NearbyItemsViewModel`
- `RentalsViewModel`
- `CreateReviewViewModel`
- `ProfileViewModel`

The project uses CommunityToolkit.Mvvm features such as:

- `ObservableObject`
- `[ObservableProperty]`
- `[RelayCommand]`

---

## Services

Services contain business logic and keep rules out of the ViewModels.

Examples:

- `ApiAuthenticationService`
- `LocationService`
- `RentalService`
- `ReviewService`

### RentalService

`RentalService` handles rental business rules such as:

- Creating rental requests
- Approving and rejecting requests
- Marking rentals as out for rent
- Marking rentals as returned
- Completing rentals
- Preventing invalid state transitions
- Preventing double-booking
- Calculating total price

### ReviewService

`ReviewService` handles review rules such as:

- Only completed rentals can be reviewed
- Users cannot review rentals they did not request
- Users cannot review the same rental twice
- Average owner rating can be calculated for the profile page

### LocationService

`LocationService` handles GPS access and keeps device location logic separate from the ViewModels.

---

## Repositories

Repositories abstract database access from ViewModels and services.

The project includes a generic base repository interface:

```text
IRepository<T>
```

Specific repositories include:

- `IItemRepository / ItemRepository`
- `IRentalRepository / RentalRepository`
- `IReviewRepository / ReviewRepository`

---

## Database

The project uses Entity Framework Core with PostgreSQL/PostGIS.

Item locations are stored as:

```text
geography(point, 4326)
```

Nearby item search uses NetTopologySuite spatial points and PostGIS backed distance queries.

The project also uses a GIST spatial index for item locations.

---

## Implemented Features

### Authentication

Implemented authentication features include:

- Register using the backend API
- Login using the backend API
- Obtain and store JWT token
- Add bearer token to authenticated API requests
- Handle token expiry
- Sync API user into local database for item/rental/review ownership

---

### Item Management

Implemented item management features include:

- Create item listing
- View list of all items
- View item details
- Edit item details
- Store item title, description, category, daily rate, location name, latitude, and longitude
- Store PostGIS spatial point for item location

---

### Location-Based Discovery

Implemented location features include:

- Get current device/emulator GPS location
- Search for nearby items
- Configurable search radius
- Adjustable radius slider
- PostGIS/NetTopologySuite spatial query support

---

### Rental Workflow

Implemented rental states:

```text
Requested → Approved / Rejected
Approved → Out For Rent
Out For Rent → Returned
Returned → Completed
```

Invalid transitions are blocked inside `RentalService`.

This behaves like a simple rental state machine, although it is not a full formal State Pattern implementation with separate state classes.

---

### Reviews and Feedback

Implemented review features include:

- Borrowers can leave a review after a rental is completed
- Reviews include a 1-5 rating and comment
- Reviews can be viewed on the item detail page
- User profile displays average owner rating from reviews on owned items

---

## Testing

The test project includes:

- xUnit tests
- AAA pattern
- Service tests
- ViewModel tests
- Repository/integration tests
- DatabaseFixture pattern
- MockLocationService for GPS testing
- Fake repositories/navigation services for isolated ViewModel testing
- XPlat Code Coverage output

Current expected test result:

```text
38 tests
0 failed
38 succeeded
```

Latest focused coverage result:

```text
Line coverage: 98.55%
Branch coverage: 75.00%
```

---

## CI/CD

GitHub Actions workflow file:

```text
.github/workflows/build.yml
```

The workflow:

- Runs on push and pull request
- Checks out the repository
- Sets up .NET 10
- Starts a PostGIS service container
- Restores projects
- Builds projects
- Runs xUnit tests
- Collects coverage
- Uploads the coverage XML as an artifact

---

## Notes on Tier 3 Features

The project mainly focuses on Tier 1 and Tier 2 requirements.

Some Tier 3 concepts are partially included:

- Rental workflow validation behaves like a simple state machine
- The nearby item search includes an adjustable radius slider

Full Tier 3 implementations were not completed due to time constraints and the priority of delivering a stable, tested Tier 1 and Tier 2 application.

Teir 3 features that are not implemented:

- Formal State Pattern classes such as `RequestedState` and `ApprovedState`
- MediatR/CQRS event-driven architecture
- Automatic overdue detection
- Full map integration
- SonarCloud quality gates

---

## AI Tool Usage

AI tools were used to support:

- Debugging
- Code explanation
- Test planning
- Refactoring guidance
- README file cleanup

All AI assisted code was reviewed, manually tested, understood and checked using automated tests before being included in the project.

I (Reece McMillan) am responsible for understanding and explaining the submitted code.