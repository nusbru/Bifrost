import { JobApplication } from "@/lib/types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || "https://localhost:7001";

export interface ApiResponse<T> {
  data?: T;
  error?: string;
}

/**
 * Fetches all job applications for a specific user
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
      return {
        error: `Failed to fetch job applications: ${response.statusText}`,
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
 * Fetches job applications filtered by status
 * @param userId - The user's ID
 * @param status - The status to filter by
 * @param token - JWT authentication token
 * @returns Promise with the filtered list of job applications or error
 */
export async function getJobApplicationsByStatus(
  userId: string,
  status: number,
  token: string
): Promise<ApiResponse<JobApplication[]>> {
  const response = await getUserJobApplications(userId, token);

  if (response.error || !response.data) {
    return response;
  }

  const filteredData = response.data.filter((app) => app.status === status);
  return { data: filteredData };
}
