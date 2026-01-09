# 🌈 Bifrost: Your Job Search Companion

Welcome to **Bifrost**! 👋

Job hunting can feel overwhelming. Between juggling spreadsheets, sticky notes, and endless email threads, it's easy to lose track of opportunities or forget important details from that interview last week.

**Bifrost** is here to bring order to the chaos. Think of it as your personal assistant for job hunting – it organizes everything in one place so you can focus on what really matters: landing your next great opportunity! 🚀

## ✨ What Makes Bifrost Special?

Bifrost helps you stay organized throughout your entire job search journey:

* **🎯 Save Jobs You Love**: Bookmark interesting positions with all the details – company name, job title, location, and the link to apply. Never lose track of "that perfect job" again!

* **📊 Track Your Applications**: Watch your progress unfold. See at a glance which applications are pending, which ones led to interviews, and celebrate those offers!

* **💭 Capture Your Thoughts**: Add notes after phone screens, jot down interview questions you were asked, or remember that great question you want to ask the hiring manager.

* **⚙️ Set Your Goals**: Define what you're looking for (remote work? relocation support? salary range?) and keep your search focused on what matters to you.

* **🔒 Keep It Private**: Your data stays yours. Bifrost is designed with privacy in mind – your job search is personal, and we keep it that way.

Think of Bifrost as the notebook you wish you had for your last job search, but better organized and always available! 📓✨

## 🏗️ Technical Architecture

Bifrost is built with a modern, clean architecture using:

* **Backend**: .NET 9 with ASP.NET Core - Clean Architecture with layered domain separation
* **Database**: PostgreSQL 16 (running in Docker)
* **Frontend**: Next.js with TypeScript
* **Authentication**: Supabase Auth with JWT tokens
* **API Documentation**: Scalar interactive API reference
* **Testing**: xUnit, FluentAssertions, NSubstitute with 4 test suites (Unit, Integration, Infrastructure, API)

### Project Structure

```
src/
├── Bifrost.Api/              # ASP.NET Core API (Entry point)
├── Bifrost.Core/             # Domain models, services, interfaces
├── Bifrost.Contracts/        # Request/Response DTOs
├── Bifrost.Infrastructure/   # EF Core, repositories, persistence
└── web/bifrost-app/          # Next.js frontend application

test/
├── Bifrost.Api.Tests/        # API endpoint tests
├── Bifrost.Core.Tests/       # Business logic unit tests
├── Bifrost.Infrastructure.Tests/  # Repository tests
└── Bifrost.Integration.Tests/     # End-to-end integration tests
```

---

## 🚀 Getting Started

Ready to take control of your job search? Here's how to get Bifrost up and running!

### Prerequisites

* **Docker & Docker Compose** – Required to run the database, API, and web app. [Download Docker Desktop](https://www.docker.com/products/docker-desktop/)
* **.NET 9 SDK** (optional) – Only needed if you want to run the API locally outside Docker. [Download .NET 9](https://dotnet.microsoft.com/download/dotnet/9.0)
* **Node.js 18+** (optional) – Only needed for local frontend development. [Download Node.js](https://nodejs.org/)
* **A Supabase account** – Used for secure authentication. [Sign up here](https://supabase.com) (free tier available)
* **A few minutes** – We've made this as simple as possible!

### Quick Start with Docker Compose (Recommended)

#### Step 1: Clone the Repository

```bash
git clone https://github.com/nusbru/Bifrost.git
cd Bifrost
```

#### Step 2: Set Up Your Supabase Account

Bifrost uses Supabase for secure user authentication. Here's how to get your credentials:

1. Go to [supabase.com](https://supabase.com) and sign in (or create a free account)
2. Create a new project (give it any name you like!)
3. Once your project is ready, go to **Project Settings** → **API**
4. You'll see two important values:
   * **Project URL** (looks like `https://xxxxx.supabase.co`)
   * **anon/public key** (a long string of characters - this is your publishable key)

#### Step 3: Start All Services

Launch the entire application stack with one command:

```bash
docker-compose up -d
```

This starts three services:
* 🗄️ **PostgreSQL** (port 5432) - Database
* 🔧 **API** (port 5037) - .NET 9 backend
* 🌐 **Web** (port 3000) - Next.js frontend

Wait about 30 seconds for all services to initialize and for the database to be ready.

#### Step 4: Access the Application

* 🌐 **Web Dashboard**: http://localhost:3000
* 📚 **API Documentation**: http://localhost:5037/docs (Scalar interactive reference)

Create your account on the web dashboard and start organizing your job search!

### Local Development Setup (Alternative)

If you prefer to run services individually for development:

#### Running the API Locally

1. **Start PostgreSQL**:
   ```bash
   docker-compose up postgres -d
   ```

2. **Run the API**:
   ```bash
   cd src/Bifrost.Api
   dotnet run
   ```

   The API will be available at:
   - HTTP: http://localhost:5037
   - HTTPS: https://localhost:7193
   - Docs: http://localhost:5037/docs

#### Running the Frontend Locally

1. **Install dependencies**:
   ```bash
   cd src/web/bifrost-app
   npm install
   ```

2. **Start the dev server**:
Bifrost has comprehensive test coverage across all layers:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test test/Bifrost.Core.Tests
dotnet test test/Bifrost.Integration.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

Test suites available:
- **Unit Tests** (`Bifrost.Core.Tests`) - Business logic with mocked dependencies
- **Infrastructure Tests** (`Bifrost.Infrastructure.Tests`) - Repository implementations
- **API Tests** (`Bifrost.Api.Tests`) - Endpoint handlers
- **Integration Tests** (`Bifrost.Integration.Tests`) - End-to-end HTTP flows with in-memory database## 🔧 Configuration

### Database Connection

The default PostgreSQL connection is configured in [appsettings.Development.json](src/Bifrost.Api/appsettings.Development.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=BifrostDB;Username=bifrost_user;Password=bifrost_pass;Port=5432"
  }
}
```

### API Endpoints

All endpoints are grouped under `/api/{resource}` and include:

- `/api/auth` - User registration and authentication
- `/api/jobs` - Job postings management
- `/api/job-applications` - Application tracking
- `/api/application-notes` - Interview notes and observations
- `/api/preferences` - User preferences and goals

### Authentication

Bifrost uses JWT token authentication via Supabase:
- Tokens are validated against Supabase's JWT issuer
- All endpoints (except auth) require a valid JWT token
- User isolation via `SupabaseUserId` ensures data privacy

## 📊 Database Schema

The application uses Entity Framework Core 9 with PostgreSQL 16. Key entities:

- **Job** - Saved job postings with company details
- **JobApplication** - Application status tracking (one-to-many with Job)
- **ApplicationNote** - Interview notes and observations
- **Preferences** - User preferences for job search criteria

All entities inherit from a base `Entity` class with:
- `Id` (Guid) - Primary key
- `SupabaseUserId` (Guid) - Multi-tenant isolation
- `CreatedAt` / `UpdatedAt` - Timestamps

## 🛠️ Development

### Database Migrations

Entity Framework migrations are located in `src/Bifrost.Infrastructure/Persistence/Migrations/`.

To create a new migration:

```bash
cd src/Bifrost.Api
dotnet ef migrations add MigrationName --project ../Bifrost.Infrastructure
```

To apply migrations:

```bash
dotnet ef database update --project ../Bifrost.Infrastructure
```

### Code Structure

The project follows Clean Architecture principles:

1. **Core Layer** (`Bifrost.Core`) - Domain models, interfaces, business logic
2. **Infrastructure Layer** (`Bifrost.Infrastructure`) - EF Core, repositories, persistence
3. **Contracts Layer** (`Bifrost.Contracts`) - DTOs for API requests/responses
4. **API Layer** (`Bifrost.Api`) - ASP.NET Core endpoints, DI configuration

### Testing Patterns

- **Arrange-Act-Assert** pattern for all tests
- **FluentAssertions** for readable assertions
- **NSubstitute** for mocking interfaces
- **In-memory database** for integration tests (no Docker required)

## 🚢 Deployment

The application is containerized and ready for deployment. The [compose.yaml](compose.yaml) file includes production-ready configurations for all services.

### Docker Images

- API: Built from `src/Bifrost.Api/Dockerfile`
- Web: Built from `src/web/bifrost-app/Dockerfile`
- Database: PostgreSQL 16 Alpine

## 🐛 Troubleshooting

**Database connection failed:**
- Ensure PostgreSQL is running: `docker-compose ps`
- Check logs: `docker-compose logs postgres`

**API not responding:**
- Check API logs: `docker-compose logs api`
- Verify API is healthy: `curl http://localhost:5037/docs`

**Frontend can't connect to API:**
- Verify CORS settings in [Program.cs](src/Bifrost.Api/Program.cs)
- Check browser console for errors

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Write tests for your changes
4. Ensure all tests pass (`dotnet test`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request



## 🌟 License

Bifrost is open source and available under the MIT License. Feel free to use it, modify it, and make it your own!

---

**Happy job hunting! May your pipeline be full and your offers be plenty!** 🎯✨
