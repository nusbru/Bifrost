"use server";

/**
 * Server Actions for Job Application Management
 *
 * These actions run on the server only and keep API URLs and tokens secure.
 * They follow Next.js 16 Server Actions pattern with proper error handling.
 *
 * SOLID Principles:
 * - Single Responsibility: Each action handles one specific API operation
 * - Dependency Inversion: Actions depend on server config abstraction
 */

import { SERVER_API_URL } from "@/lib/config/server";
import { JobApplication } from "@/lib/types";
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
 * Fetches all job applications for the authenticated user
 * Server Action that securely calls the .NET backend API
 *
 * @returns Promise with the list of job applications or error
 */
export async function getUserJobApplicationsAction(): Promise<ApiResponse<JobApplication[]>> {
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
      `${SERVER_API_URL}/api/job-applications/user/${user.id}`,
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
        error: problemDetails.detail || `Failed to fetch job applications: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    console.error("Error in getUserJobApplicationsAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}

/**
 * Fetches job applications for a specific job
 * Server Action that securely calls the .NET backend API
 *
 * @param jobId - The job ID to fetch applications for
 * @returns Promise with the list of job applications or error
 */
export async function getJobApplicationsByJobIdAction(jobId: string): Promise<ApiResponse<JobApplication[]>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: "Authentication required" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/job-applications/job/${jobId}`,
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
        error: problemDetails.detail || `Failed to fetch job applications: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    console.error("Error in getJobApplicationsByJobIdAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}

/**
 * Fetches a single job application by ID
 * Server Action that securely calls the .NET backend API
 *
 * @param applicationId - The job application ID to fetch
 * @returns Promise with the job application data or error
 */
export async function getJobApplicationByIdAction(applicationId: string): Promise<ApiResponse<JobApplication>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: "Authentication required" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/job-applications/${applicationId}`,
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
        error: problemDetails.detail || `Failed to fetch job application: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    console.error("Error in getJobApplicationByIdAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}

/**
 * Creates a new job application
 * Server Action that securely calls the .NET backend API
 *
 * @param applicationData - The job application data to create
 * @returns Promise with the created job application or error
 */
export async function createJobApplicationAction(
  applicationData: Omit<JobApplication, "id" | "createdAt" | "updatedAt" | "supabaseUserId">
): Promise<ApiResponse<JobApplication>> {
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
      `${SERVER_API_URL}/api/job-applications`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          ...applicationData,
          supabaseUserId: user.id,
        }),
      }
    );

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to create job application: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    console.error("Error in createJobApplicationAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}

/**
 * Updates a job application's status
 * Server Action that securely calls the .NET backend API
 *
 * @param applicationId - The job application ID to update
 * @param status - The new status
 * @returns Promise with success/error
 */
export async function updateJobApplicationStatusAction(
  applicationId: string,
  status: string
): Promise<ApiResponse<void>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: "Authentication required" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/job-applications/${applicationId}/status`,
      {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ status }),
      }
    );

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to update job application status: ${response.statusText}`,
      };
    }

    return { data: undefined };
  } catch (error) {
    console.error("Error in updateJobApplicationStatusAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}

/**
 * Deletes a job application
 * Server Action that securely calls the .NET backend API
 *
 * @param applicationId - The job application ID to delete
 * @returns Promise with success/error
 */
export async function deleteJobApplicationAction(applicationId: string): Promise<ApiResponse<void>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: "Authentication required" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/job-applications/${applicationId}`,
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
        error: problemDetails.detail || `Failed to delete job application: ${response.statusText}`,
      };
    }

    return { data: undefined };
  } catch (error) {
    console.error("Error in deleteJobApplicationAction:", error);
    return {
      error: error instanceof Error ? error.message : "An unexpected error occurred",
    };
  }
}
