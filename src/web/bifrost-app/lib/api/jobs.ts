/**
 * @deprecated This file is deprecated. Use server actions from @/lib/actions/jobs instead.
 *
 * Migration guide:
 * - getUserJobs() → getUserJobsAction()
 * - getJobById() → getJobByIdAction()
 * - createJob() → createJobAction()
 * - updateJob() → updateJobAction()
 * - deleteJob() → deleteJobAction()
 *
 * Server actions run on the server and keep API URLs secure.
 * They provide better security by not exposing the API endpoint to the client.
 */

import { Job } from "@/lib/types";

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
 * Fetches all jobs for a specific user
 * GET /api/jobs/user/{userId}
 * @param userId - The user's ID (Supabase UUID)
 * @param token - JWT authentication token from Supabase
 * @returns Promise with the list of jobs or error
 */
export async function getUserJobs(
  userId: string,
  token: string
): Promise<ApiResponse<Job[]>> {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/jobs/user/${userId}`,
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
        error: problemDetails.detail || `Failed to fetch jobs: ${response.statusText}`,
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
 * Fetches a single job by ID
 * GET /api/jobs/{jobId}
 * @param jobId - The job ID
 * @param token - JWT authentication token
 * @returns Promise with the job or error
 */
export async function getJob(
  jobId: number,
  token: string
): Promise<ApiResponse<Job>> {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/jobs/${jobId}`,
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
        error: problemDetails.detail || `Failed to fetch job: ${response.statusText}`,
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
