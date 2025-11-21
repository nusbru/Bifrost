export enum JobApplicationStatus {
  NotApplied = 0,
  Applied = 1,
  InProcess = 2,
  WaitingFeedback = 3,
  WaitingJobOffer = 4,
  Failed = 5,
}

export enum JobType {
  FullTime = 0,
  PartTime = 1,
  Contract = 2,
  Freelance = 3,
  Internship = 4,
  Temporary = 5,
}

/**
 * Job Application Response from API
 * Matches the JobApplicationResponse schema from OpenAPI
 */
export interface JobApplication {
  id: number;
  jobId: number;
  userId: string;
  status: number;
  created: string;
  updated: string;
}

/**
 * Job Response from API
 * Matches the JobResponse schema from OpenAPI
 */
export interface Job {
  id: number;
  title: string;
  company: string;
  location: string;
  jobType: number;
  description: string;
  offerSponsorship: boolean;
  offerRelocation: boolean;
  userId: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface UserInfo {
  id: string;
  email: string;
  accessToken: string;
  refreshToken?: string;
}
