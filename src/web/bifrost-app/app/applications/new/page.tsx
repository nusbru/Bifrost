"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { createClient } from "@/lib/supabase/client";
import { Job, UserInfo } from "@/lib/types";
import { getUserJobs } from "@/lib/api/jobs";
import { createJobApplication } from "@/lib/api/job-applications";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Loader2, ArrowLeft } from "lucide-react";
import Link from "next/link";

/**
 * New Job Application Page Component
 * Allows users to create a new job application
 * Follows Single Responsibility Principle - manages application creation
 */
export default function NewApplicationPage() {
  const [jobs, setJobs] = useState<Job[]>([]);
  const [selectedJobId, setSelectedJobId] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const router = useRouter();

  useEffect(() => {
    fetchJobs();
  }, [router]);

  const fetchJobs = async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Check authentication
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

      // Fetch user's jobs
      const jobsResponse = await getUserJobs(
        parsedUserInfo.id,
        parsedUserInfo.accessToken
      );

      if (jobsResponse.error) {
        setError(jobsResponse.error);
        return;
      }

      if (jobsResponse.data) {
        setJobs(jobsResponse.data);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load jobs");
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!userInfo || !selectedJobId) {
      setError("Please select a job");
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      const response = await createJobApplication(
        userInfo.id,
        selectedJobId,
        userInfo.accessToken
      );

      if (response.error) {
        setError(response.error);
        return;
      }

      // Navigate to applications list on success
      router.push("/applications");
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create application"
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 max-w-2xl">
      <div className="mb-6">
        <Link href="/applications">
          <Button variant="ghost" className="mb-4">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Applications
          </Button>
        </Link>
        <h1 className="text-3xl font-bold">New Job Application</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Create a new job application
        </p>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-950 border border-red-200 dark:border-red-800 text-red-800 dark:text-red-200 px-4 py-3 rounded mb-6">
          {error}
        </div>
      )}

      {jobs.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <p className="text-muted-foreground mb-4">
              No jobs found. Please create a job first before applying.
            </p>
            <Link href="/jobs/new">
              <Button>Create a Job</Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>Application Details</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-6">
              <div className="space-y-2">
                <Label htmlFor="job">Select Job *</Label>
                <select
                  id="job"
                  required
                  value={selectedJobId || ""}
                  onChange={(e) => setSelectedJobId(Number(e.target.value))}
                  className="w-full px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                >
                  <option value="">-- Select a job --</option>
                  {jobs.map((job) => (
                    <option key={job.id} value={job.id}>
                      {job.title} at {job.company}
                    </option>
                  ))}
                </select>
              </div>

              {selectedJobId && (
                <div className="p-4 border rounded-md bg-muted/50">
                  {(() => {
                    const selectedJob = jobs.find((j) => j.id === selectedJobId);
                    if (!selectedJob) return null;
                    return (
                      <div className="space-y-2">
                        <h3 className="font-semibold">{selectedJob.title}</h3>
                        <p className="text-sm text-muted-foreground">
                          {selectedJob.company}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          {selectedJob.location}
                        </p>
                        <p className="text-sm mt-2">{selectedJob.description}</p>
                      </div>
                    );
                  })()}
                </div>
              )}

              <div className="flex gap-4">
                <Button
                  type="submit"
                  disabled={isSubmitting || !selectedJobId}
                  className="flex-1"
                >
                  {isSubmitting ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Creating...
                    </>
                  ) : (
                    "Create Application"
                  )}
                </Button>
                <Link href="/applications" className="flex-1">
                  <Button type="button" variant="outline" className="w-full">
                    Cancel
                  </Button>
                </Link>
              </div>
            </form>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
