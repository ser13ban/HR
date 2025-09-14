# HR Management System

A modern HR management system built with Angular 20 (frontend) and ASP.NET Core 9 (backend) for managing employee profiles, absence requests, and feedback.

## 🚀 Instructions to Run the Application

### Prerequisites

#### Required Tools & Versions
- **Node.js**: v18+ (for Angular)
- **npm**: v9+ (comes with Node.js)
- **Angular CLI**: v20.2.1+ (`npm install -g @angular/cli`)
- **.NET SDK**: 9.0+ 
- **MySQL**: 8.0+ (for database)
- **Git**: Latest version

#### Database Setup
1. Install MySQL Server 8.0+
2. Create a database named `HrDatabase_Dev`
3. Ensure MySQL is running on port 3306 with root user (no password for development)

### Running the Application Locally

#### 1. Clone the Repository
```bash
git clone <repository-url>
cd HR
```

#### 2. Backend Setup (.NET API)
```bash
cd HrAPI/HrAPI

# Restore NuGet packages
dotnet restore

# Run database migrations
dotnet ef database update

# Start the API server (runs on https://localhost:5099)
dotnet run
```

The API will be available at:
- **HTTPS**: https://localhost:5099
- **Swagger UI**: https://localhost:5099 (root path)

#### 3. Frontend Setup (Angular)
```bash
cd hr-app

# Install npm dependencies
npm install

# Start the development server (runs on http://localhost:4200)
npm start
```

The Angular app will be available at: **http://localhost:4200**

#### 4. Initial Data
The system uses ASP.NET Identity for user management. You can:
- Register new users through the registration page
- Users are automatically assigned roles (Employee or Manager)
- Sample test data can be created through the API endpoints

### Package Dependencies

#### Backend (.NET)
- **Microsoft.AspNetCore.OpenApi** (9.0.8) - OpenAPI/Swagger support
- **Microsoft.EntityFrameworkCore** (9.0.8) - ORM framework
- **Pomelo.EntityFrameworkCore.MySql** (9.0.0) - MySQL provider
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (9.0.8) - Identity system
- **Microsoft.AspNetCore.Authentication.JwtBearer** (9.0.8) - JWT authentication
- **System.IdentityModel.Tokens.Jwt** (8.1.2) - JWT token handling
- **Swashbuckle.AspNetCore** (7.2.0) - Swagger UI

#### Frontend (Angular)
- **@angular/core** (20.2.0) - Angular framework
- **@angular/common** (20.2.0) - Common Angular utilities
- **@angular/forms** (20.2.0) - Reactive forms
- **@angular/router** (20.2.0) - Routing
- **@angular/ssr** (20.2.1) - Server-side rendering
- **rxjs** (7.8.0) - Reactive programming
- **express** (5.1.0) - SSR server

## 🏗️ Architecture Decisions

### Database Choice: MySQL
**Decision**: Using MySQL as the relational database.

**Reasoning**: 
- The HR domain has well-defined relationships (employees, departments, roles, absence requests, feedback)
- ACID compliance is crucial for HR data integrity
- Strong consistency requirements for employee data and approval workflows
- Mature ecosystem with excellent .NET support through Entity Framework Core
- Familiar SQL querying for complex reporting needs

### Backend Architecture: Controller/Service Pattern
**Decision**: Implemented a layered architecture with Controllers and Services, using Entity Framework Core directly.

**Reasoning**:
- **Service Layer**: Encapsulates business logic and provides a clean separation of concerns
- **Entity Framework Core**: Already implements Repository and Unit of Work patterns internally
- **No Additional Repository Layer**: Avoided over-engineering since EF Core provides sufficient abstraction
- **Dependency Injection**: Leverages .NET's built-in DI container for loose coupling
- **Clean Architecture Principles**: Maintains separation between API, business logic, and data access

### Error Handling Pattern
**Decision**: Centralized error handling with custom authorization service and structured error responses.

**Reasoning**:
- **Global Exception Handling**: Provides consistent error responses across all endpoints
- **Custom Authorization Service**: Implements fine-grained permission checks (RBAC)
- **Structured Error DTOs**: Returns meaningful error messages to the frontend
- **HTTP Status Code Compliance**: Uses appropriate status codes (401, 403, 404, 400, 500)
- **Security**: Prevents sensitive information leakage in error responses

### Authentication: JWT (JSON Web Tokens)
**Decision**: Stateless JWT-based authentication with ASP.NET Identity.

**Reasoning**:
- **Stateless**: No server-side session storage required, enables horizontal scaling
- **Cross-Platform**: Works seamlessly with Angular frontend and any future mobile apps
- **Security**: Uses HS256 algorithm with configurable secret key
- **Claims-Based**: Embeds user ID, email, and role directly in the token
- **ASP.NET Identity Integration**: Leverages Microsoft's mature identity system
- **Token Expiration**: Configurable expiration (24 hours in development)

**JWT Implementation Details**:
```csharp
// Token contains claims:
- NameIdentifier (User ID)
- Email
- Role (Employee/Manager)
- JTI (unique token identifier)
- IAT (issued at timestamp)
```

### Frontend Architecture Decisions

#### Angular Signals for State Management
**Decision**: Using Angular's new Signals system for reactive state management.

**Reasoning**:
- **Performance**: Signals provide fine-grained reactivity without Zone.js overhead
- **Zoneless Change Detection**: Enables `provideZonelessChangeDetection()` for better performance
- **Simplicity**: Eliminates complex RxJS operators for simple state management
- **Type Safety**: Full TypeScript support with compile-time checking
- **Future-Proof**: Angular's recommended approach for state management

**Implementation Example**:
```typescript
// AuthService using signals
private currentUserSignal = signal<User | null>(null);
private isAuthenticatedSignal = signal<boolean>(false);

get currentUser() {
  return this.currentUserSignal.asReadonly();
}

// Component using signals
employee = signal<EmployeeProfile | null>(null);
loading = signal<boolean>(false);
error = signal<string | null>(null);
```

#### Role-Based Access Control (RBAC) on Frontend
**Decision**: Client-side RBAC with server-side validation.

**Reasoning**:
- **User Experience**: Immediate UI feedback without server round-trips
- **Security**: All permissions validated on the backend regardless of frontend state
- **Maintainability**: Centralized permission logic in AuthorizationService
- **Scalability**: Easy to extend with new roles and permissions

**RBAC Implementation**:
```typescript
// Permission checks in components
get canEdit() {
  const currentUser = this.authService.getCurrentUser();
  return currentUser?.role === EmployeeRole.Manager || this.isOwnProfile();
}

// Guard-based route protection
canActivate(): boolean {
  return this.authService.isUserAuthenticated();
}

// Backend validation (always enforced)
await authorizationService.RequireManagerAsync(userId);
```

#### Standalone Components
**Decision**: Using Angular standalone components throughout the application.

**Reasoning**:
- **Reduced Bundle Size**: No unnecessary NgModule overhead
- **Better Tree Shaking**: Only imports what's actually used
- **Simplified Architecture**: Fewer abstraction layers
- **Modern Angular**: Aligns with Angular's future direction

## 🔒 Authentication Flow

1. **User Registration/Login**: Credentials sent to `/api/auth/register` or `/api/auth/login`
2. **Token Generation**: Server validates credentials and generates JWT with user claims
3. **Token Storage**: Frontend stores JWT in localStorage (browser-only with SSR support)
4. **Request Interception**: Auth interceptor adds `Authorization: Bearer <token>` header
5. **Token Validation**: Backend validates JWT signature and claims on each request
6. **Auto Logout**: Frontend automatically logs out users on 401 responses or token expiration

## 🛠️ What Would Be Improved With More Time

### Testing Infrastructure
- **Unit Tests**: Comprehensive test coverage for services and components using Jasmine/Karma
- **Integration Tests**: API endpoint testing with TestServer and in-memory database
- **E2E Tests**: Full user workflow testing with Playwright or Cypress
- **Test Data Management**: Automated test data seeding and cleanup

### Multi-Tenancy Support
- **Team-Based Isolation**: Separate data by teams or organizations
- **Configurable Permissions**: Role-based permissions per team
- **Team Admin Role**: Additional role for team-level administration
- **Data Segregation**: Ensure users only see data from their organization

### AI-Powered Feedback Enhancement
- **Smart Suggestions**: AI-assisted feedback writing with tone analysis
- **Sentiment Analysis**: Automatic categorization of feedback sentiment
- **Performance Insights**: AI-generated insights from feedback patterns
- **Writing Assistant**: Real-time suggestions for constructive feedback

### Enhanced Absence Management
- **Calendar Integration**: Visual calendar view for absence requests and approvals
- **Team Calendar**: View team availability and upcoming absences
- **Conflict Detection**: Automatic detection of overlapping absence requests
- **Integration**: Sync with external calendar systems (Google Calendar, Outlook)
- **Approval Workflows**: Multi-level approval processes for different absence types
- **Automated Notifications**: Email/SMS notifications for requests and approvals

### Additional Improvements
- **Real-time Notifications**: WebSocket-based notifications for approvals and updates
- **Advanced Reporting**: Analytics dashboard with charts and insights
- **Mobile App**: React Native or Flutter mobile application
- **File Uploads**: Profile pictures and document attachments
- **Audit Logging**: Comprehensive audit trail for all user actions
- **Performance Monitoring**: Application performance monitoring and logging
- **Docker Deployment**: Containerized deployment with Docker Compose
- **CI/CD Pipeline**: Automated testing and deployment pipeline

## 📁 Project Structure

```
HR/
├── hr-app/                 # Angular 20 Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/       # Core services, guards, models
│   │   │   ├── features/   # Feature modules (auth, profile, etc.)
│   │   │   └── shared/     # Shared components and services
│   │   └── ...
│   └── ...
├── HrAPI/                  # ASP.NET Core 9 Backend
│   ├── HrAPI/
│   │   ├── Controllers/    # API Controllers
│   │   ├── Services/       # Business logic services
│   │   ├── Models/         # Entity models
│   │   ├── DTOs/           # Data transfer objects
│   │   └── Data/           # Database context
│   └── ...
└── README.md
```
