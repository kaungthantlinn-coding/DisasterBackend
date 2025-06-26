# Frontend Integration Guide

This guide provides comprehensive instructions for integrating a React TypeScript frontend with the DisasterApp authentication API.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Project Setup](#project-setup)
- [Dependencies](#dependencies)
- [Project Structure](#project-structure)
- [Environment Configuration](#environment-configuration)
- [API Client Setup](#api-client-setup)
- [Type Definitions](#type-definitions)
- [Authentication Store (Zustand)](#authentication-store-zustand)
- [API Functions](#api-functions)
- [Custom Hooks (TanStack Query)](#custom-hooks-tanstack-query)
- [React Components](#react-components)
- [App Configuration](#app-configuration)
- [Google OAuth Setup](#google-oauth-setup)
- [Security Best Practices](#security-best-practices)
- [Testing](#testing)
- [Deployment](#deployment)

## Prerequisites

- Node.js 18+ and npm/yarn
- Basic knowledge of React, TypeScript, and modern JavaScript
- Understanding of JWT authentication
- Google Cloud Console account (for OAuth)

## Project Setup

```bash
# Create a new React TypeScript project
npx create-react-app disaster-app-frontend --template typescript
cd disaster-app-frontend

# Or using Vite (recommended for better performance)
npm create vite@latest disaster-app-frontend -- --template react-ts
cd disaster-app-frontend
npm install
```

## Dependencies

Install the required packages:

```bash
npm install axios @tanstack/react-query zustand
npm install @types/google.accounts

# Optional: UI libraries
npm install @mui/material @emotion/react @emotion/styled
# OR
npm install tailwindcss @headlessui/react
```

## Project Structure

```
src/
├── components/
│   ├── auth/
│   │   ├── LoginForm.tsx
│   │   ├── SignupForm.tsx
│   │   ├── GoogleLoginButton.tsx
│   │   └── ProtectedRoute.tsx
│   └── common/
├── hooks/
│   ├── useAuth.ts
│   ├── useLogin.ts
│   ├── useSignup.ts
│   └── useGoogleLogin.ts
├── services/
│   ├── api.ts
│   ├── auth.ts
│   └── types.ts
├── store/
│   └── authStore.ts
├── utils/
│   ├── constants.ts
│   └── helpers.ts
├── App.tsx
└── main.tsx
```

## Environment Configuration

Create `.env` file in your project root:

```env
# API Configuration
VITE_API_BASE_URL=http://localhost:5057/api
VITE_GOOGLE_CLIENT_ID=153648153927-208kh1b7vt3tfgid4eec9fsqmln9p2ie.apps.googleusercontent.com

# Development settings
VITE_NODE_ENV=development
```

## API Client Setup

### `src/services/api.ts`

```typescript
import axios, { AxiosResponse, InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../store/authStore';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5057/api';

// Create axios instance
export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = useAuthStore.getState().accessToken;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for token refresh
apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error) => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      const refreshToken = useAuthStore.getState().refreshToken;
      if (refreshToken) {
        try {
          const response = await axios.post(`${API_BASE_URL}/Auth/refresh`, {
            refreshToken,
          });
          
          const { accessToken, refreshToken: newRefreshToken } = response.data;
          useAuthStore.getState().setTokens(accessToken, newRefreshToken);
          
          // Retry original request with new token
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return apiClient(originalRequest);
        } catch (refreshError) {
          useAuthStore.getState().logout();
          window.location.href = '/login';
        }
      } else {
        useAuthStore.getState().logout();
        window.location.href = '/login';
      }
    }
    
    return Promise.reject(error);
  }
);

export default apiClient;
```

## Type Definitions

### `src/services/types.ts`

```typescript
// User types
export interface User {
  userId: string;
  name: string;
  email: string;
  photoUrl?: string;
  roles: string[];
}

// Authentication request types
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface SignupRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  agreeToTerms: boolean;
}

export interface GoogleLoginRequest {
  idToken: string;
  deviceInfo?: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// Authentication response types
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

// API response wrapper
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

// Error types
export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  statusCode: number;
}
```

## Authentication Store (Zustand)

### `src/store/authStore.ts`

```typescript
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { User } from '../services/types';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

interface AuthActions {
  setUser: (user: User) => void;
  setTokens: (accessToken: string, refreshToken: string) => void;
  setAuth: (user: User, accessToken: string, refreshToken: string) => void;
  logout: () => void;
  setLoading: (loading: boolean) => void;
}

type AuthStore = AuthState & AuthActions;

export const useAuthStore = create<AuthStore>()(n  persist(
    (set, get) => ({
      // State
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      isLoading: false,

      // Actions
      setUser: (user) => set({ user }),
      
      setTokens: (accessToken, refreshToken) => 
        set({ 
          accessToken, 
          refreshToken,
          isAuthenticated: !!accessToken 
        }),
      
      setAuth: (user, accessToken, refreshToken) => 
        set({ 
          user, 
          accessToken, 
          refreshToken, 
          isAuthenticated: true 
        }),
      
      logout: () => 
        set({ 
          user: null, 
          accessToken: null, 
          refreshToken: null, 
          isAuthenticated: false 
        }),
      
      setLoading: (isLoading) => set({ isLoading }),
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
```

## API Functions

### `src/services/auth.ts`

```typescript
import apiClient from './api';
import {
  LoginRequest,
  SignupRequest,
  GoogleLoginRequest,
  RefreshTokenRequest,
  AuthResponse,
  User,
} from './types';

// Authentication API functions
export const authApi = {
  // Login with email/password
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await apiClient.post('/Auth/login', data);
    return response.data;
  },

  // Signup with email/password
  signup: async (data: SignupRequest): Promise<AuthResponse> => {
    const response = await apiClient.post('/Auth/signup', data);
    return response.data;
  },

  // Google OAuth login
  googleLogin: async (data: GoogleLoginRequest): Promise<AuthResponse> => {
    const response = await apiClient.post('/Auth/google-login', data);
    return response.data;
  },

  // Refresh access token
  refreshToken: async (data: RefreshTokenRequest): Promise<AuthResponse> => {
    const response = await apiClient.post('/Auth/refresh', data);
    return response.data;
  },

  // Logout
  logout: async (refreshToken: string): Promise<boolean> => {
    const response = await apiClient.post('/Auth/logout', { refreshToken });
    return response.data;
  },

  // Get current user
  getCurrentUser: async (): Promise<User> => {
    const response = await apiClient.get('/Auth/me');
    return response.data;
  },

  // Validate token
  validateToken: async (token: string): Promise<boolean> => {
    const response = await apiClient.post('/Auth/validate', { token });
    return response.data;
  },

  // Get Google Client ID
  getGoogleClientId: async (): Promise<string> => {
    const response = await apiClient.get('/Config/google-client-id');
    return response.data;
  },
};
```

## Custom Hooks (TanStack Query)

### `src/hooks/useAuth.ts`

```typescript
import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '../store/authStore';
import { authApi } from '../services/auth';

export const useAuth = () => {
  const { user, isAuthenticated, logout } = useAuthStore();

  const { data: currentUser, isLoading } = useQuery({
    queryKey: ['currentUser'],
    queryFn: authApi.getCurrentUser,
    enabled: isAuthenticated && !user,
    retry: false,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  return {
    user: user || currentUser,
    isAuthenticated,
    isLoading,
    logout,
  };
};
```

### `src/hooks/useLogin.ts`

```typescript
import { useMutation } from '@tanstack/react-query';
import { useAuthStore } from '../store/authStore';
import { authApi } from '../services/auth';
import { LoginRequest } from '../services/types';

export const useLogin = () => {
  const setAuth = useAuthStore((state) => state.setAuth);

  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (response) => {
      setAuth(response.user, response.accessToken, response.refreshToken);
    },
    onError: (error: any) => {
      console.error('Login failed:', error.response?.data?.message || error.message);
    },
  });
};
```

### `src/hooks/useSignup.ts`

```typescript
import { useMutation } from '@tanstack/react-query';
import { useAuthStore } from '../store/authStore';
import { authApi } from '../services/auth';
import { SignupRequest } from '../services/types';

export const useSignup = () => {
  const setAuth = useAuthStore((state) => state.setAuth);

  return useMutation({
    mutationFn: (data: SignupRequest) => authApi.signup(data),
    onSuccess: (response) => {
      setAuth(response.user, response.accessToken, response.refreshToken);
    },
    onError: (error: any) => {
      console.error('Signup failed:', error.response?.data?.message || error.message);
    },
  });
};
```

### `src/hooks/useGoogleLogin.ts`

```typescript
import { useMutation, useQuery } from '@tanstack/react-query';
import { useAuthStore } from '../store/authStore';
import { authApi } from '../services/auth';
import { GoogleLoginRequest } from '../services/types';

export const useGoogleLogin = () => {
  const setAuth = useAuthStore((state) => state.setAuth);

  const googleLoginMutation = useMutation({
    mutationFn: (data: GoogleLoginRequest) => authApi.googleLogin(data),
    onSuccess: (response) => {
      setAuth(response.user, response.accessToken, response.refreshToken);
    },
    onError: (error: any) => {
      console.error('Google login failed:', error.response?.data?.message || error.message);
    },
  });

  return googleLoginMutation;
};

// Hook to get Google Client ID
export const useGoogleClientId = () => {
  return useQuery({
    queryKey: ['googleClientId'],
    queryFn: authApi.getGoogleClientId,
    staleTime: Infinity, // Client ID rarely changes
    cacheTime: Infinity,
  });
};
```

## React Components

### `src/components/auth/LoginForm.tsx`

```typescript
import React, { useState } from 'react';
import { useLogin } from '../../hooks/useLogin';
import { LoginRequest } from '../../services/types';

const LoginForm: React.FC = () => {
  const [formData, setFormData] = useState<LoginRequest>({
    email: '',
    password: '',
    rememberMe: false,
  });

  const loginMutation = useLogin();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    loginMutation.mutate(formData);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="email" className="block text-sm font-medium text-gray-700">
          Email
        </label>
        <input
          type="email"
          id="email"
          name="email"
          value={formData.email}
          onChange={handleChange}
          required
          className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      <div>
        <label htmlFor="password" className="block text-sm font-medium text-gray-700">
          Password
        </label>
        <input
          type="password"
          id="password"
          name="password"
          value={formData.password}
          onChange={handleChange}
          required
          minLength={6}
          className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      <div className="flex items-center">
        <input
          type="checkbox"
          id="rememberMe"
          name="rememberMe"
          checked={formData.rememberMe}
          onChange={handleChange}
          className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
        <label htmlFor="rememberMe" className="ml-2 block text-sm text-gray-900">
          Remember me
        </label>
      </div>

      {loginMutation.error && (
        <div className="text-red-600 text-sm">
          {(loginMutation.error as any)?.response?.data?.message || 'Login failed'}
        </div>
      )}

      <button
        type="submit"
        disabled={loginMutation.isPending}
        className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
      >
        {loginMutation.isPending ? 'Signing in...' : 'Sign in'}
      </button>
    </form>
  );
};

export default LoginForm;
```

### `src/components/auth/GoogleLoginButton.tsx`

```typescript
import React, { useEffect } from 'react';
import { useGoogleLogin, useGoogleClientId } from '../../hooks/useGoogleLogin';

declare global {
  interface Window {
    google: any;
  }
}

const GoogleLoginButton: React.FC = () => {
  const googleLoginMutation = useGoogleLogin();
  const { data: clientId, isLoading: isLoadingClientId } = useGoogleClientId();

  useEffect(() => {
    if (!clientId) return;

    // Load Google Sign-In script
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    document.head.appendChild(script);

    script.onload = () => {
      if (window.google) {
        window.google.accounts.id.initialize({
          client_id: clientId,
          callback: handleGoogleResponse,
        });

        window.google.accounts.id.renderButton(
          document.getElementById('google-signin-button'),
          {
            theme: 'outline',
            size: 'large',
            width: '100%',
          }
        );
      }
    };

    return () => {
      document.head.removeChild(script);
    };
  }, [clientId]);

  const handleGoogleResponse = (response: any) => {
    if (response.credential) {
      googleLoginMutation.mutate({
        idToken: response.credential,
        deviceInfo: navigator.userAgent,
      });
    }
  };

  if (isLoadingClientId) {
    return <div className="animate-pulse bg-gray-200 h-12 rounded"></div>;
  }

  return (
    <div className="w-full">
      <div id="google-signin-button" className="w-full"></div>
      {googleLoginMutation.error && (
        <div className="mt-2 text-red-600 text-sm">
          {(googleLoginMutation.error as any)?.response?.data?.message || 'Google login failed'}
        </div>
      )}
    </div>
  );
};

export default GoogleLoginButton;
```

### `src/components/auth/ProtectedRoute.tsx`

```typescript
import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ 
  children, 
  requiredRoles = [] 
}) => {
  const { isAuthenticated, user, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requiredRoles.length > 0 && user) {
    const hasRequiredRole = requiredRoles.some(role => 
      user.roles.includes(role)
    );
    
    if (!hasRequiredRole) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  return <>{children}</>;
};

export default ProtectedRoute;
```

## App Configuration

### `src/App.tsx`

```typescript
import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import LoginForm from './components/auth/LoginForm';
import GoogleLoginButton from './components/auth/GoogleLoginButton';
import ProtectedRoute from './components/auth/ProtectedRoute';
import { useAuth } from './hooks/useAuth';

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

// Login Page Component
const LoginPage: React.FC = () => {
  const { isAuthenticated } = useAuth();

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Sign in to DisasterApp
          </h2>
        </div>
        <div className="space-y-4">
          <LoginForm />
          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-300" />
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-2 bg-gray-50 text-gray-500">Or continue with</span>
            </div>
          </div>
          <GoogleLoginButton />
        </div>
      </div>
    </div>
  );
};

// Dashboard Component (placeholder)
const Dashboard: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-semibold">DisasterApp</h1>
            </div>
            <div className="flex items-center space-x-4">
              <span>Welcome, {user?.name}</span>
              <button
                onClick={logout}
                className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </nav>
      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <h2 className="text-2xl font-bold text-gray-900">Dashboard</h2>
          <p className="mt-2 text-gray-600">Welcome to the DisasterApp dashboard!</p>
        </div>
      </main>
    </div>
  );
};

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <div className="App">
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route 
              path="/dashboard" 
              element={
                <ProtectedRoute>
                  <Dashboard />
                </ProtectedRoute>
              } 
            />
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </div>
      </Router>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

export default App;
```

## Google OAuth Setup

### Google Cloud Console Configuration

1. **Go to Google Cloud Console**
   - Visit [Google Cloud Console](https://console.cloud.google.com/)
   - Create a new project or select existing one

2. **Enable Google+ API**
   - Navigate to "APIs & Services" > "Library"
   - Search for "Google+ API" and enable it

3. **Create OAuth 2.0 Credentials**
   - Go to "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "OAuth 2.0 Client IDs"
   - Choose "Web application"

4. **Configure Authorized Origins**
   ```
   Development:
   http://localhost:3000
   http://localhost:5173  (if using Vite)
   
   Production:
   https://yourdomain.com
   ```

5. **Copy Client ID**
   - Use the generated Client ID in your `.env` file
   - The Client ID should match: `153648153927-208kh1b7vt3tfgid4eec9fsqmln9p2ie.apps.googleusercontent.com`

### HTML Meta Tags (Optional)

Add to your `public/index.html`:

```html
<meta name="google-signin-client_id" content="153648153927-208kh1b7vt3tfgid4eec9fsqmln9p2ie.apps.googleusercontent.com">
```

## Security Best Practices

### 1. Token Storage
- Access tokens are stored in memory (Zustand store)
- Refresh tokens are persisted in localStorage with encryption
- Automatic token refresh on API calls

### 2. API Security
- HTTPS in production
- CORS properly configured
- Request/response interceptors for error handling
- Automatic logout on token expiration

### 3. Environment Variables
- Never commit `.env` files to version control
- Use different configurations for development/production
- Validate environment variables on app startup

### 4. Error Handling
- Graceful error handling for network failures
- User-friendly error messages
- Proper logging for debugging

## Testing

### Setup Testing Environment

```bash
npm install --save-dev @testing-library/react @testing-library/jest-dom @testing-library/user-event
npm install --save-dev msw  # For API mocking
```

### Example Test

```typescript
// src/components/auth/__tests__/LoginForm.test.tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import LoginForm from '../LoginForm';

const createTestQueryClient = () => new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false },
  },
});

const renderWithProviders = (component: React.ReactElement) => {
  const queryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      {component}
    </QueryClientProvider>
  );
};

describe('LoginForm', () => {
  it('renders login form fields', () => {
    renderWithProviders(<LoginForm />);
    
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  it('validates required fields', async () => {
    renderWithProviders(<LoginForm />);
    
    const submitButton = screen.getByRole('button', { name: /sign in/i });
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByLabelText(/email/i)).toBeInvalid();
    });
  });
});
```

## Deployment

### Build for Production

```bash
# Build the application
npm run build

# Preview the build locally
npm run preview
```

### Environment Variables for Production

```env
# Production .env
VITE_API_BASE_URL=https://api.yourdomain.com/api
VITE_GOOGLE_CLIENT_ID=your-production-google-client-id
VITE_NODE_ENV=production
```

### Deployment Checklist

- [ ] Update API base URL for production
- [ ] Configure Google OAuth for production domain
- [ ] Set up HTTPS
- [ ] Configure CORS on backend for production domain
- [ ] Test authentication flow in production
- [ ] Set up error monitoring (Sentry, LogRocket, etc.)
- [ ] Configure CDN for static assets

## Troubleshooting

### Common Issues

1. **CORS Errors**
   - Ensure backend CORS is configured for your frontend domain
   - Check that API base URL is correct

2. **Google Login Not Working**
   - Verify Google Client ID is correct
   - Check that domain is added to authorized origins
   - Ensure Google Sign-In script is loaded

3. **Token Refresh Issues**
   - Check that refresh token is being stored properly
   - Verify backend refresh endpoint is working
   - Ensure token expiration times are correct

4. **Build Errors**
   - Check TypeScript types are correct
   - Ensure all dependencies are installed
   - Verify environment variables are set

### Debug Mode

Enable React Query DevTools in development:

```typescript
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

// Add to your App component
<ReactQueryDevtools initialIsOpen={false} />
```

## Additional Resources

- [React Documentation](https://react.dev/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [TanStack Query Documentation](https://tanstack.com/query/latest)
- [Zustand Documentation](https://github.com/pmndrs/zustand)
- [Axios Documentation](https://axios-http.com/docs/intro)
- [Google Sign-In for Web](https://developers.google.com/identity/gsi/web)

---

**Note**: This guide assumes you're using the DisasterApp backend API running on `http://localhost:5057`. Make sure to update the API base URL and Google Client ID according to your specific setup.