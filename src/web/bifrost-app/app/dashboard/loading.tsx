/**
 * Dashboard Loading State
 * Skeleton UI for dashboard page
 */

import { Card, CardContent, CardHeader } from "@/components/ui/card";


export default function Loading() {
  return (
    <div className="container mx-auto p-6 space-y-6">
      <div>
        <div className="h-9 bg-gray-200 dark:bg-gray-700 rounded w-64 mb-2 animate-pulse"></div>
        <div className="h-5 bg-gray-200 dark:bg-gray-700 rounded w-48 animate-pulse"></div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {[...Array(5)].map((_, i) => (
          <Card key={i} className="animate-pulse">
            <CardHeader>
              <div className="h-6 bg-gray-200 dark:bg-gray-700 rounded w-32"></div>
            </CardHeader>
            <CardContent>
              <div className="h-12 bg-gray-200 dark:bg-gray-700 rounded w-16 mb-4"></div>
              <div className="space-y-2">
                <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-full"></div>
                <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-5/6"></div>
                <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-4/6"></div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
