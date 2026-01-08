/**
 * Authentication Hook
 * Manages authentication state and redirects
 * Follows Single Responsibility Principle
 */

"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { authService } from "@/lib/api/auth";
import type { UserInfo } from "@/lib/api/types";
import { ROUTES } from "@/lib/constants";

interface UseAuthReturn {
  user: UserInfo | null;
  isLoading: boolean;
  isAuthenticated: boolean;
}

/**
 * Custom hook for handling authentication
 * Automatically redirects to login if not authenticated
 *
 * @returns Authentication state and user information
 *
 * @example
 * ```tsx
 * function ProtectedPage() {
 *   const { user, isLoading } = useAuth();
 *
 *   if (isLoading) return <LoadingSkeleton />;
 *
 *   return <div>Welcome {user.email}</div>;
 * }
 * ```
 */
export function useAuth(): UseAuthReturn {
  const router = useRouter();
  const [user, setUser] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    checkAuth();
  }, []);

  async function checkAuth() {
    try {
      const currentUser = authService.getCurrentUser();

      if (!currentUser) {
        router.push(ROUTES.LOGIN);
        return;
      }

      setUser(currentUser);
    } catch (_error) {
      router.push(ROUTES.LOGIN);
    } finally {
      setIsLoading(false);
    }
  }

  return {
    user,
    isLoading,
    isAuthenticated: user !== null,
  };
}
