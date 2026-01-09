/**
 * Edit Job Page
 * Form for editing an existing job opportunity
 */

"use client";

import React, { useEffect, useCallback } from "react";
import { useRouter, useParams } from "next/navigation";
import { useAuth } from "@/lib/hooks";
import { getJobByIdAction, updateJobAction } from "@/lib/actions/jobs";
import { JobType } from "@/lib/types";
import { MESSAGES } from "@/lib/constants";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Loader2, ArrowLeft } from "lucide-react";
import Link from "next/link";

const JOB_TYPE_OPTIONS = [
  { value: JobType.FullTime, label: "Full-time" },
  { value: JobType.PartTime, label: "Part-time" },
  { value: JobType.Contract, label: "Contract" },
  { value: JobType.Freelance, label: "Freelance" },
  { value: JobType.Internship, label: "Internship" },
  { value: JobType.Temporary, label: "Temporary" },
];

export default function EditJobPage() {
  const router = useRouter();
  const params = useParams();
  const jobId = params.id as string;
  const { user, isLoading: authLoading } = useAuth();
  const [isLoading, setIsLoading] = React.useState(true);
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  const [formData, setFormData] = React.useState({
    title: "",
    company: "",
    location: "",
    jobType: JobType.FullTime,
    description: "",
    offerSponsorship: false,
    offerRelocation: false,
  });

  const fetchJob = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      const response = await getJobByIdAction(jobId);

      if (response.error) {
        setError(response.error);
        return;
      }

      if (response.data) {
        setFormData({
          title: response.data.title,
          company: response.data.company,
          location: response.data.location,
          jobType: response.data.jobType,
          description: response.data.description || "",
          offerSponsorship: response.data.offerSponsorship,
          offerRelocation: response.data.offerRelocation,
        });
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : MESSAGES.UNKNOWN_ERROR
      );
    } finally {
      setIsLoading(false);
    }
  }, [jobId]);

  useEffect(() => {
    if (user && jobId) {
      fetchJob();
    }
  }, [user, jobId, fetchJob]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validate required fields
    if (!formData.title.trim()) {
      setError("Job title is required");
      return;
    }
    if (!formData.company.trim()) {
      setError("Company name is required");
      return;
    }
    if (!formData.location.trim()) {
      setError("Location is required");
      return;
    }

    try {
      setIsSubmitting(true);

      const response = await updateJobAction(jobId, {
        title: formData.title.trim(),
        company: formData.company.trim(),
        location: formData.location.trim(),
        jobType: formData.jobType,
        description: formData.description.trim(),
        offerSponsorship: formData.offerSponsorship,
        offerRelocation: formData.offerRelocation,
      });

      if (response.error) {
        setError(response.error);
        return;
      }

      // Redirect to jobs list
      router.push("/jobs");
    } catch (err) {
      setError(
        err instanceof Error ? err.message : MESSAGES.UNKNOWN_ERROR
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === "jobType" ? Number(value) : value,
    }));
  };

  const handleCheckboxChange = (name: string, checked: boolean) => {
    setFormData((prev) => ({
      ...prev,
      [name]: checked,
    }));
  };

  if (authLoading || isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4 max-w-2xl">
      <div className="mb-8">
        <Link href="/jobs">
          <Button variant="ghost" size="sm" className="mb-4">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Jobs
          </Button>
        </Link>
        <h1 className="text-3xl font-bold tracking-tight">Edit Job</h1>
        <p className="text-muted-foreground mt-2">
          Update the job opportunity details
        </p>
      </div>

      {error && (
        <div className="mb-6 rounded-lg border border-destructive/50 bg-destructive/10 p-4">
          <p className="text-sm text-destructive">{error}</p>
        </div>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Job Details</CardTitle>
          <CardDescription>
            Update the details of the job opportunity
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="title">
                Job Title <span className="text-destructive">*</span>
              </Label>
              <Input
                id="title"
                name="title"
                value={formData.title}
                onChange={handleChange}
                placeholder="e.g. Senior Software Engineer"
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="company">
                Company <span className="text-destructive">*</span>
              </Label>
              <Input
                id="company"
                name="company"
                value={formData.company}
                onChange={handleChange}
                placeholder="e.g. Acme Corp"
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="location">
                Location <span className="text-destructive">*</span>
              </Label>
              <Input
                id="location"
                name="location"
                value={formData.location}
                onChange={handleChange}
                placeholder="e.g. San Francisco, CA or Remote"
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="jobType">Job Type</Label>
              <select
                id="jobType"
                name="jobType"
                value={formData.jobType}
                onChange={handleChange}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {JOB_TYPE_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Description</Label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleChange}
                placeholder="Job description, requirements, responsibilities..."
                rows={5}
                className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              />
            </div>

            <div className="space-y-4">
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="offerSponsorship"
                  checked={formData.offerSponsorship}
                  onCheckedChange={(checked) =>
                    handleCheckboxChange("offerSponsorship", checked === true)
                  }
                />
                <Label
                  htmlFor="offerSponsorship"
                  className="text-sm font-normal cursor-pointer"
                >
                  Offers visa sponsorship
                </Label>
              </div>

              <div className="flex items-center space-x-2">
                <Checkbox
                  id="offerRelocation"
                  checked={formData.offerRelocation}
                  onCheckedChange={(checked) =>
                    handleCheckboxChange("offerRelocation", checked === true)
                  }
                />
                <Label
                  htmlFor="offerRelocation"
                  className="text-sm font-normal cursor-pointer"
                >
                  Offers relocation assistance
                </Label>
              </div>
            </div>

            <div className="flex gap-4 pt-4">
              <Button
                type="submit"
                disabled={isSubmitting}
                className="flex-1"
              >
                {isSubmitting ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Saving...
                  </>
                ) : (
                  "Save Changes"
                )}
              </Button>
              <Link href="/jobs" className="flex-1">
                <Button type="button" variant="outline" className="w-full">
                  Cancel
                </Button>
              </Link>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
