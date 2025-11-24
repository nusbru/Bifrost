/**
 * Server-side configuration for Bifrost application
 *
 * These environment variables are NOT exposed to the browser.
 * They are evaluated at runtime on the server only.
 *
 * DO NOT prefix these with NEXT_PUBLIC_ as they contain sensitive information.
 */

/**
 * Internal API URL for server-side requests
 * Points to the .NET backend API
 */
export const SERVER_API_URL = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || "http://localhost:5037";

/**
 * Supabase service role key for admin operations
 * This is a SECRET and should NEVER be exposed to the client
 */
export const SUPABASE_SERVICE_ROLE_KEY = process.env.SUPABASE_SERVICE_ROLE_KEY;

/**
 * Server-side Supabase configuration
 * URL can be the same as client-side, but we access it securely here
 */
export const SERVER_SUPABASE_URL = process.env.SUPABASE_URL || process.env.NEXT_PUBLIC_SUPABASE_URL;

/**
 * Validate that required server-side environment variables are set
 */
export function validateServerConfig() {
  const errors: string[] = [];

  if (!SERVER_API_URL) {
    errors.push("API_URL is not set");
  }

  if (!SERVER_SUPABASE_URL) {
    errors.push("SUPABASE_URL or NEXT_PUBLIC_SUPABASE_URL is not set");
  }

  if (errors.length > 0) {
    throw new Error(`Server configuration errors:\n${errors.join("\n")}`);
  }
}
