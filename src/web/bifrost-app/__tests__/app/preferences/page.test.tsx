/**
 * Unit tests for Preferences Page
 * Tests CRUD operations, form validation, and user interactions
 */

import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import "@testing-library/jest-dom";
import userEvent from "@testing-library/user-event";
import PreferencesPage from "@/app/preferences/page";
import * as preferencesActions from "@/lib/actions/preferences";
import * as authHook from "@/lib/hooks";
import { JobType } from "@/lib/types";

// Mock hooks and actions
jest.mock("@/lib/hooks", () => ({
  useAuth: jest.fn(),
}));

jest.mock("@/lib/actions/preferences", () => ({
  getUserPreferencesAction: jest.fn(),
  createPreferencesAction: jest.fn(),
  updatePreferencesAction: jest.fn(),
  deletePreferencesAction: jest.fn(),
}));

describe("PreferencesPage Component", () => {
  const mockUser = {
    id: "test-user-id",
    email: "test@example.com",
  };

  const mockPreferences = {
    id: 1,
    userId: "test-user-id",
    minSalary: 80000,
    maxSalary: 150000,
    jobType: JobType.FullTime,
    needSponsorship: true,
    needRelocation: false,
    createdAt: "2024-01-01T00:00:00Z",
    updatedAt: "2024-01-02T00:00:00Z",
  };

  beforeEach(() => {
    jest.clearAllMocks();

    // Mock useAuth hook
    (authHook.useAuth as jest.Mock).mockReturnValue({
      user: mockUser,
      isLoading: false,
    });
  });

  describe("Loading States", () => {
    it("should show loading spinner when auth is loading", () => {
      (authHook.useAuth as jest.Mock).mockReturnValue({
        user: null,
        isLoading: true,
      });

      const { container } = render(<PreferencesPage />);
      // Check for the loading spinner by looking for the animate-spin class
      const spinner = container.querySelector('.animate-spin');
      expect(spinner).toBeInTheDocument();
    });

    it("should fetch preferences on mount when user is authenticated", async () => {
      (preferencesActions.getUserPreferencesAction as jest.Mock).mockResolvedValue({
        data: mockPreferences,
      });

      render(<PreferencesPage />);

      await waitFor(() => {
        expect(preferencesActions.getUserPreferencesAction).toHaveBeenCalledTimes(1);
      });
    });
  });

  describe("Display Mode - Existing Preferences", () => {
    beforeEach(() => {
      (preferencesActions.getUserPreferencesAction as jest.Mock).mockResolvedValue({
        data: mockPreferences,
      });
    });

    it("should render preferences display view with correct data", async () => {
      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByText("Current Preferences")).toBeInTheDocument();
      });

      expect(screen.getByText(/\$80,000 - \$150,000/)).toBeInTheDocument();
      expect(screen.getByText("Full-time")).toBeInTheDocument();
      expect(screen.getByText("Needs Visa Sponsorship")).toBeInTheDocument();
    });

    it("should show edit and delete buttons", async () => {
      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByRole("button", { name: /edit/i })).toBeInTheDocument();
      });

      expect(screen.getByRole("button", { name: /delete/i })).toBeInTheDocument();
    });

    it("should switch to edit mode when edit button is clicked", async () => {
      const user = userEvent.setup();
      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByRole("button", { name: /edit/i })).toBeInTheDocument();
      });

      const editButton = screen.getByRole("button", { name: /edit/i });
      await user.click(editButton);

      expect(screen.getByText("Edit Preferences")).toBeInTheDocument();
      expect(screen.getByLabelText(/minimum salary/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/maximum salary/i)).toBeInTheDocument();
    });
  });

  describe("Create Mode - No Existing Preferences", () => {
    beforeEach(() => {
      (preferencesActions.getUserPreferencesAction as jest.Mock).mockResolvedValue({
        error: "Not Found",
      });
    });

    it("should show create form when no preferences exist", async () => {
      render(<PreferencesPage />);

      await waitFor(
        () => {
          expect(screen.getByLabelText(/minimum salary/i)).toBeInTheDocument();
        },
        { timeout: 3000 }
      );

      // Verify we're in create mode by checking form elements
      expect(screen.getByLabelText(/minimum salary/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/maximum salary/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/preferred job type/i)).toBeInTheDocument();
      expect(screen.getByRole("button", { name: /create preferences/i })).toBeInTheDocument();
    });

    it("should create preferences with valid data", async () => {
      const user = userEvent.setup();
      (preferencesActions.createPreferencesAction as jest.Mock).mockResolvedValue({
        data: mockPreferences,
      });

      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByLabelText(/minimum salary/i)).toBeInTheDocument();
      });

      const minSalaryInput = screen.getByLabelText(/minimum salary/i);
      const maxSalaryInput = screen.getByLabelText(/maximum salary/i);
      const submitButton = screen.getByRole("button", { name: /create preferences/i });

      await user.clear(minSalaryInput);
      await user.type(minSalaryInput, "80000");
      await user.clear(maxSalaryInput);
      await user.type(maxSalaryInput, "150000");
      await user.click(submitButton);

      await waitFor(() => {
        expect(preferencesActions.createPreferencesAction).toHaveBeenCalledWith(
          expect.objectContaining({
            minSalary: 80000,
            maxSalary: 150000,
            userId: mockUser.id,
          })
        );
      });
    });
  });

  describe("Form Validation", () => {
    beforeEach(() => {
      // Ensure no preferences exist for validation tests
      (preferencesActions.getUserPreferencesAction as jest.Mock).mockResolvedValue({
        error: "Not Found",
      });
    });

    it("should show error when max salary is less than min salary", async () => {
      const user = userEvent.setup();

      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByLabelText(/minimum salary/i)).toBeInTheDocument();
      });

      const minSalaryInput = screen.getByLabelText(/minimum salary/i);
      const maxSalaryInput = screen.getByLabelText(/maximum salary/i);
      const submitButton = screen.getByRole("button", { name: /create preferences/i });

      await user.clear(minSalaryInput);
      await user.type(minSalaryInput, "150000");
      await user.clear(maxSalaryInput);
      await user.type(maxSalaryInput, "80000");
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText(/maximum salary must be greater than minimum salary/i)).toBeInTheDocument();
      });
    });
  });

  describe("Update Operations", () => {
    beforeEach(() => {
      (preferencesActions.getUserPreferencesAction as jest.Mock).mockResolvedValue({
        data: mockPreferences,
      });
    });

    it("should update preferences with modified values", async () => {
      const user = userEvent.setup();
      const updatedPreferences = { ...mockPreferences, minSalary: 90000 };
      (preferencesActions.updatePreferencesAction as jest.Mock).mockResolvedValue({
        data: updatedPreferences,
      });

      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByRole("button", { name: /edit/i })).toBeInTheDocument();
      });

      const editButton = screen.getByRole("button", { name: /edit/i });
      await user.click(editButton);

      const minSalaryInput = screen.getByLabelText(/minimum salary/i);
      const saveButton = screen.getByRole("button", { name: /save changes/i });

      await user.clear(minSalaryInput);
      await user.type(minSalaryInput, "90000");
      await user.click(saveButton);

      await waitFor(() => {
        expect(preferencesActions.updatePreferencesAction).toHaveBeenCalledWith(
          "1",
          expect.objectContaining({
            minSalary: 90000,
          })
        );
      });
    });

    it("should cancel edit and restore original values", async () => {
      const user = userEvent.setup();
      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByRole("button", { name: /edit/i })).toBeInTheDocument();
      });

      const editButton = screen.getByRole("button", { name: /edit/i });
      await user.click(editButton);

      const minSalaryInput = screen.getByLabelText(/minimum salary/i) as HTMLInputElement;
      const cancelButton = screen.getByRole("button", { name: /cancel/i });

      await user.clear(minSalaryInput);
      await user.type(minSalaryInput, "99999");
      await user.click(cancelButton);

      await waitFor(() => {
        expect(screen.getByText("Current Preferences")).toBeInTheDocument();
      });
    });
  });

  describe("Delete Operations", () => {
    beforeEach(() => {
      (preferencesActions.getUserPreferencesAction as jest.Mock).mockResolvedValue({
        data: mockPreferences,
      });
    });

    it("should show delete confirmation dialog", async () => {
      const user = userEvent.setup();
      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByRole("button", { name: /delete/i })).toBeInTheDocument();
      });

      const deleteButton = screen.getByRole("button", { name: /delete/i });
      await user.click(deleteButton);

      await waitFor(() => {
        expect(screen.getByText("Delete Preferences")).toBeInTheDocument();
      });

      expect(screen.getByText(/are you sure you want to delete your preferences/i)).toBeInTheDocument();
    });

    it("should delete preferences and switch to create mode", async () => {
      const user = userEvent.setup();
      (preferencesActions.deletePreferencesAction as jest.Mock).mockResolvedValue({
        data: undefined,
      });

      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByRole("button", { name: /delete/i })).toBeInTheDocument();
      });

      const deleteButton = screen.getByRole("button", { name: /delete/i });
      await user.click(deleteButton);

      await waitFor(() => {
        expect(screen.getByRole("dialog")).toBeInTheDocument();
      });

      // Get all delete buttons and select the one inside the dialog (second one)
      const confirmButtons = screen.getAllByRole("button", { name: /delete/i });
      const confirmButton = confirmButtons[confirmButtons.length - 1]; // Last one is in dialog
      await user.click(confirmButton);

      await waitFor(() => {
        expect(preferencesActions.deletePreferencesAction).toHaveBeenCalledWith("1");
      });
    });
  });

  describe("Checkbox Interactions", () => {
    beforeEach(() => {
      (preferencesActions.getUserPreferencesAction as jest.Mock).mockResolvedValue({
        error: "Not Found",
      });
    });

    it("should toggle sponsorship checkbox", async () => {
      const user = userEvent.setup();
      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByLabelText(/i need visa sponsorship/i)).toBeInTheDocument();
      });

      const sponsorshipCheckbox = screen.getByLabelText(/i need visa sponsorship/i);
      await user.click(sponsorshipCheckbox);

      expect(sponsorshipCheckbox).toBeChecked();
    });

    it("should toggle relocation checkbox", async () => {
      const user = userEvent.setup();
      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByLabelText(/i need relocation assistance/i)).toBeInTheDocument();
      });

      const relocationCheckbox = screen.getByLabelText(/i need relocation assistance/i);
      await user.click(relocationCheckbox);

      expect(relocationCheckbox).toBeChecked();
    });
  });

  describe("Error Handling", () => {
    it("should display error message when fetch fails", async () => {
      (preferencesActions.getUserPreferencesAction as jest.Mock).mockResolvedValue({
        error: "Failed to fetch preferences: Server error",
      });

      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByText(/failed to fetch preferences: server error/i)).toBeInTheDocument();
      });
    });

    it("should display error message when create fails", async () => {
      const user = userEvent.setup();
      (preferencesActions.getUserPreferencesAction as jest.Mock).mockResolvedValue({
        error: "Not Found",
      });
      (preferencesActions.createPreferencesAction as jest.Mock).mockResolvedValue({
        error: "Failed to create preferences: Invalid data",
      });

      render(<PreferencesPage />);

      await waitFor(() => {
        expect(screen.getByLabelText(/minimum salary/i)).toBeInTheDocument();
      });

      const submitButton = screen.getByRole("button", { name: /create preferences/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText(/failed to create preferences: invalid data/i)).toBeInTheDocument();
      });
    });
  });
});
