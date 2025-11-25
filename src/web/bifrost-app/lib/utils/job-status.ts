/**
 * Job Status Utilities
 * Centralized logic for job application status handling
 */

import { JobApplicationStatus } from "@/lib/types";

/**
 * Human-readable labels for job application statuses
 */
export const JOB_STATUS_LABELS: Record<JobApplicationStatus, string> = {
  [JobApplicationStatus.NotApplied]: "Not Applied",
  [JobApplicationStatus.Applied]: "Applied",
  [JobApplicationStatus.InProcess]: "In Process",
  [JobApplicationStatus.WaitingFeedback]: "Waiting Feedback",
  [JobApplicationStatus.WaitingJobOffer]: "Waiting Job Offer",
  [JobApplicationStatus.Failed]: "Failed",
};

/**
 * Color classes for job application statuses (Tailwind CSS)
 */
export const JOB_STATUS_COLORS: Record<JobApplicationStatus, string> = {
  [JobApplicationStatus.NotApplied]: "bg-gray-400",
  [JobApplicationStatus.Applied]: "bg-blue-500",
  [JobApplicationStatus.InProcess]: "bg-yellow-500",
  [JobApplicationStatus.WaitingFeedback]: "bg-orange-500",
  [JobApplicationStatus.WaitingJobOffer]: "bg-green-500",
  [JobApplicationStatus.Failed]: "bg-red-500",
};

/**
 * Get human-readable label for a job application status
 * @param status - The JobApplicationStatus enum value
 * @returns The human-readable label
 */
export function getStatusLabel(status: JobApplicationStatus): string {
  return JOB_STATUS_LABELS[status] ?? "Unknown";
}

/**
 * Get color class for a job application status
 * @param status - The JobApplicationStatus enum value
 * @returns The Tailwind CSS color class
 */
export function getStatusColor(status: JobApplicationStatus): string {
  return JOB_STATUS_COLORS[status] ?? "bg-gray-400";
}

/**
 * Filter applications by status
 * @param applications - Array of applications to filter
 * @param status - Status to filter by
 * @returns Filtered array of applications
 */
export function filterByStatus<T extends { status: number }>(
  applications: T[],
  status: JobApplicationStatus
): T[] {
  return applications.filter((app) => app.status === status);
}
