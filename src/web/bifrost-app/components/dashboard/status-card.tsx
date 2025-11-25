/**
 * Status Card Component
 * Displays application status summary with count and recent applications
 */

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { JobApplication, Job, JobApplicationStatus } from "@/lib/types";
import { LIMITS } from "@/lib/constants";

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

export function StatusCard({ title, count, applications, color }: StatusCardProps) {
  return (
    <Card className="hover:shadow-lg transition-shadow">
      <CardHeader>
        <CardTitle className="text-lg font-semibold flex items-center gap-2">
          <div
            className={`w-3 h-3 rounded-full ${color}`}
            role="presentation"
            aria-label={`Status indicator for ${title}`}
          ></div>
          {title}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div
          className="text-3xl font-bold mb-4"
          aria-label={`${count} applications in ${title} status`}
        >
          {count}
        </div>
        {applications.length > 0 && (
          <div className="space-y-2">
            <p className="text-sm text-muted-foreground">Recent applications:</p>
            <ul className="space-y-1" role="list">
              {applications.slice(0, LIMITS.DASHBOARD_RECENT_APPLICATIONS).map((app) => (
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
