"use server";

/**
 * Server Actions for Preferences Management
 *
 * These actions run on the server only and keep API URLs and tokens secure.
 * They follow Next.js 16 Server Actions pattern with proper error handling.
 *
 * SOLID Principles:
 * - Single Responsibility: Each action handles one specific API operation
 * - Dependency Inversion: Actions depend on server config abstraction
 */

import { SERVER_API_URL } from "@/lib/config/server";
import { Preferences } from "@/lib/types";
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
 * Fetches user preferences
 * Server Action that securely calls the .NET backend API
 *
 * @returns Promise with the preferences data or error
 */
export async function getUserPreferencesAction(): Promise<ApiResponse<Preferences>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    // Get user ID from session
    const supabase = await createClient();
    const { data: { user } } = await supabase.auth.getUser();

    if (!user) {
      return { error: "User not found" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/preferences/user/${user.id}`,
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
        error: problemDetails.detail || `Failed to fetch preferences: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    logger.error("Error in getUserPreferencesAction", error, { action: "getUserPreferencesAction" });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
    };
  }
}

/**
 * Creates user preferences
 * Server Action that securely calls the .NET backend API
 *
 * @param preferencesData - The preferences data to create
 * @returns Promise with the created preferences or error
 */
export async function createPreferencesAction(
  preferencesData: Omit<Preferences, "id" | "createdAt" | "updatedAt">
): Promise<ApiResponse<Preferences>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    // Get user ID from session
    const supabase = await createClient();
    const { data: { user } } = await supabase.auth.getUser();

    if (!user) {
      return { error: "User not found" };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/preferences`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          ...preferencesData,
          userId: user.id,
          preferredJobType: preferencesData.jobType,
        }),
      }
    );

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to create preferences: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    logger.error("Error in createPreferencesAction", error, { action: "createPreferencesAction" });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
    };
  }
}

/**
 * Updates user preferences
 * Server Action that securely calls the .NET backend API
 *
 * @param preferenceId - The preference ID to update
 * @param preferencesData - The updated preferences data
 * @returns Promise with success/error
 */
export async function updatePreferencesAction(
  preferenceId: string,
  preferencesData: Partial<Omit<Preferences, "id" | "userId" | "createdAt" | "updatedAt">>
): Promise<ApiResponse<Preferences>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/preferences/${preferenceId}`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(preferencesData),
      }
    );

    if (!response.ok) {
      const problemDetails: ProblemDetails = await response.json().catch(() => ({}));
      return {
        error: problemDetails.detail || `Failed to update preferences: ${response.statusText}`,
      };
    }

    const data = await response.json();
    return { data };
  } catch (error) {
    logger.error("Error in updatePreferencesAction", error, { action: "updatePreferencesAction", preferenceId });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
    };
  }
}

/**
 * Deletes user preferences
 * Server Action that securely calls the .NET backend API
 *
 * @param preferenceId - The preference ID to delete
 * @returns Promise with success/error
 */
export async function deletePreferencesAction(preferenceId: string): Promise<ApiResponse<void>> {
  try {
    const token = await getAuthToken();

    if (!token) {
      return { error: MESSAGES.AUTH_REQUIRED };
    }

    const response = await fetch(
      `${SERVER_API_URL}/api/preferences/${preferenceId}`,
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
        error: problemDetails.detail || `Failed to delete preferences: ${response.statusText}`,
      };
    }

    return { data: undefined };
  } catch (error) {
    logger.error("Error in deletePreferencesAction", error, { action: "deletePreferencesAction", preferenceId });
    return {
      error: error instanceof Error ? error.message : MESSAGES.UNKNOWN_ERROR,
    };
  }
}
