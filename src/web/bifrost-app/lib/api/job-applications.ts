/**
 * @deprecated This file is deprecated. Use server actions from @/lib/actions/job-applications instead.
 *
 * Migration guide:
 * - getUserJobApplications() → getUserJobApplicationsAction()
 * - getJobApplicationsByJobId() → getJobApplicationsByJobIdAction()
 * - getJobApplicationById() → getJobApplicationByIdAction()
 * - createJobApplication() → createJobApplicationAction()
 * - updateJobApplicationStatus() → updateJobApplicationStatusAction()
 * - deleteJobApplication() → deleteJobApplicationAction()
 *
 * Server actions run on the server and keep API URLs secure.
 * They provide better security by not exposing the API endpoint to the client.
 */

import { JobApplication } from "@/lib/types";

/**
 * @deprecated Use server actions instead
 */
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5037";

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
 * Fetches all job applications for a specific user
 * GET /api/applications/user/{userId}
 * @param userId - The user's ID (Supabase UUID)
 * @param token - JWT authentication token from Supabase
 * @returns Promise with the list of job applications or error
 */
export async function getUserJobApplications(
  userId: string,
  token: string
): Promise<ApiResponse<JobApplication[]>> {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/applications/user/${userId}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
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
    return {
      error: error instanceof Error ? error.message : "Unknown error occurred",
    };
  }
}

/**
 * Fetches a single job application by ID
 * GET /api/applications/{applicationId}
 * @param applicationId - The application ID
 * @param token - JWT authentication token
 * @returns Promise with the job application or error
 */
export async function getJobApplication(
  applicationId: number,
  token: string
): Promise<ApiResponse<JobApplication>> {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/applications/${applicationId}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
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
    return {
      error: error instanceof Error ? error.message : "Unknown error occurred",
    };
  }
}

/**
 * Fetches all applications for a specific job
 * GET /api/applications/job/{jobId}
 * @param jobId - The job ID
 * @param token - JWT authentication token
 * @returns Promise with the list of job applications or error
 */
export async function getJobApplications(
  jobId: number,
  token: string
): Promise<ApiResponse<JobApplication[]>> {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/applications/job/${jobId}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
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
    return {
      error: error instanceof Error ? error.message : "Unknown error occurred",
    };
  }
}

/**
 * Creates a new job application
 * POST /api/applications
 * @param userId - The user's ID
 * @param jobId - The job ID to apply for
 * @param token - JWT authentication token
 * @returns Promise with the created job application or error
 */
export async function createJobApplication(
  userId: string,
  jobId: number,
  token: string
): Promise<ApiResponse<JobApplication>> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/applications`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ userId, jobId }),
    });

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to create job application: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    return {
      error: error instanceof Error ? error.message : "Unknown error occurred",
    };
  }
}

/**
 * Updates a job application status
 * PUT /api/applications/{applicationId}/status
 * @param applicationId - The application ID
 * @param status - The new status
 * @param token - JWT authentication token
 * @returns Promise with the updated job application or error
 */
export async function updateJobApplicationStatus(
  applicationId: number,
  status: number,
  token: string
): Promise<ApiResponse<JobApplication>> {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/applications/${applicationId}/status`,
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

    const data = await response.json();
    return { data };
  } catch (error) {
    return {
      error: error instanceof Error ? error.message : "Unknown error occurred",
    };
  }
}

/**
 * Deletes a job application
 * DELETE /api/applications/{applicationId}
 * @param applicationId - The application ID
 * @param token - JWT authentication token
 * @returns Promise with success or error
 */
export async function deleteJobApplication(
  applicationId: number,
  token: string
): Promise<ApiResponse<void>> {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/applications/${applicationId}`,
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
    return {
      error: error instanceof Error ? error.message : "Unknown error occurred",
    };
  }
}
