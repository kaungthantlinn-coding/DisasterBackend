# DisasterApp

A comprehensive disaster management application built with ASP.NET Core Web API and modern frontend technologies.

## Features

- **User Authentication**: Traditional email/password login and Google OAuth 2.0 integration
- **Disaster Reporting**: Report and track disaster incidents
- **Real-time Communication**: Chat functionality for coordination
- **Support Requests**: Request and provide assistance during disasters
- **Location Services**: GPS-based location tracking and mapping
- **Photo Documentation**: Upload and manage disaster-related photos
- **Notifications**: Real-time alerts and updates

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: JWT tokens with Google OAuth 2.0
- **Architecture**: Clean Architecture (Application, Infrastructure, WebApi layers)

### Frontend Options
- **Vanilla JavaScript**: Basic HTML/CSS/JS implementation (included)
- **React TypeScript**: Modern React application with TypeScript support

## Project Structure

```
DisasterApp/
├── DisasterApp.Application/     # Application layer (DTOs, Services, Interfaces)
├── DisasterApp.Infrastructure/   # Infrastructure layer (Data, Repositories, Migrations)
├── DisasterApp.WebApi/          # Web API layer (Controllers, Program.cs)
├── DisasterApp.Tests/           # Unit and integration tests
└── README.md                    # This file
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Google Cloud Console account (for OAuth setup)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd DisasterApp
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   
   Edit `DisasterApp.WebApi/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DisasterAppDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   cd DisasterApp.WebApi
   dotnet ef database update
   ```

5. **Configure Google OAuth (see Google OAuth Setup section below)**

6. **Run the application**
   ```bash
   cd DisasterApp.WebApi
   dotnet run
   ```

   The API will be available at `http://localhost:5057`

## Google OAuth 2.0 Integration

### Backend Setup

The application includes complete Google OAuth 2.0 integration with the following components:

#### 1. NuGet Packages
- `Microsoft.AspNetCore.Authentication.Google` - ASP.NET Core Google authentication
- `Google.Apis.Auth` - Google ID token verification

#### 2. Configuration

Add Google OAuth credentials to `appsettings.json`:

```json
{
  "GoogleAuth": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-google-client-secret"
  }
}
```

#### 3. API Endpoints

- `POST /api/auth/google-login` - Google OAuth login endpoint
- `GET /api/config/google-client-id` - Get Google Client ID for frontend

#### 4. Authentication Flow

1. Frontend receives Google ID token from Google Sign-In
2. Token is sent to `/api/auth/google-login` endpoint
3. Backend verifies token with Google
4. User is created/updated in database
5. JWT access and refresh tokens are returned

### Google Cloud Console Setup

1. **Create a Google Cloud Project**
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create a new project or select existing one

2. **Enable Google+ API**
   - Navigate to "APIs & Services" > "Library"
   - Search for "Google+ API" and enable it

3. **Create OAuth 2.0 Credentials**
   - Go to "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "OAuth 2.0 Client IDs"
   - Choose "Web application"
   - Add authorized origins:
     - `http://localhost:5057` (for development)
     - Your production domain
   - Add authorized redirect URIs (if needed)

4. **Copy Credentials**
   - Copy the Client ID and Client Secret
   - Add them to your `appsettings.json`

### Frontend Integration

#### Vanilla JavaScript (Included)

The project includes a complete vanilla JavaScript implementation:

- **Files**: `DisasterApp.WebApi/wwwroot/index.html`, `script.js`
- **Features**: Google Sign-In buttons, automatic token handling, UI updates
- **Setup**: No additional configuration needed

#### React TypeScript Integration

For React applications, use the following setup:

##### Dependencies

```bash
npm install @tanstack/react-query axios zustand
npm install -D @types/google-one-tap
```

##### Environment Variables

```env
# .env.local
REACT_APP_API_BASE_URL=http://localhost:5057/api
```

##### Key Components

1. **Axios Client with Interceptors**
   ```typescript
   // src/lib/api-client.ts
   import axios from 'axios';
   import { useAuthStore } from '../stores/auth-store';
   
   const apiClient = axios.create({
     baseURL: process.env.REACT_APP_API_BASE_URL,
   });
   
   // Request interceptor for auth token
   apiClient.interceptors.request.use((config) => {
     const token = useAuthStore.getState().accessToken;
     if (token) {
       config.headers.Authorization = `Bearer ${token}`;
     }
     return config;
   });
   
   // Response interceptor for token refresh
   apiClient.interceptors.response.use(
     (response) => response,
     async (error) => {
       if (error.response?.status === 401) {
         const { refreshToken, setTokens, logout } = useAuthStore.getState();
         if (refreshToken) {
           try {
             const response = await axios.post('/auth/refresh', { refreshToken });
             setTokens(response.data.accessToken, response.data.refreshToken);
             return apiClient.request(error.config);
           } catch {
             logout();
           }
         }
       }
       return Promise.reject(error);
     }
   );
   ```

2. **Zustand Auth Store**
   ```typescript
   // src/stores/auth-store.ts
   import { create } from 'zustand';
   import { persist } from 'zustand/middleware';
   
   interface AuthState {
     accessToken: string | null;
     refreshToken: string | null;
     user: User | null;
     isAuthenticated: boolean;
     setTokens: (accessToken: string, refreshToken: string) => void;
     setUser: (user: User) => void;
     logout: () => void;
   }
   
   export const useAuthStore = create<AuthState>()()
     persist(
       (set) => ({
         accessToken: null,
         refreshToken: null,
         user: null,
         isAuthenticated: false,
         setTokens: (accessToken, refreshToken) =>
           set({ accessToken, refreshToken, isAuthenticated: true }),
         setUser: (user) => set({ user }),
         logout: () =>
           set({
             accessToken: null,
             refreshToken: null,
             user: null,
             isAuthenticated: false,
           }),
       }),
       { name: 'auth-storage' }
     )
   );
   ```

3. **Google Login Hook**
   ```typescript
   // src/hooks/use-google-login.ts
   import { useMutation } from '@tanstack/react-query';
   import { googleLogin } from '../api/auth-api';
   import { useAuthStore } from '../stores/auth-store';
   
   export const useGoogleLogin = () => {
     const { setTokens, setUser } = useAuthStore();
   
     return useMutation({
       mutationFn: googleLogin,
       onSuccess: (data) => {
         setTokens(data.accessToken, data.refreshToken);
         setUser(data.user);
       },
     });
   };
   ```

4. **Google Login Component**
   ```typescript
   // src/components/GoogleLoginButton.tsx
   import { useEffect } from 'react';
   import { useGoogleLogin } from '../hooks/use-google-login';
   import { useGoogleClientId } from '../hooks/use-google-client-id';
   
   export const GoogleLoginButton = () => {
     const googleLoginMutation = useGoogleLogin();
     const { data: clientId } = useGoogleClientId();
   
     useEffect(() => {
       if (!clientId) return;
   
       window.google?.accounts.id.initialize({
         client_id: clientId,
         callback: (response) => {
           googleLoginMutation.mutate({
             idToken: response.credential,
             deviceInfo: navigator.userAgent,
           });
         },
       });
   
       window.google?.accounts.id.renderButton(
         document.getElementById('google-signin-button')!,
         {
           theme: 'outline',
           size: 'large',
           width: 300,
         }
       );
     }, [clientId]);
   
     return <div id="google-signin-button" />;
   };
   ```

## API Documentation

### Authentication Endpoints

- `POST /api/auth/login` - Traditional email/password login
- `POST /api/auth/signup` - User registration
- `POST /api/auth/google-login` - Google OAuth login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout user
- `GET /api/auth/me` - Get current user info

### Configuration Endpoints

- `GET /api/config/google-client-id` - Get Google OAuth Client ID

## Security Features

- **JWT Authentication**: Secure token-based authentication
- **Token Refresh**: Automatic token renewal
- **Google OAuth**: Secure third-party authentication
- **Password Hashing**: BCrypt password hashing
- **CORS Configuration**: Proper cross-origin resource sharing
- **Input Validation**: Data annotation validation

## Development

### Running Tests

```bash
dotnet test
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Code Quality

- Follow Clean Architecture principles
- Use dependency injection
- Implement proper error handling
- Add comprehensive logging
- Write unit and integration tests

## Deployment

### Production Configuration

1. **Update connection strings** for production database
2. **Configure Google OAuth** with production domains
3. **Set environment variables** for sensitive data
4. **Enable HTTPS** and update CORS settings
5. **Configure logging** for production monitoring

### Environment Variables

```bash
# Production environment variables
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<production-db-connection>
GoogleAuth__ClientId=<google-client-id>
GoogleAuth__ClientSecret=<google-client-secret>
JwtSettings__SecretKey=<jwt-secret-key>
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:
- Create an issue in the repository
- Check the documentation
- Review the code examples

## Changelog

### v1.0.0
- Initial release
- Basic authentication system
- Google OAuth 2.0 integration
- Disaster reporting functionality
- Real-time chat system
- Location services
- Photo management
- Notification system