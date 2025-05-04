# TaskAspNet

A modern ASP.NET Core web application for task and project management with real-time notifications.


## Project Structure
 
```text
TaskAspNet/
├── TaskAspNet.Business    → Business logic, services, DTOs
├── TaskAspNet.Data        → Entity models, DbContexts, migrations
├── TaskAspNet.Web         → ASP.NET Core MVC app, SignalR, views, controllers
├── TaskAspNet.Tests       → Unit and integration tests
 
```

##  Topology
 
```text
[Client Browser]
       ↓
[TaskAspNet.Web]
   ├── Controllers (API + MVC)
   ├── SignalR Hubs
   └── Views (Razor)
 
↳ Services via DI
       ↓
[TaskAspNet.Business]
   ├── Interfaces
   └── Services
 
↳ Repositories via DI
       ↓
[TaskAspNet.Data]
   ├── AppDbContext (Domain)
   └── User (Identity)
 
```

## Features

- User authentication and authorization using ASP.NET Core Identity
- Real-time notifications using SignalR
- Project management functionality
- Member management system
- Client management
- File handling capabilities
- Role-based access control
- Logging system using Serilog
- Rich-text editor via [Quill.js](https://quilljs.com/)
- Search filtering using [Fuse.js](https://fusejs.io/)
- Dark mode toggle with persistent user preference
- For pagination i used XPagedList package 
- There is a seed file for SuperAdmin(check it out for sign in credentials)

## Technology Stack

- ASP.NET Core
- Entity Framework Core
- SQL Server
- SignalR for real-time communications
- Serilog for logging
- Identity for authentication

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- SQL Server
- Visual Studio 2022 or later
- EF Core CLI tools (if not already installed):

## Getting Started

1. Clone the repository
2. Update the connection strings in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "your_connection_string",
       "IdentityConnection": "your_identity_connection_string"
     }
   }
   ```
3. Run the following commands in the terminal:
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet restore
   dotnet build
   dotnet run --project TaskAspNet.Web
   ```

### Apply Entity Framework Migrations
 
From the root directory, run:
 
```
 
dotnet ef database update --context AppDbContext --project TaskAspNet.Data --startup-project TaskAspNet.Web
dotnet ef database update --context UserDbContext --project TaskAspNet.Data --startup-project TaskAspNet.Web
 
```
 
This initializes both the application and identity databases.

## Project Configuration

The application uses several key configurations:

- Cookie authentication with secure settings
- CORS policy configuration
- SignalR hub configuration
- File logging with daily rotation
- Database contexts for both application and identity

## Security Features

- Secure cookie configuration
- HTTPS enforcement
- Role-based authorization
- Identity-based authentication
- CORS policy implementation

## Logging

The application uses Serilog for logging with the following features:
- Console logging
- File logging with daily rotation
- Log file size limits
- Retention policies

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request
