/**
 * Jobs Page
 * Displays list of jobs with CRUD operations
 * Uses custom hooks and server actions
 */

"use client";

import React, { useEffect } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/hooks";
import { getUserJobsAction, deleteJobAction } from "@/lib/actions/jobs";
import { Job, JobType } from "@/lib/types";
import { MESSAGES } from "@/lib/constants";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { DeleteConfirmDialog } from "@/components/delete-confirm-dialog";
import { Loader2, Plus, Edit, Trash2, Briefcase, MapPin, Building2 } from "lucide-react";

const JOB_TYPE_LABELS: Record<JobType, string> = {
  [JobType.FullTime]: "Full-time",
  [JobType.PartTime]: "Part-time",
  [JobType.Contract]: "Contract",
  [JobType.Freelance]: "Freelance",
  [JobType.Internship]: "Internship",
  [JobType.Temporary]: "Temporary",
};

export default function JobsPage() {
  const { user, isLoading: authLoading } = useAuth();
  const [jobs, setJobs] = React.useState<Job[]>([]);
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [deletingId, setDeletingId] = React.useState<number | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [jobToDelete, setJobToDelete] = React.useState<Job | null>(null);

  useEffect(() => {
    if (user) {
      fetchJobs();
    }
  }, [user]);

  const fetchJobs = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const response = await getUserJobsAction();

      if (response.error) {
        setError(response.error);
        return;
      }

      if (response.data) {
        setJobs(response.data);
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : MESSAGES.UNKNOWN_ERROR
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteClick = (job: Job) => {
    setJobToDelete(job);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!jobToDelete) return;

    try {
      setDeletingId(jobToDelete.id);
      const response = await deleteJobAction(jobToDelete.id.toString());

      if (response.error) {
        setError(response.error);
        return;
      }

      // Remove from local state
      setJobs((prev) => prev.filter((job) => job.id !== jobToDelete.id));
      setDeleteDialogOpen(false);
      setJobToDelete(null);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : MESSAGES.UNKNOWN_ERROR
      );
    } finally {
      setDeletingId(null);
    }
  };

  if (authLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Jobs</h1>
          <p className="text-muted-foreground mt-2">
            Manage your job opportunities
          </p>
        </div>
        <Link href="/jobs/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            New Job
          </Button>
        </Link>
      </div>

      {error && (
        <div className="mb-6 rounded-lg border border-destructive/50 bg-destructive/10 p-4">
          <p className="text-sm text-destructive">{error}</p>
        </div>
      )}

      {isLoading ? (
        <div className="flex justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : jobs.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Briefcase className="h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-semibold mb-2">No jobs yet</h3>
            <p className="text-muted-foreground text-center mb-4">
              Start by adding your first job opportunity
            </p>
            <Link href="/jobs/new">
              <Button>
                <Plus className="mr-2 h-4 w-4" />
                Create Job
              </Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {jobs.map((job) => (
            <Card key={job.id} className="flex flex-col">
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <CardTitle className="text-xl mb-2">{job.title}</CardTitle>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1">
                      <Building2 className="h-4 w-4" />
                      <span>{job.company}</span>
                    </div>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <MapPin className="h-4 w-4" />
                      <span>{job.location}</span>
                    </div>
                  </div>
                </div>
              </CardHeader>
              <CardContent className="flex-1 flex flex-col">
                <div className="flex flex-wrap gap-2 mb-4">
                  <Badge variant="secondary">
                    {JOB_TYPE_LABELS[job.jobType as JobType]}
                  </Badge>
                  {job.offerSponsorship && (
                    <Badge variant="outline" className="border-blue-500 text-blue-600">
                      Sponsorship
                    </Badge>
                  )}
                  {job.offerRelocation && (
                    <Badge variant="outline" className="border-green-500 text-green-600">
                      Relocation
                    </Badge>
                  )}
                </div>

                {job.description && (
                  <p className="text-sm text-muted-foreground mb-4 line-clamp-3">
                    {job.description}
                  </p>
                )}

                <div className="flex gap-2 mt-auto pt-4 border-t">
                  <Link href={`/jobs/${job.id}`} className="flex-1">
                    <Button variant="outline" className="w-full">
                      <Edit className="mr-2 h-4 w-4" />
                      Edit
                    </Button>
                  </Link>
                  <Button
                    variant="outline"
                    size="icon"
                    onClick={() => handleDeleteClick(job)}
                    disabled={deletingId === job.id}
                    className="text-destructive hover:bg-destructive hover:text-destructive-foreground"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <DeleteConfirmDialog
        isOpen={deleteDialogOpen}
        onClose={() => {
          setDeleteDialogOpen(false);
          setJobToDelete(null);
        }}
        onConfirm={handleDeleteConfirm}
        title="Delete Job"
        description={`Are you sure you want to delete "${jobToDelete?.title}"? This will also delete all related applications. This action cannot be undone.`}
        isDeleting={deletingId !== null}
      />
    </div>
  );
}
