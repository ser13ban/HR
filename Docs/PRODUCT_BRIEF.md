# HR Employee Profile Application - Product Brief

## Project Overview

A comprehensive single-page HR application designed to manage employee profiles, absence requests, and peer feedback within an organization. The system provides role-based access control allowing managers, employees, and co-workers to interact with employee data according to their permissions and responsibilities.

## Target Audience

- **Primary Users**: managers and employees
- **Organization Size**: Small to medium-sized companies requiring streamlined HR processes

## Primary Benefits & Features

### Core Functionality
- **Employee Profile Management**: Centralized employee data with role-based access controls
- **Absence Request System**: Streamlined absence request and approval workflow
- **Peer Feedback Platform**: AI-enhanced feedback system for professional development
- **Manager Dashboard**: Administrative controls for employee data and absence approvals

### Key Features by Role

#### Managers
- Full access to all employee data
- Ability to edit any employee profile
- Absence request approval workflow
- Comprehensive view of team absence patterns

#### Employees (Profile Owners)
- Complete access to personal profile data
- Ability to edit personal information
- Request and track absence history
- View all received feedback
- Give feedback to co-workers
- see non-sensitive data on co-workers profile
- see approved requeest of leave of co-workers



## High-Level Tech/Architecture

### Frontend
- **Framework**: Angular (Single Page Application)
- **Styling**: SASS for maintainable CSS architecture
- **State Management**: Angular Signals for reactive state handling
- **UI Components**: Standalone components with composition patterns

### Backend
- **Framework**: ASP.NET Core (.NET 9)
- **API Design**: RESTful API with proper HTTP status codes
- **Authentication**: JWT-based authentication system
- **Authorization**: Role-based access control (RBAC)

### Database
- **Primary Database**: MySQL
- **ORM**: Entity Framework Core
- **Data Models**: Employee profiles, absence requests, feedback records

### External Services
- **AI Enhancement**: HuggingFace API integration for feedback polishing
- **Authentication**: JWT token-based security

### Architecture Patterns
- **Frontend**: Component-based architecture with standalone components
- **Backend**: Clean architecture with separation of concerns
- **Data Access**: Repository pattern with Entity Framework Core
- **API**: RESTful design with proper error handling and validation

## Application Structure

### Pages & Navigation
1. **Main-page Page**
   - link to my profile
   - table view list to all co-workers

2. **Profile Page** (3 subtabs)
   - Personal Info: View/edit personal data
        - when manager or own the profile you can edit personal data
        - otherwise you can only see non-sensitive data
   - Absence: 
        - for own profile you can reqeust new absence and see previous requests
        - as a manager you can approve requests of employees
        - as a co-worker you can see all approved reqeusts
   - Feedback: 
        - for own profile you can see all you feedback
        - for co-worker profile you can leave feedback



### Data Security
- Sensitive data protection based on user roles
- Co-worker access limited to non-sensitive information
- Manager override capabilities for administrative functions
