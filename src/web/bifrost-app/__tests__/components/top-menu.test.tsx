import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { TopMenu } from "@/components/top-menu";
import { useRouter, usePathname } from "next/navigation";
import { authService } from "@/lib/api/auth";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  useRouter: jest.fn(),
  usePathname: jest.fn(),
}));

// Mock auth service
jest.mock("@/lib/api/auth", () => ({
  authService: {
    logout: jest.fn(),
  },
}));

describe("TopMenu Component", () => {
  const mockPush = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();

    (useRouter as jest.Mock).mockReturnValue({
      push: mockPush,
    });

    (usePathname as jest.Mock).mockReturnValue("/dashboard");

    // Mock localStorage
    Object.defineProperty(window, "localStorage", {
      value: {
        removeItem: jest.fn(),
        getItem: jest.fn(),
        setItem: jest.fn(),
        clear: jest.fn(),
      },
      writable: true,
    });
  });

  it("should render the TopMenu component", () => {
    render(<TopMenu />);

    expect(screen.getByText("Bifrost")).toBeInTheDocument();
    expect(screen.getByText("Jobs")).toBeInTheDocument();
    expect(screen.getByText("Job Applications")).toBeInTheDocument();
    expect(screen.getByText("My Preferences")).toBeInTheDocument();
    expect(screen.getByText("Logout")).toBeInTheDocument();
  });

  it("should have correct navigation links", () => {
    render(<TopMenu />);

    const jobsLink = screen.getByRole("link", { name: /view all jobs/i });
    const applicationsLink = screen.getByRole("link", { name: /view job applications/i });
    const preferencesLink = screen.getByRole("link", { name: /manage user preferences/i });

    expect(jobsLink).toHaveAttribute("href", "/jobs");
    expect(applicationsLink).toHaveAttribute("href", "/applications");
    expect(preferencesLink).toHaveAttribute("href", "/preferences");
  });

  it("should highlight active navigation link", () => {
    (usePathname as jest.Mock).mockReturnValue("/applications");

    render(<TopMenu />);

    const applicationsLink = screen.getByRole("link", { name: /view job applications/i });

    expect(applicationsLink).toHaveClass("bg-primary");
    expect(applicationsLink).toHaveClass("text-primary-foreground");
    expect(applicationsLink).toHaveAttribute("aria-current", "page");
  });

  it("should not highlight inactive navigation links", () => {
    (usePathname as jest.Mock).mockReturnValue("/applications");

    render(<TopMenu />);

    const jobsLink = screen.getByRole("link", { name: /view all jobs/i });
    const preferencesLink = screen.getByRole("link", { name: /manage user preferences/i });

    expect(jobsLink).toHaveClass("text-muted-foreground");
    expect(preferencesLink).toHaveClass("text-muted-foreground");
    expect(jobsLink).not.toHaveAttribute("aria-current");
    expect(preferencesLink).not.toHaveAttribute("aria-current");
  });

  it("should render brand logo with correct link", () => {
    render(<TopMenu />);

    const logo = screen.getByRole("link", { name: /bifrost - go to dashboard/i });

    expect(logo).toHaveAttribute("href", "/dashboard");
    expect(logo).toHaveTextContent("Bifrost");
  });

  it("should handle logout correctly", async () => {
    (authService.logout as jest.Mock).mockResolvedValue(undefined);

    render(<TopMenu />);

    const logoutButton = screen.getByRole("button", { name: /logout from application/i });

    fireEvent.click(logoutButton);

    await waitFor(() => {
      expect(authService.logout).toHaveBeenCalledTimes(1);
      expect(mockPush).toHaveBeenCalledWith("/auth/login");
    });
  });

  it("should handle logout error gracefully", async () => {
    const consoleErrorSpy = jest.spyOn(console, "error").mockImplementation(() => {});
    const logoutError = new Error("Logout failed");
    (authService.logout as jest.Mock).mockRejectedValue(logoutError);

    render(<TopMenu />);

    const logoutButton = screen.getByRole("button", { name: /logout from application/i });

    fireEvent.click(logoutButton);

    await waitFor(() => {
      expect(consoleErrorSpy).toHaveBeenCalledWith("Logout error:", logoutError);
    });

    consoleErrorSpy.mockRestore();
  });

  it("should render all menu icons", () => {
    const { container } = render(<TopMenu />);

    // Check that SVG icons are present (they have aria-hidden="true")
    const icons = container.querySelectorAll('svg[aria-hidden="true"]');

    // We expect 4 icons: BriefcaseIcon, ListIcon, SettingsIcon, LogOut icon
    expect(icons.length).toBe(4);
  });

  it("should apply custom className prop", () => {
    const { container } = render(<TopMenu className="custom-class" />);

    const nav = container.querySelector("nav");

    expect(nav).toHaveClass("custom-class");
  });

  it("should have proper accessibility attributes", () => {
    render(<TopMenu />);

    const nav = screen.getByRole("navigation", { name: /main navigation/i });

    expect(nav).toBeInTheDocument();
    expect(nav).toHaveAttribute("aria-label", "Main navigation");
  });

  it("should render logout button with correct styling", () => {
    render(<TopMenu />);

    const logoutButton = screen.getByRole("button", { name: /logout from application/i });

    expect(logoutButton).toHaveClass("flex");
    expect(logoutButton).toHaveClass("items-center");
    expect(logoutButton).toHaveClass("gap-2");
  });

  describe("Navigation active state", () => {
    const testCases = [
      { path: "/jobs", expectedActive: "Jobs" },
      { path: "/applications", expectedActive: "Job Applications" },
      { path: "/preferences", expectedActive: "My Preferences" },
    ];

    testCases.forEach(({ path, expectedActive }) => {
      it(`should highlight ${expectedActive} when on ${path}`, () => {
        (usePathname as jest.Mock).mockReturnValue(path);

        render(<TopMenu />);

        const activeLink = screen.getByText(expectedActive).closest("a");

        expect(activeLink).toHaveClass("bg-primary");
        expect(activeLink).toHaveAttribute("aria-current", "page");
      });
    });
  });
});
