"use client";

import { useEffect, useState } from "react";
import { useRouter, useParams } from "next/navigation";
import { useAuth } from "@/lib/hooks";
import {
  Job,
  JobApplication,
  JobApplicationStatus,
  JobType,
} from "@/lib/types";
import {
  getJobApplicationByIdAction,
  updateJobApplicationStatusAction,
} from "@/lib/actions/job-applications";
import { getJobByIdAction } from "@/lib/actions/jobs";
import { getStatusLabel } from "@/lib/utils/job-status";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Loader2, ArrowLeft, Save } from "lucide-react";
import Link from "next/link";

/**
 * Job Application Details Page Component
 * Displays job application details with ability to update status
 * Follows Single Responsibility Principle - manages single application view and status updates
 */
export default function ApplicationDetailsPage() {
  const { user, isLoading: authLoading } = useAuth();
  const [application, setApplication] = useState<JobApplication | null>(null);
  const [job, setJob] = useState<Job | null>(null);
  const [selectedStatus, setSelectedStatus] = useState<number>(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isUpdating, setIsUpdating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const router = useRouter();
  const params = useParams();
  const applicationId = params.id ? String(params.id) : null;

  useEffect(() => {
    if (user && applicationId) {
      fetchApplicationDetails();
    }
  }, [user, applicationId]);

  const fetchApplicationDetails = async () => {
    if (!applicationId) return;

    try {
      setIsLoading(true);
      setError(null);

      // Fetch application details using server action
      const appResponse = await getJobApplicationByIdAction(applicationId);

      if (appResponse.error) {
        setError(appResponse.error);
        return;
      }

      if (appResponse.data) {
        setApplication(appResponse.data);
        setSelectedStatus(appResponse.data.status);

        // Fetch job details using server action
        const jobResponse = await getJobByIdAction(String(appResponse.data.jobId));

        if (jobResponse.error) {
          setError(jobResponse.error);
          return;
        }

        if (jobResponse.data) {
          setJob(jobResponse.data);
        }
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to load application details"
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleStatusUpdate = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!applicationId) {
      setError("Invalid request");
      return;
    }

    try {
      setIsUpdating(true);
      setError(null);
      setSuccessMessage(null);

      const response = await updateJobApplicationStatusAction(
        applicationId,
        selectedStatus
      );

      if (response.error) {
        setError(response.error);
        return;
      }

      // Refetch to get updated data
      await fetchApplicationDetails();
      setSuccessMessage("Status updated successfully!");
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to update status"
      );
    } finally {
      setIsUpdating(false);
    }
  };

  const getJobTypeLabel = (jobType: number): string => {
    switch (jobType) {
      case JobType.FullTime:
        return "Full Time";
      case JobType.PartTime:
        return "Part Time";
      case JobType.Contract:
        return "Contract";
      case JobType.Freelance:
        return "Freelance";
      case JobType.Internship:
        return "Internship";
      case JobType.Temporary:
        return "Temporary";
      default:
        return "Unknown";
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!application || !job) {
    return (
      <div className="container mx-auto p-6 max-w-4xl">
        <div className="text-center py-12">
          <p className="text-muted-foreground mb-4">Application not found</p>
          <Link href="/applications">
            <Button>Back to Applications</Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 max-w-4xl">
      <div className="mb-6">
        <Link href="/applications">
          <Button variant="ghost" className="mb-4">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Applications
          </Button>
        </Link>
        <h1 className="text-3xl font-bold">Application Details</h1>
        <p className="text-sm text-muted-foreground mt-1">
          View and manage your job application
        </p>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-950 border border-red-200 dark:border-red-800 text-red-800 dark:text-red-200 px-4 py-3 rounded mb-6">
          {error}
        </div>
      )}

      {successMessage && (
        <div className="bg-green-50 dark:bg-green-950 border border-green-200 dark:border-green-800 text-green-800 dark:text-green-200 px-4 py-3 rounded mb-6">
          {successMessage}
        </div>
      )}

      <div className="grid gap-6">
        {/* Job Details Card */}
        <Card>
          <CardHeader>
            <CardTitle>Job Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label className="text-muted-foreground">Position</Label>
              <p className="text-xl font-semibold">{job.title}</p>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label className="text-muted-foreground">Company</Label>
                <p className="font-medium">{job.company}</p>
              </div>
              <div>
                <Label className="text-muted-foreground">Location</Label>
                <p className="font-medium">{job.location}</p>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label className="text-muted-foreground">Job Type</Label>
                <p className="font-medium">{getJobTypeLabel(job.jobType)}</p>
              </div>
              <div>
                <Label className="text-muted-foreground">Posted</Label>
                <p className="font-medium">
                  {new Date(job.createdAt).toLocaleDateString()}
                </p>
              </div>
            </div>
            <div>
              <Label className="text-muted-foreground">Description</Label>
              <p className="mt-2 text-sm whitespace-pre-wrap">{job.description}</p>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label className="text-muted-foreground">Sponsorship</Label>
                <p className="font-medium">
                  {job.offerSponsorship ? "Available" : "Not Available"}
                </p>
              </div>
              <div>
                <Label className="text-muted-foreground">Relocation</Label>
                <p className="font-medium">
                  {job.offerRelocation ? "Available" : "Not Available"}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Application Status Card */}
        <Card>
          <CardHeader>
            <CardTitle>Application Status</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label className="text-muted-foreground">Current Status</Label>
                <p className="font-medium text-lg">
                  {getStatusLabel(application.status)}
                </p>
              </div>
              <div>
                <Label className="text-muted-foreground">Applied On</Label>
                <p className="font-medium">
                  {new Date(application.created).toLocaleDateString()}
                </p>
              </div>
            </div>
            <div>
              <Label className="text-muted-foreground">Last Updated</Label>
              <p className="font-medium">
                {new Date(application.updated).toLocaleDateString()}
              </p>
            </div>

            <div className="pt-4 border-t">
              <Label htmlFor="status">Update Status</Label>
              <div className="flex gap-4 mt-2">
                <select
                  id="status"
                  value={selectedStatus}
                  onChange={(e) => setSelectedStatus(Number(e.target.value))}
                  className="flex-1 px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                >
                  <option value={JobApplicationStatus.NotApplied}>
                    Not Applied
                  </option>
                  <option value={JobApplicationStatus.Applied}>Applied</option>
                  <option value={JobApplicationStatus.InProcess}>
                    In Process
                  </option>
                  <option value={JobApplicationStatus.WaitingFeedback}>
                    Waiting Feedback
                  </option>
                  <option value={JobApplicationStatus.WaitingJobOffer}>
                    Waiting Job Offer
                  </option>
                  <option value={JobApplicationStatus.Failed}>Failed</option>
                </select>
                <Button
                  onClick={handleStatusUpdate}
                  disabled={isUpdating || selectedStatus === application.status}
                >
                  {isUpdating ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Updating...
                    </>
                  ) : (
                    <>
                      <Save className="h-4 w-4 mr-2" />
                      Update Status
                    </>
                  )}
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
