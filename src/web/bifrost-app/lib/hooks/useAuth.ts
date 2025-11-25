/**
 * Authentication Hook
 * Manages authentication state and redirects
 * Follows Single Responsibility Principle
 */

"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { createClient } from "@/lib/supabase/client";
import { User } from "@supabase/supabase-js";
import { ROUTES } from "@/lib/constants";

interface UseAuthReturn {
  user: User | null;
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
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    checkAuth();
  }, []);

  async function checkAuth() {
    try {
      const supabase = createClient();
      const {
        data: { user },
      } = await supabase.auth.getUser();

      if (!user) {
        router.push(ROUTES.LOGIN);
        return;
      }

      setUser(user);
    } catch (error) {
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
