/**
 * Server Action Hook
 * Generic hook for executing server actions with loading/error states
 * Follows DRY principle and Single Responsibility
 */

"use client";

import { useState, useCallback } from "react";
import { ApiResponse } from "@/lib/types/api";

interface UseServerActionReturn<T> {
  data: T | null;
  isLoading: boolean;
  error: string | null;
  execute: () => Promise<void>;
  reset: () => void;
}

/**
 * Custom hook for executing server actions
 * Manages loading, error, and data states automatically
 *
 * @param action - Server action function that returns ApiResponse<T>
 * @returns Object with data, loading state, error, and execute function
 *
 * @example
 * ```tsx
 * function JobsList() {
 *   const { data: jobs, isLoading, error, execute } = useServerAction(
 *     () => getUserJobsAction()
 *   );
 *
 *   useEffect(() => {
 *     execute();
 *   }, []);
 *
 *   if (isLoading) return <Loading />;
 *   if (error) return <Error message={error} />;
 *
 *   return <JobsTable jobs={jobs} />;
 * }
 * ```
 */
export function useServerAction<T>(
  action: () => Promise<ApiResponse<T>>
): UseServerActionReturn<T> {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const execute = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await action();

      if (response.error) {
        setError(response.error);
        setData(null);
      } else if (response.data) {
        setData(response.data);
        setError(null);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "An unexpected error occurred");
      setData(null);
    } finally {
      setIsLoading(false);
    }
  }, [action]);

  const reset = useCallback(() => {
    setData(null);
    setError(null);
    setIsLoading(false);
  }, []);

  return {
    data,
    isLoading,
    error,
    execute,
    reset,
  };
}
