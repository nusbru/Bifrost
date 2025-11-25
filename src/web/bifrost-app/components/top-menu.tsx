"use client";

import { useRouter, usePathname } from "next/navigation";
import { createClient } from "@/lib/supabase/client";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import { LogOut, BriefcaseIcon, ListIcon, SettingsIcon, PlusIcon } from "lucide-react";

interface TopMenuProps {
  className?: string;
}

/**
 * TopMenu component - Displays navigation menu across all application pages
 * Follows Single Responsibility Principle by focusing only on navigation
 */
export function TopMenu({ className = "" }: TopMenuProps) {
  const router = useRouter();
  const pathname = usePathname();

  const handleLogout = async () => {
    try {
      const supabase = createClient();
      await supabase.auth.signOut();
      // Session is automatically cleared from cookies
      router.push("/auth/login");
    } catch (error) {
      // Error is logged automatically if using logger, but we can keep this for immediate feedback
      console.error("Logout error:", error);
    }
  };

  const isActive = (path: string) => pathname === path;

  const menuItems = [
    {
      href: "/jobs",
      label: "Jobs",
      icon: BriefcaseIcon,
      ariaLabel: "View all jobs",
    },
    {
      href: "/applications",
      label: "Job Applications",
      icon: ListIcon,
      ariaLabel: "View job applications",
    },
    {
      href: "/preferences",
      label: "My Preferences",
      icon: SettingsIcon,
      ariaLabel: "Manage user preferences",
    },
  ];

  return (
    <nav
      className={`bg-background border-b border-border ${className}`}
      role="navigation"
      aria-label="Main navigation"
    >
      <div className="container mx-auto px-6 py-4">
        <div className="flex items-center justify-between">
          {/* Logo/Brand */}
          <Link
            href="/dashboard"
            className="text-2xl font-bold text-primary hover:text-primary/80 transition-colors"
            aria-label="Bifrost - Go to dashboard"
          >
            Bifrost
          </Link>

          {/* Navigation Links */}
          <div className="flex items-center gap-6">
            {menuItems.map((item) => {
              const Icon = item.icon;
              const active = isActive(item.href);

              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-2 px-3 py-2 rounded-md text-sm font-medium transition-all duration-200 ${
                    active
                      ? "bg-primary text-primary-foreground shadow-sm"
                      : "text-muted-foreground hover:text-foreground hover:bg-accent"
                  }`}
                  aria-label={item.ariaLabel}
                  aria-current={active ? "page" : undefined}
                  prefetch={true}
                >
                  <Icon className="h-4 w-4" aria-hidden="true" />
                  <span>{item.label}</span>
                </Link>
              );
            })}

            {/* Logout Button */}
            <Button
              onClick={handleLogout}
              variant="outline"
              size="sm"
              className="flex items-center gap-2"
              aria-label="Logout from application"
            >
              <LogOut className="h-4 w-4" aria-hidden="true" />
              <span>Logout</span>
            </Button>
          </div>
        </div>
      </div>
    </nav>
  );
}
