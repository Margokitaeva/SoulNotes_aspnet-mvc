# SoulNotes

SoulNotes is a web application for emotion journaling built with **C#**, **ASP.NET Core MVC**, **Razor Views**, and **SQLite**.

The application allows users to create diary records, assign emotions and tags, manage their own emotion/tag lists, and view simple emotional statistics over time.

## Architecture

SoulNotes is built as a server-rendered ASP.NET Core MVC application.
The web interface uses Razor Views and session-based authentication.
The project also exposes a simple REST API secured with login + token validation.
Data is stored in a local SQLite database and accessed through custom C# services using Microsoft.Data.Sqlite.

## Features

- User login and logout
- Session-based authentication for the web interface
- Token-based authentication for REST API access
- Create, view and delete diary records
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

## Tech Stack

- **Backend:** ASP.NET Core MVC (.NET 7)
- **Frontend:** Razor Views, HTML, CSS, JavaScript
- **Database:** SQLite
- **Data access:** `Microsoft.Data.Sqlite`
- **Authentication:**
  - Session-based for web pages
  - Token-based for API endpoints

## Project Structure

```text
SoulNotes/
├── Controllers/         # MVC controllers and API endpoints
├── Models/              # Domain models and view models
├── Services/            # Database, authentication, user, record, emotion, tag, and statistics services
├── Views/               # Razor views
├── wwwroot/             # Static files
├── AppData.db           # SQLite database
├── Program.cs           # Application entry point and middleware configuration
├── SoulNotes.csproj     # .NET project file
└── SoulNotes.sln        # Visual Studio solution
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

API access requires:
- login
- token

## How to Run
Requirements:
- `.NET 7 SDK`

Run locally:
```shell
dotnet restore
dotnet build
dotnet run
```

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

## Possible Improvements

- replace MD5 password hashing with a stronger algorithm such as BCrypt or PBKDF2
- move API authentication to the Authorization header
- split data access into smaller repository/service classes
- improve validation and error handling
- move inline styles into separate CSS files