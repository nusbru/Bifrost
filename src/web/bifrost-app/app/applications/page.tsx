/**
 * Job Applications Page
 * Displays list of job applications with CRUD operations
 * Uses custom hooks and server actions
 */

"use client";

import React, { useEffect } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/hooks";
import { getUserJobApplicationsAction, deleteJobApplicationAction } from "@/lib/actions/job-applications";
import { getUserJobsAction } from "@/lib/actions/jobs";
import { Job, JobApplication } from "@/lib/types";
import { getStatusLabel } from "@/lib/utils/job-status";
import { MESSAGES, ROUTES } from "@/lib/constants";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Loader2, Plus, Edit, Trash2, Eye } from "lucide-react";

interface ApplicationWithJob extends JobApplication {
  job?: Job;
}

export default function JobApplicationsPage() {
  const { user, isLoading: authLoading } = useAuth();
  const [applications, setApplications] = React.useState<ApplicationWithJob[]>([]);
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [deletingId, setDeletingId] = React.useState<number | null>(null);

  useEffect(() => {
    if (user) {
      fetchApplications();
    }
  }, [user]);

  const fetchApplications = async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Fetch applications and jobs in parallel using server actions
      const [applicationsResponse, jobsResponse] = await Promise.all([
        getUserJobApplicationsAction(),
        getUserJobsAction(),
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
        err instanceof Error ? err.message : MESSAGES.UNKNOWN_ERROR
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (applicationId: number) => {
    if (!confirm(MESSAGES.DELETE_APPLICATION_CONFIRMATION)) {
      return;
    }

    try {
      setDeletingId(applicationId);
      const response = await deleteJobApplicationAction(applicationId.toString());

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
        err instanceof Error ? err.message : MESSAGES.UNKNOWN_ERROR
      );
    } finally {
      setDeletingId(null);
    }
  };

  if (authLoading || isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Job Applications</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Manage your job applications
          </p>
        </div>
        <Link href={ROUTES.NEW_APPLICATION}>
          <Button>
            <Plus className="h-4 w-4 mr-2" />
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
              No applications found. Create your first application!
            </p>
            <Link href={ROUTES.NEW_APPLICATION}>
              <Button>
                <Plus className="h-4 w-4 mr-2" />
                Create Application
              </Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {applications.map((app) => (
            <Card key={app.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <CardTitle className="text-lg">
                  {app.job ? app.job.title : `Job ID: ${app.jobId}`}
                </CardTitle>
                {app.job && (
                  <p className="text-sm text-muted-foreground">{app.job.company}</p>
                )}
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  <div>
                    <span className="text-sm font-medium">Status: </span>
                    <span className="text-sm">{getStatusLabel(app.status)}</span>
                  </div>
                  <div>
                    <span className="text-sm font-medium">Applied: </span>
                    <span className="text-sm">
                      {new Date(app.created).toLocaleDateString()}
                    </span>
                  </div>
                  <div className="flex gap-2 pt-2">
                    <Link href={`${ROUTES.APPLICATIONS}/${app.id}`} className="flex-1">
                      <Button variant="outline" size="sm" className="w-full">
                        <Eye className="h-4 w-4 mr-1" />
                        View
                      </Button>
                    </Link>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleDelete(app.id)}
                      disabled={deletingId === app.id}
                      className="flex-1"
                    >
                      {deletingId === app.id ? (
                        <Loader2 className="h-4 w-4 animate-spin" />
                      ) : (
                        <>
                          <Trash2 className="h-4 w-4 mr-1" />
                          Delete
                        </>
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
