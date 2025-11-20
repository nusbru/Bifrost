import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import DashboardPage from "@/app/dashboard/page";
import { useRouter } from "next/navigation";
import { getUserJobApplications } from "@/lib/api/job-applications";
import { JobApplicationStatus } from "@/lib/types";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  useRouter: jest.fn(),
}));

// Mock Supabase client
jest.mock("@/lib/supabase/client", () => ({
  createClient: jest.fn(() => ({
    auth: {
      getUser: jest.fn(),
      signOut: jest.fn(),
    },
  })),
}));

// Mock API functions
jest.mock("@/lib/api/job-applications", () => ({
  getUserJobApplications: jest.fn(),
}));

describe("DashboardPage Component", () => {
  const mockPush = jest.fn();
  const mockUserInfo = {
    id: "mock-user-id",
    email: "test@example.com",
    accessToken: "mock-token",
    refreshToken: "mock-refresh",
  };

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({
      push: mockPush,
    });

    // Mock localStorage
    Object.defineProperty(window, "localStorage", {
      value: {
        getItem: jest.fn((key: string) => {
          if (key === "userInfo") return JSON.stringify(mockUserInfo);
          return null;
        }),
        setItem: jest.fn(),
        removeItem: jest.fn(),
        clear: jest.fn(),
      },
      writable: true,
    });
  });

  it("should display loading state initially", () => {
    const { createClient } = require("@/lib/supabase/client");
    createClient.mockReturnValue({
      auth: {
        getUser: jest.fn().mockResolvedValue({ data: { user: { id: "mock-user" } } }),
      },
    });

    (getUserJobApplications as jest.Mock).mockImplementation(
      () => new Promise(() => {}) // Never resolves to keep loading state
    );

    render(<DashboardPage />);

    // Check for the loading spinner SVG
    const loader = screen.getByText((content, element) => {
      return element?.classList.contains('lucide-loader-circle') ?? false;
    });
    expect(loader).toBeInTheDocument();
  });  it("should redirect to login if user is not authenticated", async () => {
    const { createClient } = require("@/lib/supabase/client");
    createClient.mockReturnValue({
      auth: {
        getUser: jest.fn().mockResolvedValue({ data: { user: null } }),
      },
    });

    render(<DashboardPage />);

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/auth/login");
    });
  });

  it("should redirect to login if no userInfo in localStorage", async () => {
    const { createClient } = require("@/lib/supabase/client");
    createClient.mockReturnValue({
      auth: {
        getUser: jest.fn().mockResolvedValue({
          data: { user: { id: "mock-user" } }
        }),
      },
    });

    localStorage.getItem = jest.fn(() => null);

    render(<DashboardPage />);

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/auth/login");
    });
  });

  it("should display job applications grouped by status", async () => {
    const { createClient } = require("@/lib/supabase/client");
    createClient.mockReturnValue({
      auth: {
        getUser: jest.fn().mockResolvedValue({
          data: { user: { id: "mock-user" } }
        }),
      },
    });

    const mockApplications = [
      {
        id: 1,
        jobId: 101,
        userId: "mock-user",
        status: JobApplicationStatus.NotApplied,
        created: "2024-01-01",
        updated: "2024-01-01",
      },
      {
        id: 2,
        jobId: 102,
        userId: "mock-user",
        status: JobApplicationStatus.Applied,
        created: "2024-01-02",
        updated: "2024-01-02",
      },
      {
        id: 3,
        jobId: 103,
        userId: "mock-user",
        status: JobApplicationStatus.InProcess,
        created: "2024-01-03",
        updated: "2024-01-03",
      },
    ];

    (getUserJobApplications as jest.Mock).mockResolvedValue({
      data: mockApplications,
    });

    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText(/job applications dashboard/i)).toBeInTheDocument();
      // Check status cards are displayed (using getAllByText since text appears multiple times)
      expect(screen.getAllByText("Not Applied").length).toBeGreaterThan(0);
      expect(screen.getAllByText("Applied").length).toBeGreaterThan(0);
      expect(screen.getAllByText("In Process").length).toBeGreaterThan(0);
      expect(screen.getAllByText("Waiting Feedback").length).toBeGreaterThan(0);
      expect(screen.getAllByText("Waiting Job Offer").length).toBeGreaterThan(0);
    });
  });

  it("should display correct counts for each status", async () => {
    const { createClient } = require("@/lib/supabase/client");
    createClient.mockReturnValue({
      auth: {
        getUser: jest.fn().mockResolvedValue({
          data: { user: { id: "mock-user" } }
        }),
      },
    });

    const mockApplications = [
      {
        id: 1,
        jobId: 101,
        userId: "mock-user",
        status: JobApplicationStatus.Applied,
        created: "2024-01-01",
        updated: "2024-01-01",
      },
      {
        id: 2,
        jobId: 102,
        userId: "mock-user",
        status: JobApplicationStatus.Applied,
        created: "2024-01-02",
        updated: "2024-01-02",
      },
    ];

    (getUserJobApplications as jest.Mock).mockResolvedValue({
      data: mockApplications,
    });

    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText(/total applications:/i)).toBeInTheDocument();
      // Check total count
      const totalCounts = screen.getAllByText("2");
      expect(totalCounts.length).toBeGreaterThan(0);
    });
  });

  it("should display error message when API call fails", async () => {
    const { createClient } = require("@/lib/supabase/client");
    createClient.mockReturnValue({
      auth: {
        getUser: jest.fn().mockResolvedValue({
          data: { user: { id: "mock-user" } }
        }),
      },
    });

    const errorMessage = "Failed to fetch applications";
    (getUserJobApplications as jest.Mock).mockResolvedValue({
      error: errorMessage,
    });

    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });
  });

  it("should display user email in welcome message", async () => {
    const { createClient } = require("@/lib/supabase/client");
    createClient.mockReturnValue({
      auth: {
        getUser: jest.fn().mockResolvedValue({
          data: { user: { id: "mock-user" } }
        }),
      },
    });

    (getUserJobApplications as jest.Mock).mockResolvedValue({
      data: [],
    });

    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText(/welcome, test@example.com/i)).toBeInTheDocument();
    });
  });

  it("should handle logout correctly", async () => {
    const { createClient } = require("@/lib/supabase/client");
    const mockSignOut = jest.fn().mockResolvedValue({});

    createClient.mockReturnValue({
      auth: {
        getUser: jest.fn().mockResolvedValue({
          data: { user: { id: "mock-user" } }
        }),
        signOut: mockSignOut,
      },
    });

    (getUserJobApplications as jest.Mock).mockResolvedValue({
      data: [],
    });

    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText(/job applications dashboard/i)).toBeInTheDocument();
    });

    const logoutButton = screen.getByRole("button", { name: /logout/i });
    fireEvent.click(logoutButton);

    await waitFor(() => {
      expect(mockSignOut).toHaveBeenCalled();
      expect(localStorage.removeItem).toHaveBeenCalledWith("userInfo");
      expect(mockPush).toHaveBeenCalledWith("/auth/login");
    });
  });

  it("should call getUserJobApplications with correct parameters", async () => {
    const { createClient } = require("@/lib/supabase/client");
    createClient.mockReturnValue({
      auth: {
        getUser: jest.fn().mockResolvedValue({
          data: { user: { id: "mock-user" } }
        }),
      },
    });

    (getUserJobApplications as jest.Mock).mockResolvedValue({
      data: [],
    });

    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText(/job applications dashboard/i)).toBeInTheDocument();
      expect(getUserJobApplications).toHaveBeenCalledWith(
        mockUserInfo.id,
        mockUserInfo.accessToken
      );
    });
  });
});
