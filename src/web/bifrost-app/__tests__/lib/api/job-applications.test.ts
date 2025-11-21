import {
  getUserJobApplications,
  getJobApplication,
  getJobApplications,
} from "@/lib/api/job-applications";
import { JobApplication } from "@/lib/types";

// Mock fetch globally
global.fetch = jest.fn();

describe("Job Applications API Client", () => {
  const mockToken = "mock-token-123";
  const mockUserId = "123e4567-e89b-12d3-a456-426614174000";

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("getUserJobApplications", () => {
    it("should fetch user job applications successfully", async () => {
      const mockApplications: JobApplication[] = [
        {
          id: 1,
          jobId: 1,
          userId: mockUserId,
          status: 1,
          created: "2025-01-01T00:00:00Z",
          updated: "2025-01-01T00:00:00Z",
        },
        {
          id: 2,
          jobId: 2,
          userId: mockUserId,
          status: 2,
          created: "2025-01-02T00:00:00Z",
          updated: "2025-01-02T00:00:00Z",
        },
      ];

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockApplications,
      });

      const result = await getUserJobApplications(mockUserId, mockToken);

      expect(global.fetch).toHaveBeenCalledWith(
        `http://localhost:5037/api/applications/user/${mockUserId}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );

      expect(result.data).toEqual(mockApplications);
      expect(result.error).toBeUndefined();
    });

    it("should handle API error response with ProblemDetails", async () => {
      const mockProblemDetails = {
        type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        title: "Bad Request",
        status: 400,
        detail: "Invalid user ID format",
      };

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        statusText: "Bad Request",
        json: async () => mockProblemDetails,
      });

      const result = await getUserJobApplications(mockUserId, mockToken);

      expect(result.data).toBeUndefined();
      expect(result.error).toBe("Invalid user ID format");
    });

    it("should handle network errors", async () => {
      (global.fetch as jest.Mock).mockRejectedValueOnce(
        new Error("Network error")
      );

      const result = await getUserJobApplications(mockUserId, mockToken);

      expect(result.data).toBeUndefined();
      expect(result.error).toBe("Network error");
    });
  });

  describe("getJobApplication", () => {
    it("should fetch a single job application successfully", async () => {
      const mockApplication: JobApplication = {
        id: 1,
        jobId: 1,
        userId: mockUserId,
        status: 1,
        created: "2025-01-01T00:00:00Z",
        updated: "2025-01-01T00:00:00Z",
      };

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockApplication,
      });

      const result = await getJobApplication(1, mockToken);

      expect(global.fetch).toHaveBeenCalledWith(
        "http://localhost:5037/api/applications/1",
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );

      expect(result.data).toEqual(mockApplication);
      expect(result.error).toBeUndefined();
    });

    it("should handle 404 not found error", async () => {
      const mockProblemDetails = {
        type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        title: "Not Found",
        status: 404,
        detail: "Job application not found",
      };

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        statusText: "Not Found",
        json: async () => mockProblemDetails,
      });

      const result = await getJobApplication(999, mockToken);

      expect(result.data).toBeUndefined();
      expect(result.error).toBe("Job application not found");
    });
  });

  describe("getJobApplications", () => {
    it("should fetch applications for a specific job successfully", async () => {
      const mockApplications: JobApplication[] = [
        {
          id: 1,
          jobId: 1,
          userId: mockUserId,
          status: 1,
          created: "2025-01-01T00:00:00Z",
          updated: "2025-01-01T00:00:00Z",
        },
        {
          id: 3,
          jobId: 1,
          userId: "another-user-id",
          status: 2,
          created: "2025-01-03T00:00:00Z",
          updated: "2025-01-03T00:00:00Z",
        },
      ];

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockApplications,
      });

      const result = await getJobApplications(1, mockToken);

      expect(global.fetch).toHaveBeenCalledWith(
        "http://localhost:5037/api/applications/job/1",
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );

      expect(result.data).toEqual(mockApplications);
      expect(result.error).toBeUndefined();
    });

    it("should handle errors when fetching job applications", async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        statusText: "Internal Server Error",
        json: async () => ({}),
      });

      const result = await getJobApplications(1, mockToken);

      expect(result.data).toBeUndefined();
      expect(result.error).toBe(
        "Failed to fetch job applications: Internal Server Error"
      );
    });
  });
});
