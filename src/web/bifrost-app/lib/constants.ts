/**
 * Application-wide constants
 * Centralized location for magic numbers and reusable strings
 */

/**
 * User-facing messages
 */
export const MESSAGES = {
  // Confirmation dialogs
  DELETE_JOB_CONFIRMATION: "Are you sure you want to delete this job posting?",
  DELETE_APPLICATION_CONFIRMATION: "Are you sure you want to delete this application?",
  DELETE_NOTE_CONFIRMATION: "Are you sure you want to delete this note?",

  // Error messages
  AUTH_REQUIRED: "Authentication required. Please log in.",
  SESSION_EXPIRED: "Your session has expired. Please log in again.",
  NETWORK_ERROR: "Network error. Please check your connection and try again.",
  UNKNOWN_ERROR: "An unexpected error occurred. Please try again.",

  // Success messages
  JOB_CREATED: "Job posting created successfully!",
  JOB_UPDATED: "Job posting updated successfully!",
  JOB_DELETED: "Job posting deleted successfully!",
  APPLICATION_CREATED: "Application created successfully!",
  APPLICATION_UPDATED: "Application updated successfully!",
  APPLICATION_DELETED: "Application deleted successfully!",

  // Loading messages
  LOADING_JOBS: "Loading jobs...",
  LOADING_APPLICATIONS: "Loading applications...",
  LOADING_DATA: "Loading...",
} as const;

/**
 * Limits and pagination
 */
export const LIMITS = {
  DASHBOARD_RECENT_APPLICATIONS: 3,
  APPLICATIONS_PER_PAGE: 20,
  JOBS_PER_PAGE: 20,
  MAX_FILE_SIZE_MB: 5,
} as const;

/**
 * Route paths
 */
export const ROUTES = {
  HOME: "/",
  LOGIN: "/auth/login",
  SIGNUP: "/auth/sign-up",
  FORGOT_PASSWORD: "/auth/forgot-password",
  DASHBOARD: "/dashboard",
  JOBS: "/jobs",
  NEW_JOB: "/jobs/new",
  APPLICATIONS: "/applications",
  NEW_APPLICATION: "/applications/new",
  PREFERENCES: "/preferences",
} as const;

/**
 * API configuration
 */
export const API_CONFIG = {
  TIMEOUT_MS: 30000,
  RETRY_ATTEMPTS: 3,
  RETRY_DELAY_MS: 1000,
} as const;
