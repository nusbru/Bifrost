/**
 * Preferences Page
 * Displays and manages user job preferences with CRUD operations
 * Uses custom hooks and server actions
 */

"use client";

import React, { useEffect } from "react";
import { useAuth } from "@/lib/hooks";
import {
  getUserPreferencesAction,
  createPreferencesAction,
  updatePreferencesAction,
  deletePreferencesAction,
} from "@/lib/actions/preferences";
import { Preferences, JobType } from "@/lib/types";
import { MESSAGES } from "@/lib/constants";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Badge } from "@/components/ui/badge";
import { DeleteConfirmDialog } from "@/components/delete-confirm-dialog";
import { Loader2, Settings, DollarSign, Briefcase, Edit, Trash2, Save, X } from "lucide-react";

const JOB_TYPE_OPTIONS = [
  { value: JobType.FullTime, label: "Full-time" },
  { value: JobType.PartTime, label: "Part-time" },
  { value: JobType.Contract, label: "Contract" },
  { value: JobType.Freelance, label: "Freelance" },
  { value: JobType.Internship, label: "Internship" },
  { value: JobType.Temporary, label: "Temporary" },
];

const formatSalary = (amount: number): string => {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount);
};

export default function PreferencesPage() {
  const { user, isLoading: authLoading } = useAuth();
  const [preferences, setPreferences] = React.useState<Preferences | null>(null);
  const [isLoading, setIsLoading] = React.useState(true);
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [isEditing, setIsEditing] = React.useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);

  const [formData, setFormData] = React.useState({
    minSalary: 0,
    maxSalary: 0,
    jobType: JobType.FullTime,
    needSponsorship: false,
    needRelocation: false,
  });

  useEffect(() => {
    if (user) {
      fetchPreferences();
    }
  }, [user]);

  const fetchPreferences = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const response = await getUserPreferencesAction();

      if (response.error) {
        // If preferences don't exist, show the create form
        if (response.error.includes("Not Found") || response.error.includes("404")) {
          setPreferences(null);
          setIsEditing(true);
        } else {
          setError(response.error);
        }
        return;
      }

      if (response.data) {
        setPreferences(response.data);
        setFormData({
          minSalary: response.data.minSalary,
          maxSalary: response.data.maxSalary,
          jobType: response.data.jobType,
          needSponsorship: response.data.needSponsorship,
          needRelocation: response.data.needRelocation,
        });
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : MESSAGES.UNKNOWN_ERROR
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validate salary range
    if (formData.minSalary < 0) {
      setError("Minimum salary must be positive");
      return;
    }
    if (formData.maxSalary < formData.minSalary) {
      setError("Maximum salary must be greater than minimum salary");
      return;
    }

    try {
      setIsSubmitting(true);

      let response;
      if (preferences) {
        // Update existing preferences
        response = await updatePreferencesAction(preferences.id.toString(), formData);
      } else {
        // Create new preferences
        response = await createPreferencesAction({
          ...formData,
          userId: user?.id || "",
        });
      }

      if (response.error) {
        setError(response.error);
        return;
      }

      if (response.data) {
        setPreferences(response.data);
        setIsEditing(false);
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : MESSAGES.UNKNOWN_ERROR
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!preferences) return;

    try {
      setIsSubmitting(true);
      const response = await deletePreferencesAction(preferences.id.toString());

      if (response.error) {
        setError(response.error);
        return;
      }

      setPreferences(null);
      setDeleteDialogOpen(false);
      setIsEditing(true);
      setFormData({
        minSalary: 0,
        maxSalary: 0,
        jobType: JobType.FullTime,
        needSponsorship: false,
        needRelocation: false,
      });
    } catch (err) {
      setError(
        err instanceof Error ? err.message : MESSAGES.UNKNOWN_ERROR
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === "jobType" ? Number(value) : name === "minSalary" || name === "maxSalary" ? parseFloat(value) || 0 : value,
    }));
  };

  const handleCheckboxChange = (name: string, checked: boolean) => {
    setFormData((prev) => ({
      ...prev,
      [name]: checked,
    }));
  };

  const handleCancel = () => {
    if (preferences) {
      setFormData({
        minSalary: preferences.minSalary,
        maxSalary: preferences.maxSalary,
        jobType: preferences.jobType,
        needSponsorship: preferences.needSponsorship,
        needRelocation: preferences.needRelocation,
      });
      setIsEditing(false);
    }
    setError(null);
  };

  if (authLoading || isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4 max-w-4xl">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">My Preferences</h1>
        <p className="text-muted-foreground mt-2">
          Manage your job search preferences
        </p>
      </div>

      {error && (
        <div className="mb-6 rounded-lg border border-destructive/50 bg-destructive/10 p-4">
          <p className="text-sm text-destructive">{error}</p>
        </div>
      )}

      {!isEditing && preferences ? (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle className="flex items-center gap-2">
                  <Settings className="h-5 w-5" />
                  Current Preferences
                </CardTitle>
                <CardDescription className="mt-2">
                  Your job search preferences
                </CardDescription>
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setIsEditing(true)}
                >
                  <Edit className="mr-2 h-4 w-4" />
                  Edit
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setDeleteDialogOpen(true)}
                  className="text-destructive hover:bg-destructive hover:text-destructive-foreground"
                >
                  <Trash2 className="mr-2 h-4 w-4" />
                  Delete
                </Button>
              </div>
            </div>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid gap-6 md:grid-cols-2">
              <div className="space-y-2">
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <DollarSign className="h-4 w-4" />
                  <span className="font-medium">Salary Range</span>
                </div>
                <div className="text-lg font-semibold">
                  {formatSalary(preferences.minSalary)} - {formatSalary(preferences.maxSalary)}
                </div>
              </div>

              <div className="space-y-2">
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Briefcase className="h-4 w-4" />
                  <span className="font-medium">Preferred Job Type</span>
                </div>
                <div>
                  <Badge variant="secondary">
                    {JOB_TYPE_OPTIONS.find((opt) => opt.value === preferences.jobType)?.label}
                  </Badge>
                </div>
              </div>
            </div>

            <div className="space-y-3 pt-4 border-t">
              <h3 className="text-sm font-medium">Additional Requirements</h3>
              <div className="flex flex-wrap gap-3">
                {preferences.needSponsorship && (
                  <Badge variant="outline" className="border-blue-500 text-blue-600">
                    Needs Visa Sponsorship
                  </Badge>
                )}
                {preferences.needRelocation && (
                  <Badge variant="outline" className="border-green-500 text-green-600">
                    Needs Relocation Assistance
                  </Badge>
                )}
                {!preferences.needSponsorship && !preferences.needRelocation && (
                  <span className="text-sm text-muted-foreground">No additional requirements</span>
                )}
              </div>
            </div>

            <div className="text-xs text-muted-foreground pt-4 border-t">
              Last updated: {new Date(preferences.updatedAt || preferences.createdAt).toLocaleDateString()}
            </div>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>
              {preferences ? "Edit Preferences" : "Create Preferences"}
            </CardTitle>
            <CardDescription>
              {preferences
                ? "Update your job search preferences"
                : "Set your job search preferences to help track relevant opportunities"}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-6">
              <div className="grid gap-6 md:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="minSalary">
                    Minimum Salary (USD) <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="minSalary"
                    name="minSalary"
                    type="number"
                    min="0"
                    step="1000"
                    value={formData.minSalary}
                    onChange={handleChange}
                    placeholder="e.g. 80000"
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="maxSalary">
                    Maximum Salary (USD) <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="maxSalary"
                    name="maxSalary"
                    type="number"
                    min="0"
                    step="1000"
                    value={formData.maxSalary}
                    onChange={handleChange}
                    placeholder="e.g. 150000"
                    required
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="jobType">Preferred Job Type</Label>
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

              <div className="space-y-4">
                <Label>Additional Requirements</Label>
                <div className="space-y-3">
                  <div className="flex items-center space-x-2">
                    <Checkbox
                      id="needSponsorship"
                      checked={formData.needSponsorship}
                      onCheckedChange={(checked) =>
                        handleCheckboxChange("needSponsorship", checked === true)
                      }
                    />
                    <Label
                      htmlFor="needSponsorship"
                      className="text-sm font-normal cursor-pointer"
                    >
                      I need visa sponsorship
                    </Label>
                  </div>

                  <div className="flex items-center space-x-2">
                    <Checkbox
                      id="needRelocation"
                      checked={formData.needRelocation}
                      onCheckedChange={(checked) =>
                        handleCheckboxChange("needRelocation", checked === true)
                      }
                    />
                    <Label
                      htmlFor="needRelocation"
                      className="text-sm font-normal cursor-pointer"
                    >
                      I need relocation assistance
                    </Label>
                  </div>
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
                      {preferences ? "Saving..." : "Creating..."}
                    </>
                  ) : (
                    <>
                      <Save className="mr-2 h-4 w-4" />
                      {preferences ? "Save Changes" : "Create Preferences"}
                    </>
                  )}
                </Button>
                {preferences && (
                  <Button
                    type="button"
                    variant="outline"
                    onClick={handleCancel}
                    disabled={isSubmitting}
                    className="flex-1"
                  >
                    <X className="mr-2 h-4 w-4" />
                    Cancel
                  </Button>
                )}
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      <DeleteConfirmDialog
        isOpen={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
        onConfirm={handleDelete}
        title="Delete Preferences"
        description="Are you sure you want to delete your preferences? This action cannot be undone."
        isDeleting={isSubmitting}
      />
    </div>
  );
}
