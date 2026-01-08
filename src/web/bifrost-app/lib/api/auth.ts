import { apiClient } from "./client";
import type { AuthResponse, LoginRequest, RegisterRequest } from "./types";

const TOKEN_KEY = "bifrost_access_token";
const REFRESH_TOKEN_KEY = "bifrost_refresh_token";
const USER_KEY = "bifrost_user";

export class AuthService {
  /**
   * Register a new user account
   */
  async register(request: RegisterRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>("/api/auth/register", request);
    this.storeAuthData(response);
    return response;
  }

  /**
   * Login with email and password
   */
  async login(request: LoginRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>("/api/auth/login", request);
    this.storeAuthData(response);
    return response;
  }

  /**
   * Logout the current user
   */
  async logout(): Promise<void> {
    const token = this.getAccessToken();

    try {
      if (token) {
        await apiClient.post("/api/auth/logout", undefined, token);
      }
    } finally {
      this.clearAuthData();
    }
  }

  /**
   * Store authentication data in localStorage and cookies
   */
  private storeAuthData(authResponse: AuthResponse): void {
    if (typeof window !== "undefined") {
      // Store in localStorage for client-side access
      localStorage.setItem(TOKEN_KEY, authResponse.accessToken);
      localStorage.setItem(REFRESH_TOKEN_KEY, authResponse.refreshToken);
      localStorage.setItem(USER_KEY, JSON.stringify(authResponse.user));

      // Also store in cookies for server-side access
      document.cookie = `bifrost_access_token=${authResponse.accessToken}; path=/; max-age=${authResponse.expiresIn}; SameSite=Lax`;
      document.cookie = `bifrost_refresh_token=${authResponse.refreshToken}; path=/; max-age=${authResponse.expiresIn * 2}; SameSite=Lax`;
      document.cookie = `bifrost_user=${encodeURIComponent(JSON.stringify(authResponse.user))}; path=/; max-age=${authResponse.expiresIn}; SameSite=Lax`;
    }
  }

  /**
   * Clear authentication data from localStorage and cookies
   */
  private clearAuthData(): void {
    if (typeof window !== "undefined") {
      // Clear localStorage
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
      localStorage.removeItem(USER_KEY);

      // Clear cookies
      document.cookie = "bifrost_access_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
      document.cookie = "bifrost_refresh_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
      document.cookie = "bifrost_user=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
    }
  }

  /**
   * Get the access token from localStorage
   */
  getAccessToken(): string | null {
    if (typeof window !== "undefined") {
      return localStorage.getItem(TOKEN_KEY);
    }
    return null;
  }

  /**
   * Get the refresh token from localStorage
   */
  getRefreshToken(): string | null {
    if (typeof window !== "undefined") {
      return localStorage.getItem(REFRESH_TOKEN_KEY);
    }
    return null;
  }

  /**
   * Get the current user from localStorage
   */
  getCurrentUser(): AuthResponse["user"] | null {
    if (typeof window !== "undefined") {
      const userJson = localStorage.getItem(USER_KEY);
      if (userJson) {
        try {
          return JSON.parse(userJson);
        } catch {
          return null;
        }
      }
    }
    return null;
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.getAccessToken() !== null;
  }
}

export const authService = new AuthService();
