# DisasterApp Setup Guide

## Configuration Setup

### 1. Create appsettings.json

Copy `appsettings.example.json` to `appsettings.json` in the `DisasterApp.WebApi` directory:

```bash
cp DisasterApp.WebApi/appsettings.example.json DisasterApp.WebApi/appsettings.json
```

### 2. Configure Secrets

Update the following values in your `appsettings.json`:

#### JWT Configuration
- Replace `YOUR_JWT_SECRET_KEY_HERE` with a strong secret key (minimum 32 characters)
- Example: `"DisasterAppSecretKeyForJWTTokenGeneration2024!@#$%^&*()"`

#### Google OAuth Configuration
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Google+ API
4. Create OAuth 2.0 Client ID credentials
5. Add authorized origins:
   - `http://localhost:3000` (for React development)
   - `http://localhost:5173` (for Vite development)
   - `http://localhost:5057` (for API)
6. Replace `YOUR_GOOGLE_CLIENT_ID_HERE` and `YOUR_GOOGLE_CLIENT_SECRET_HERE` with your credentials

#### Database Configuration
The default configuration uses Windows Authentication with SQL Server. Modify the connection string if needed:

```json
"DefaultConnection": "Server=.;Database=Disaster;Integrated Security=true;TrustServerCertificate=true"
```

### 3. Environment Variables (Alternative)

Instead of modifying `appsettings.json`, you can use environment variables:

```bash
export JWT__KEY="your-jwt-secret-key"
export GOOGLEAUTH__CLIENTID="your-google-client-id"
export GOOGLEAUTH__CLIENTSECRET="your-google-client-secret"
```

### 4. User Secrets (Recommended for Development)

For development, use .NET User Secrets:

```bash
cd DisasterApp.WebApi
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "your-jwt-secret-key"
dotnet user-secrets set "GoogleAuth:ClientId" "your-google-client-id"
dotnet user-secrets set "GoogleAuth:ClientSecret" "your-google-client-secret"
```

## Running the Application

1. Ensure you have .NET 9.0 SDK installed
2. Navigate to the project directory
3. Run the application:

```bash
cd DisasterApp.WebApi
dotnet run
```

The API will be available at `http://localhost:5057`

## Security Notes

- Never commit `appsettings.json` with real secrets to version control
- Use User Secrets for development
- Use environment variables or Azure Key Vault for production
- The `appsettings.json` file is now in `.gitignore` to prevent accidental commits