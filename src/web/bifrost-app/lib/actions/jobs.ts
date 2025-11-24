"use server";

/**
 * Server Actions for Job Management
 *
 * These actions run on the server only and keep API URLs and tokens secure.
 * They follow Next.js 16 Server Actions pattern with proper error handling.
 *
 * SOLID Principles:
 * - Single Responsibility: Each action handles one specific API operation
 * - Dependency Inversion: Actions depend on server config abstraction
 */

import { SERVER_API_URL } from "@/lib/config/server";
import { Job } from "@/lib/types";
import { createClient } from "@/lib/supabase/server";

export interface ApiResponse<T> {
  data?: T;
  error?: string;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

/**
 * Get authentication token from current session
 * @returns JWT token or null if not authenticated
 */
async function getAuthToken(): Promise<string | null> {
  const supabase = await createClient();
  const { data: { session } } = await supabase.auth.getSession();
  return session?.access_token || null;
}

/**
 * Fetches all jobs for the authenticated user
 * Server Action that securely calls the .NET backend API
 *
 * @returns Promise with the list of jobs or error
 */
export async function getUserJobsAction(): Promise<ApiResponse<Job[]>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: "Authentication required" };
    }

    // Get user ID from session
    const supabase = await createClient();
    const { data: { user } } = await supabase.auth.getUser();

    if (!user) {
      return { error: "User not found" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/jobs/user/${user.id}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        cache: "no-store", // Always fetch fresh data
      }
    );

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to fetch jobs: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    console.error("Error in getUserJobsAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}

/**
 * Fetches a single job by ID
 * Server Action that securely calls the .NET backend API
 *
 * @param jobId - The job ID to fetch
 * @returns Promise with the job data or error
 */
export async function getJobByIdAction(jobId: string): Promise<ApiResponse<Job>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: "Authentication required" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/jobs/${jobId}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        cache: "no-store",
      }
    );

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to fetch job: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    console.error("Error in getJobByIdAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}

/**
 * Creates a new job
 * Server Action that securely calls the .NET backend API
 *
 * @param jobData - The job data to create
 * @returns Promise with the created job or error
 */
export async function createJobAction(
  jobData: Omit<Job, "id" | "createdAt" | "updatedAt" | "supabaseUserId">
): Promise<ApiResponse<Job>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: "Authentication required" };
    }

    // Get user ID from session
    const supabase = await createClient();
    const { data: { user } } = await supabase.auth.getUser();

    if (!user) {
      return { error: "User not found" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/jobs`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          ...jobData,
          supabaseUserId: user.id,
        }),
      }
    );

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to create job: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    console.error("Error in createJobAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}

/**
 * Updates an existing job
 * Server Action that securely calls the .NET backend API
 *
 * @param jobId - The job ID to update
 * @param jobData - The updated job data
 * @returns Promise with success/error
 */
export async function updateJobAction(
  jobId: string,
  jobData: Partial<Omit<Job, "id" | "createdAt" | "updatedAt" | "supabaseUserId">>
): Promise<ApiResponse<void>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: "Authentication required" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/jobs/${jobId}`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(jobData),
      }
    );

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to update job: ${response.statusText}`,
      };
    }

    return { data: undefined };
  } catch (error) {
    console.error("Error in updateJobAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}

/**
 * Deletes a job
 * Server Action that securely calls the .NET backend API
 *
 * @param jobId - The job ID to delete
 * @returns Promise with success/error
 */
export async function deleteJobAction(jobId: string): Promise<ApiResponse<void>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: "Authentication required" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/jobs/${jobId}`,
      {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      }
    );

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to delete job: ${response.statusText}`,
      };
    }

    return { data: undefined };
  } catch (error) {
    console.error("Error in deleteJobAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}
