/**
 * Dashboard Page - Server Component
 * Displays job application statistics and overview
 * Converted from Client Component for better performance
 */

import { redirect } from "next/navigation";
import { createClient } from "@/lib/supabase/server";
import { getUserJobApplicationsAction } from "@/lib/actions/job-applications";
import { getUserJobsAction } from "@/lib/actions/jobs";
import { Job, JobApplication, JobApplicationStatus } from "@/lib/types";
import { StatusCard } from "@/components/dashboard/status-card";
import { filterByStatus, getStatusColor } from "@/lib/utils/job-status";
import { ROUTES } from "@/lib/constants";

interface ApplicationWithJob extends JobApplication {
  job?: Job;
}

export default async function DashboardPage() {
  // Check authentication server-side
  const supabase = await createClient();
  const {
    data: { user },
  } = await supabase.auth.getUser();

  if (!user) {
    redirect(ROUTES.LOGIN);
  }

  // Fetch data in parallel on the server
  const [applicationsResponse, jobsResponse] = await Promise.all([
    getUserJobApplicationsAction(),
    getUserJobsAction(),
  ]);

  // Handle errors
  if (applicationsResponse.error || jobsResponse.error) {
    return (
      <div className="container mx-auto p-6">
        <div className="bg-red-50 dark:bg-red-950 border border-red-200 dark:border-red-800 text-red-800 dark:text-red-200 px-4 py-3 rounded">
          {applicationsResponse.error || jobsResponse.error}
        </div>
      </div>
    );
  }

  // Combine applications with job details
  const applications: ApplicationWithJob[] = applicationsResponse.data
    ? applicationsResponse.data.map((app) => ({
        ...app,
        job: jobsResponse.data?.find((job) => job.id === app.jobId),
      }))
    : [];

  // Filter applications by status using utility function
  const notApplied = filterByStatus(applications, JobApplicationStatus.NotApplied);
  const applied = filterByStatus(applications, JobApplicationStatus.Applied);
  const inProcess = filterByStatus(applications, JobApplicationStatus.InProcess);
  const waitingFeedback = filterByStatus(applications, JobApplicationStatus.WaitingFeedback);
  const waitingJobOffer = filterByStatus(applications, JobApplicationStatus.WaitingJobOffer);

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Job Applications Dashboard</h1>
        <p className="text-sm text-muted-foreground mt-1">Welcome, {user.email}</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <StatusCard
          title="Not Applied"
          count={notApplied.length}
          status={JobApplicationStatus.NotApplied}
          applications={notApplied}
          color={getStatusColor(JobApplicationStatus.NotApplied)}
        />
        <StatusCard
          title="Applied"
          count={applied.length}
          status={JobApplicationStatus.Applied}
          applications={applied}
          color={getStatusColor(JobApplicationStatus.Applied)}
        />
        <StatusCard
          title="In Process"
          count={inProcess.length}
          status={JobApplicationStatus.InProcess}
          applications={inProcess}
          color={getStatusColor(JobApplicationStatus.InProcess)}
        />
        <StatusCard
          title="Waiting Feedback"
          count={waitingFeedback.length}
          status={JobApplicationStatus.WaitingFeedback}
          applications={waitingFeedback}
          color={getStatusColor(JobApplicationStatus.WaitingFeedback)}
        />
        <StatusCard
          title="Waiting Job Offer"
          count={waitingJobOffer.length}
          status={JobApplicationStatus.WaitingJobOffer}
          applications={waitingJobOffer}
          color={getStatusColor(JobApplicationStatus.WaitingJobOffer)}
        />
      </div>
    </div>
  );
}
