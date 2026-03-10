# SoulNotes

SoulNotes is an emotion journaling application built with **C#**, **ASP.NET Core MVC**, **Razor Views**, and **SQLite**.

The solution consists of:
- a **web application** with a server-rendered user interface,
- a **REST API** for working with records, emotions, and tags,
- and a **console client** that consumes the API.

The application allows users to create diary records, assign emotions and tags, manage their own emotion/tag lists, and view simple emotional statistics over time.

## Architecture

SoulNotes is built as a multi-project .NET solution.

The main web application is a server-rendered **ASP.NET Core MVC** project using **Razor Views** and **session-based authentication** for the browser interface.

The application also exposes a simple **REST API** secured with **login + token validation**.

A separate **console client** communicates with the API over HTTP and allows basic management of emotions and tags from the command line.

Data is stored in a local **SQLite** database and accessed through custom C# services using **Microsoft.Data.Sqlite**.


## Features

### Web Application
- User login and logout
- Session-based authentication for the web interface
- Create, view, and delete diary records
- Assign one primary emotion and multiple additional emotions to each record
- Assign tags to records
- Add custom emotions and tags
- Manage personal emotions and tags on a separate page
- View statistics:
  - total number of records
  - records created today / this week / this month
  - emotion frequency
  - top emotions
  - top tags
  - emotion-tag correlation
- Admin page for creating new users

### REST API
- Retrieve records
- Create records
- Retrieve emotions
- Add emotions
- Delete emotions
- Retrieve tags
- Add tags
- Delete tags

### Console Client
- Connects to the REST API
- Authenticates using login + token
- Displays emotions and tags
- Adds new emotions and tags
- Deletes emotions and tags
- Demonstrates API consumption from a separate client application

## Tech Stack

- **Language:** C#
- **Backend:** ASP.NET Core MVC (.NET 7)
- **Frontend:** Razor Views, HTML, CSS, JavaScript
- **Database:** SQLite
- **Data access:** Microsoft.Data.Sqlite
- **API:** REST API
- **Authentication:**
  - Session-based for web pages
  - Token-based for API access
- **Client application:** .NET Console Application

## Project Structure

```text
SoulNotes/
├── ConsoleClient/      # Console client consuming the SoulNotes REST API
├── SoulNotes/          # Main ASP.NET Core MVC web application
│   ├── Controllers/    # MVC controllers and API endpoints
│   ├── Models/         # Domain models and view models
│   ├── Services/       # Database, authentication, and application services
│   ├── Views/          # Razor views
│   ├── wwwroot/        # Static files
│   ├── AppData.db      # SQLite database
│   ├── Program.cs      # Application entry point and middleware configuration
│   └── SoulNotes.csproj
├── SoulNotes.sln       # Solution containing both projects
├── README.md
└── .gitignore
```

## Main Pages
- Login — user authentication
- Diary — create a new journal entry
- Records — browse saved entries
- Statistics — overview of emotional trends
- Emotions & Tags — add or remove custom emotions and tags
- Users — admin-only user management page

## Database Design

The application uses SQLite with the following core tables:
- `users`
- `emotions`
- `tags`
- `moodEntries`
- `emotionEntries`
- `tagEntries`

The schema supports:
- one-to-many relationship between users and records
- many-to-many relationship between records and emotions
- many-to-many relationship between records and tags

## API Endpoints

The project exposes REST API endpoints for working with records, emotions and tags.

Examples:
- `GET /api/records`
- `POST /api/records`
- `GET /api/emotions`
- `POST /api/emotions`
- `DELETE /api/emotions/{id}`
- `GET /api/tags`
- `POST /api/tags`
- `DELETE /api/tags/{id}`

API access requires user login and token validation.

In the current implementation, the token is passed with the request and verified against the value stored for the user in the database.

## How to Run
Requirements:
- `.NET 7 SDK`

### 1. Run the web application
From the solution root:
```shell
dotnet run --project SoulNotes/SoulNotes.csproj
```
The application will start on a local ASP.NET Core address shown in the console.

### 2. Run the console client

Open a second terminal and run:
```shell
dotnet run --project ConsoleClient/ConsoleClient.csproj
```
Make sure the web application is already running before starting the console client.

## Default Data

On first launch, the application initializes the database automatically and creates:
- default users
- default emotions
- default tags

Example default accounts:
- `Playlist / Mercy`
- `admin / admin`

## Notes

This project was created as an educational ASP.NET Core MVC application.
It demonstrates:
- MVC architecture
- Razor-based server-rendered UI
- session handling
- REST API design
- SQLite integration
- form validation
- basic statistics generation
- communication between a web application and a separate console API client


## Possible Improvements

- replace MD5 password hashing with a stronger algorithm such as BCrypt or PBKDF2
- move API authentication to the Authorization header
- split data access into smaller repository/service classes
- improve validation and error handling
- move inline styles into separate CSS files