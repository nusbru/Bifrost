/**
 * Shared API types for backend communication
 * Used by Server Actions to interact with .NET backend
 */

/**
 * Generic API response wrapper
 * @template T - The type of data returned on success
 */
export interface ApiResponse<T> {
  data?: T;
  error?: string;
}

/**
 * Problem Details - RFC 7807 standard error response
 * Matches the ProblemDetails schema from .NET backend
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

/**
 * Type guard to check if response has error
 */
export function hasError<T>(response: ApiResponse<T>): response is { error: string } {
  return response.error !== undefined;
}

/**
 * Type guard to check if response has data
 */
export function hasData<T>(response: ApiResponse<T>): response is { data: T } {
  return response.data !== undefined;
}
