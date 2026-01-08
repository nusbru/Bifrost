/**
 * Client-side authentication compatibility layer
 * Placeholder for Supabase client migration
 */

/**
 * Create a placeholder createClient function for compatibility
 * This is temporary - features using this should be migrated to use authService
 */
export function createClient() {
  return {
    auth: {
      getUser: async () => {
        // Placeholder - features should use authService instead
        return {
          data: { user: null },
          error: new Error("Feature not yet migrated to new auth system"),
        };
      },
      verifyOtp: async () => {
        return {
          data: null,
          error: new Error("Feature not yet migrated to new auth system"),
        };
      },
      resetPasswordForEmail: async () => {
        return {
          data: null,
          error: new Error("Feature not yet migrated to new auth system"),
        };
      },
      updateUser: async () => {
        return {
          data: null,
          error: new Error("Feature not yet migrated to new auth system"),
        };
      },
    },
  };
}
