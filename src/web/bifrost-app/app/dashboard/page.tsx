"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { createClient } from "@/lib/supabase/client";
import {
  Job,
  JobApplication,
  JobApplicationStatus,
  UserInfo,
} from "@/lib/types";
import { getUserJobApplications } from "@/lib/api/job-applications";
import { getUserJobs } from "@/lib/api/jobs";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Loader2 } from "lucide-react";

interface ApplicationWithJob extends JobApplication {
  job?: Job;
}

interface StatusCardProps {
  title: string;
  count: number;
  status: JobApplicationStatus;
  applications: ApplicationWithJob[];
  color: string;
}

function StatusCard({ title, count, applications, color }: StatusCardProps) {
  return (
    <Card className="hover:shadow-lg transition-shadow">
      <CardHeader>
        <CardTitle className="text-lg font-semibold flex items-center gap-2">
          <div className={`w-3 h-3 rounded-full ${color}`}></div>
          {title}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="text-3xl font-bold mb-4">{count}</div>
        {applications.length > 0 && (
          <div className="space-y-2">
            <p className="text-sm text-muted-foreground">Recent applications:</p>
            <ul className="space-y-1">
              {applications.slice(0, 3).map((app) => (
                <li key={app.id} className="text-sm truncate">
                  {app.job ? `${app.job.title} at ${app.job.company}` : `Job ID: ${app.jobId}`}
                </li>
              ))}
            </ul>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export default function DashboardPage() {
  const [applications, setApplications] = useState<ApplicationWithJob[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const router = useRouter();

  useEffect(() => {
    async function fetchData() {
      try {
        // Check if user is authenticated
        const supabase = createClient();
        const {
          data: { user },
        } = await supabase.auth.getUser();

        if (!user) {
          router.push("/auth/login");
          return;
        }

        // Get user info from localStorage
        const storedUserInfo = localStorage.getItem("userInfo");
        if (!storedUserInfo) {
          router.push("/auth/login");
          return;
        }

        const parsedUserInfo: UserInfo = JSON.parse(storedUserInfo);
        setUserInfo(parsedUserInfo);

        // Fetch job applications and jobs in parallel
        const [applicationsResponse, jobsResponse] = await Promise.all([
          getUserJobApplications(
            parsedUserInfo.id,
            parsedUserInfo.accessToken
          ),
          getUserJobs(
            parsedUserInfo.id,
            parsedUserInfo.accessToken
          ),
        ]);

        if (applicationsResponse.error) {
          setError(applicationsResponse.error);
          return;
        }

        if (jobsResponse.error) {
          setError(jobsResponse.error);
          return;
        }

        // Combine applications with job details
        if (applicationsResponse.data && jobsResponse.data) {
          const jobsMap = new Map(jobsResponse.data.map((job) => [job.id, job]));
          const applicationsWithJobs = applicationsResponse.data.map((app) => ({
            ...app,
            job: jobsMap.get(app.jobId),
          }));
          setApplications(applicationsWithJobs);
        }
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to load dashboard data"
        );
      } finally {
        setIsLoading(false);
      }
    }

    fetchData();
  }, [router]);

  const handleLogout = async () => {
    const supabase = createClient();
    await supabase.auth.signOut();
    localStorage.removeItem("userInfo");
    router.push("/auth/login");
  };

  // Filter applications by status
  const notApplied = applications.filter(
    (app) => app.status === JobApplicationStatus.NotApplied
  );
  const applied = applications.filter(
    (app) => app.status === JobApplicationStatus.Applied
  );
  const inProcess = applications.filter(
    (app) => app.status === JobApplicationStatus.InProcess
  );
  const waitingFeedback = applications.filter(
    (app) => app.status === JobApplicationStatus.WaitingFeedback
  );
  const waitingJobOffer = applications.filter(
    (app) => app.status === JobApplicationStatus.WaitingJobOffer
  );

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Job Applications Dashboard</h1>
          {userInfo && (
            <p className="text-sm text-muted-foreground mt-1">
              Welcome, {userInfo.email}
            </p>
          )}
        </div>
        <Button onClick={handleLogout} variant="outline">
          Logout
        </Button>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-950 border border-red-200 dark:border-red-800 text-red-800 dark:text-red-200 px-4 py-3 rounded">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <StatusCard
          title="Not Applied"
          count={notApplied.length}
          status={JobApplicationStatus.NotApplied}
          applications={notApplied}
          color="bg-gray-400"
        />
        <StatusCard
          title="Applied"
          count={applied.length}
          status={JobApplicationStatus.Applied}
          applications={applied}
          color="bg-blue-500"
        />
        <StatusCard
          title="In Process"
          count={inProcess.length}
          status={JobApplicationStatus.InProcess}
          applications={inProcess}
          color="bg-yellow-500"
        />
        <StatusCard
          title="Waiting Feedback"
          count={waitingFeedback.length}
          status={JobApplicationStatus.WaitingFeedback}
          applications={waitingFeedback}
          color="bg-orange-500"
        />
        <StatusCard
          title="Waiting Job Offer"
          count={waitingJobOffer.length}
          status={JobApplicationStatus.WaitingJobOffer}
          applications={waitingJobOffer}
          color="bg-green-500"
        />
      </div>

      <div className="mt-6">
        <Card>
          <CardHeader>
            <CardTitle>Summary</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              <p className="text-lg">
                Total Applications:{" "}
                <span className="font-bold">{applications.length}</span>
              </p>
              <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mt-4">
                <div className="text-center">
                  <p className="text-2xl font-bold text-gray-600">
                    {notApplied.length}
                  </p>
                  <p className="text-xs text-muted-foreground">Not Applied</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-bold text-blue-600">
                    {applied.length}
                  </p>
                  <p className="text-xs text-muted-foreground">Applied</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-bold text-yellow-600">
                    {inProcess.length}
                  </p>
                  <p className="text-xs text-muted-foreground">In Process</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-bold text-orange-600">
                    {waitingFeedback.length}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    Waiting Feedback
                  </p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-bold text-green-600">
                    {waitingJobOffer.length}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    Waiting Job Offer
                  </p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
