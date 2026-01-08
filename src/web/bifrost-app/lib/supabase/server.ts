/**
 * Server-side authentication helper
 * Placeholder for server-side session management
 * TODO: Implement proper server-side authentication with JWT tokens
 */

import { cookies } from "next/headers";

export interface ServerSession {
  access_token: string;
  user: {
    id: string;
    email: string;
  };
}

/**
 * Get the current session from cookies (server-side)
 * This is a placeholder implementation
 */
export async function getServerSession(): Promise<ServerSession | null> {
  const cookieStore = await cookies();
  const token = cookieStore.get("bifrost_access_token")?.value;
  const userJson = cookieStore.get("bifrost_user")?.value;

  if (!token || !userJson) {
    return null;
  }

  try {
    const user = JSON.parse(userJson);
    return {
      access_token: token,
      user: {
        id: user.id,
        email: user.email,
      },
    };
  } catch {
    return null;
  }
}

/**
 * Create a placeholder createClient function for compatibility
 * Reads authentication from cookies set by the client
 */
export async function createClient() {
  return {
    auth: {
      getSession: async () => {
        const session = await getServerSession();
        return {
          data: { session },
          error: null,
        };
      },
      getUser: async () => {
        const session = await getServerSession();
        return {
          data: { user: session?.user || null },
          error: null,
        };
      },
    },
  };
}
