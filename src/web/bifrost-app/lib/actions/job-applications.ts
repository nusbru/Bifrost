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
import { ApiResponse, ProblemDetails } from "@/lib/types/api";
import { createClient } from "@/lib/supabase/server";
import { logger } from "@/lib/logger";
import { MESSAGES } from "@/lib/constants";

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
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    // Get user ID from session
    const supabase = await createClient();
    const { data: { user } } = await supabase.auth.getUser();

    if (!user) {
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/applications/user/${user.id}`,
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
    logger.error("Error in getUserJobApplicationsAction", error, { action: "getUserJobApplicationsAction" });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
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
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/applications/job/${jobId}`,
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
    logger.error("Error in getJobApplicationsByJobIdAction", error, { action: "getJobApplicationsByJobIdAction", jobId });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
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
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/applications/${applicationId}`,
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
    logger.error("Error in getJobApplicationByIdAction", error, { action: "getJobApplicationByIdAction", applicationId });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
    };
  }
}

/**
 * Creates a new job application
 * Server Action that securely calls the .NET backend API
 *
 * @param jobId - The job ID to create an application for
 * @returns Promise with the created job application or error
 */
export async function createJobApplicationAction(
  jobId: number
): Promise<ApiResponse<JobApplication>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    // Get user ID from session
    const supabase = await createClient();
    const { data: { user } } = await supabase.auth.getUser();

    if (!user) {
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/applications`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          userId: user.id,
          jobId,
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
    logger.error("Error in createJobApplicationAction", error, { action: "createJobApplicationAction", jobId });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
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
  status: number
): Promise<ApiResponse<void>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/applications/${applicationId}/status`,
      {
        method: "PUT",
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
    logger.error("Error in updateJobApplicationStatusAction", error, { action: "updateJobApplicationStatusAction", applicationId, status });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
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
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/applications/${applicationId}`,
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
    logger.error("Error in deleteJobApplicationAction", error, { action: "deleteJobApplicationAction", applicationId });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
    };
  }
}
