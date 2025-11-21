import { getUserJobs, getJob } from "@/lib/api/jobs";
import { Job } from "@/lib/types";

// Mock fetch globally
global.fetch = jest.fn();

describe("Jobs API Client", () => {
  const mockToken = "mock-token-123";
  const mockUserId = "123e4567-e89b-12d3-a456-426614174000";

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("getUserJobs", () => {
    it("should fetch user jobs successfully", async () => {
      const mockJobs: Job[] = [
        {
          id: 1,
          title: "Software Engineer",
          company: "Tech Corp",
          location: "Remote",
          jobType: 0,
          description: "Great opportunity",
          offerSponsorship: true,
          offerRelocation: false,
          userId: mockUserId,
          createdAt: "2025-01-01T00:00:00Z",
          updatedAt: null,
        },
        {
          id: 2,
          title: "Senior Developer",
          company: "Startup Inc",
          location: "New York",
          jobType: 0,
          description: "Exciting role",
          offerSponsorship: false,
          offerRelocation: true,
          userId: mockUserId,
          createdAt: "2025-01-02T00:00:00Z",
          updatedAt: null,
        },
      ];

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockJobs,
      });

      const result = await getUserJobs(mockUserId, mockToken);

      expect(global.fetch).toHaveBeenCalledWith(
        `https://localhost:7001/api/jobs/user/${mockUserId}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );

      expect(result.data).toEqual(mockJobs);
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

      const result = await getUserJobs(mockUserId, mockToken);

      expect(result.data).toBeUndefined();
      expect(result.error).toBe("Invalid user ID format");
    });

    it("should handle API error response without ProblemDetails", async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        statusText: "Internal Server Error",
        json: async () => {
          throw new Error("Invalid JSON");
        },
      });

      const result = await getUserJobs(mockUserId, mockToken);

      expect(result.data).toBeUndefined();
      expect(result.error).toBe(
        "Failed to fetch jobs: Internal Server Error"
      );
    });

    it("should handle network errors", async () => {
      (global.fetch as jest.Mock).mockRejectedValueOnce(
        new Error("Network error")
      );

      const result = await getUserJobs(mockUserId, mockToken);

      expect(result.data).toBeUndefined();
      expect(result.error).toBe("Network error");
    });
  });

  describe("getJob", () => {
    it("should fetch a single job successfully", async () => {
      const mockJob: Job = {
        id: 1,
        title: "Software Engineer",
        company: "Tech Corp",
        location: "Remote",
        jobType: 0,
        description: "Great opportunity",
        offerSponsorship: true,
        offerRelocation: false,
        userId: mockUserId,
        createdAt: "2025-01-01T00:00:00Z",
        updatedAt: null,
      };

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockJob,
      });

      const result = await getJob(1, mockToken);

      expect(global.fetch).toHaveBeenCalledWith(
        "https://localhost:7001/api/jobs/1",
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );

      expect(result.data).toEqual(mockJob);
      expect(result.error).toBeUndefined();
    });

    it("should handle 404 not found error", async () => {
      const mockProblemDetails = {
        type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        title: "Not Found",
        status: 404,
        detail: "Job not found",
      };

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        statusText: "Not Found",
        json: async () => mockProblemDetails,
      });

      const result = await getJob(999, mockToken);

      expect(result.data).toBeUndefined();
      expect(result.error).toBe("Job not found");
    });
  });
});
