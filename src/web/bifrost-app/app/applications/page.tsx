"use client";

import React, { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { createClient } from "@/lib/supabase/client";
import {
  Job,
  JobApplication,
  JobApplicationStatus,
  UserInfo,
} from "@/lib/types";
import {
  getUserJobApplications,
  deleteJobApplication,
} from "@/lib/api/job-applications";
import { getUserJobs } from "@/lib/api/jobs";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Loader2, Plus, Edit, Trash2, Eye } from "lucide-react";
import Link from "next/link";

interface ApplicationWithJob extends JobApplication {
  job?: Job;
}

/**
 * Job Applications Page Component
 * Displays list of job applications with CRUD operations
 * Follows Single Responsibility Principle - manages job applications list view
 */
export default function JobApplicationsPage() {
  const [applications, setApplications] = useState<ApplicationWithJob[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const router = useRouter();

  useEffect(() => {
    fetchApplications();
  }, [router]);

  const fetchApplications = async () => {
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

      // Fetch applications and jobs in parallel
      const [applicationsResponse, jobsResponse] = await Promise.all([
        getUserJobApplications(parsedUserInfo.id, parsedUserInfo.accessToken),
        getUserJobs(parsedUserInfo.id, parsedUserInfo.accessToken),
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
        err instanceof Error ? err.message : "Failed to load applications"
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (applicationId: number) => {
    if (!userInfo) return;

    if (!confirm("Are you sure you want to delete this application?")) {
      return;
    }

    try {
      setDeletingId(applicationId);
      const response = await deleteJobApplication(
        applicationId,
        userInfo.accessToken
      );

      if (response.error) {
        setError(response.error);
        return;
      }

      // Remove from local state
      setApplications((prev) =>
        prev.filter((app) => app.id !== applicationId)
      );
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to delete application"
      );
    } finally {
      setDeletingId(null);
    }
  };

  const getStatusLabel = (status: number): string => {
    switch (status) {
      case JobApplicationStatus.NotApplied:
        return "Not Applied";
      case JobApplicationStatus.Applied:
        return "Applied";
      case JobApplicationStatus.InProcess:
        return "In Process";
      case JobApplicationStatus.WaitingFeedback:
        return "Waiting Feedback";
      case JobApplicationStatus.WaitingJobOffer:
        return "Waiting Job Offer";
      case JobApplicationStatus.Failed:
        return "Failed";
      default:
        return "Unknown";
    }
  };

  const getStatusColor = (status: number): string => {
    switch (status) {
      case JobApplicationStatus.NotApplied:
        return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-100";
      case JobApplicationStatus.Applied:
        return "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-100";
      case JobApplicationStatus.InProcess:
        return "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-100";
      case JobApplicationStatus.WaitingFeedback:
        return "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-100";
      case JobApplicationStatus.WaitingJobOffer:
        return "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100";
      case JobApplicationStatus.Failed:
        return "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-100";
      default:
        return "bg-gray-100 text-gray-800";
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
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Job Applications</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Manage your job applications
          </p>
        </div>
        <Link href="/applications/new">
          <Button className="flex items-center gap-2">
            <Plus className="h-4 w-4" />
            New Application
          </Button>
        </Link>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-950 border border-red-200 dark:border-red-800 text-red-800 dark:text-red-200 px-4 py-3 rounded">
          {error}
        </div>
      )}

      {applications.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <p className="text-muted-foreground mb-4">
              No job applications found
            </p>
            <Link href="/applications/new">
              <Button>
                <Plus className="h-4 w-4 mr-2" />
                Create Your First Application
              </Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4">
          {applications.map((application) => (
            <Card key={application.id} className="hover:shadow-lg transition-shadow">
              <CardContent className="p-6">
                <div className="flex items-start justify-between">
                  <div className="flex-1 space-y-2">
                    <div className="flex items-start gap-4">
                      <div className="flex-1">
                        <h3 className="text-xl font-semibold">
                          {application.job?.title || "Unknown Position"}
                        </h3>
                        <p className="text-muted-foreground">
                          {application.job?.company || "Unknown Company"}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          {application.job?.location || "Location not specified"}
                        </p>
                      </div>
                      <span
                        className={`px-3 py-1 rounded-full text-xs font-medium ${getStatusColor(
                          application.status
                        )}`}
                      >
                        {getStatusLabel(application.status)}
                      </span>
                    </div>
                    <div className="text-sm text-muted-foreground">
                      Applied: {new Date(application.created).toLocaleDateString()}
                    </div>
                  </div>

                  <div className="flex items-center gap-2 ml-4">
                    <Link href={`/applications/${application.id}`}>
                      <Button variant="outline" size="sm" title="View Details">
                        <Eye className="h-4 w-4" />
                      </Button>
                    </Link>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleDelete(application.id)}
                      disabled={deletingId === application.id}
                      title="Delete Application"
                    >
                      {deletingId === application.id ? (
                        <Loader2 className="h-4 w-4 animate-spin" />
                      ) : (
                        <Trash2 className="h-4 w-4 text-red-600" />
                      )}
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
