export enum JobApplicationStatus {
  NotApplied = 0,
  Applied = 1,
  InProcess = 2,
  WaitingFeedback = 3,
  WaitingJobOffer = 4,
  Failed = 5,
}

export interface JobApplication {
  id: number;
  jobId: number;
  userId: string;
  status: JobApplicationStatus;
  created: string;
  updated: string;
}

export interface Job {
  id: number;
  title: string;
  company: string;
  description: string;
  location?: string;
  salaryMin?: number;
  salaryMax?: number;
  type: number;
  url?: string;
  userId: string;
  created: string;
  updated: string;
}

export interface UserInfo {
  id: string;
  email: string;
  accessToken: string;
  refreshToken?: string;
}
