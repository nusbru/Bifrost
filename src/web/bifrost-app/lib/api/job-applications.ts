import { JobApplication } from "@/lib/types";

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
