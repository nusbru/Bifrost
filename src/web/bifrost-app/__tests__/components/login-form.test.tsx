import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import "@testing-library/jest-dom";
import userEvent from "@testing-library/user-event";
import { LoginForm } from "@/components/login-form";
import { useRouter } from "next/navigation";
import { authService as _authService } from "@/lib/api/auth";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  useRouter: jest.fn(),
}));

// Mock auth service
jest.mock("@/lib/api/auth", () => ({
  authService: {
    login: jest.fn(),
  },
}));

describe("LoginForm Component", () => {
  const mockPush = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({
      push: mockPush,
    });

    // Mock localStorage
    Object.defineProperty(window, "localStorage", {
      value: {
        getItem: jest.fn(),
        setItem: jest.fn(),
        removeItem: jest.fn(),
        clear: jest.fn(),
      },
      writable: true,
    });
  });

  it("should render login form with all required fields", () => {
    render(<LoginForm />);

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /^login$/i })).toBeInTheDocument();
    expect(screen.getByText(/enter your email below to login/i)).toBeInTheDocument();
  });

  it("should have links to sign up and forgot password pages", () => {
    render(<LoginForm />);

    expect(screen.getByText(/don't have an account/i)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /sign up/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /forgot your password/i })).toBeInTheDocument();
  });

  it("should display error message on failed login", async () => {
    const { createClient } = require("@/lib/supabase/client");
    const errorMessage = "Invalid login credentials";
    const mockError = new Error(errorMessage);

    const mockSignIn = jest.fn().mockResolvedValue({
      data: { session: null, user: null },
      error: mockError,
    });

    createClient.mockReturnValue({
      auth: {
        signInWithPassword: mockSignIn,
      },
    });

    render(<LoginForm />);

    const user = userEvent.setup();
    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole("button", { name: /^login$/i });

    await user.type(emailInput, "test@example.com");
    await user.type(passwordInput, "wrongpassword");
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });
  });

  it("should complete login successfully without errors", async () => {
    const mockToken = "mock-jwt-token-12345";
    const mockRefreshToken = "mock-refresh-token-67890";
    const mockUserId = "mock-user-id-uuid";
    const mockEmail = "test@example.com";

    const { createClient } = require("@/lib/supabase/client");

    const mockSignIn = jest.fn().mockResolvedValue({
      data: {
        session: {
          access_token: mockToken,
          refresh_token: mockRefreshToken,
        },
        user: {
          id: mockUserId,
          email: mockEmail,
        },
      },
      error: null,
    });

    createClient.mockReturnValue({
      auth: {
        signInWithPassword: mockSignIn,
      },
    });

    render(<LoginForm />);

    const user = userEvent.setup();
    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole("button", { name: /^login$/i });

    await user.type(emailInput, mockEmail);
    await user.type(passwordInput, "password123");
    await user.click(submitButton);

    // Verify that signInWithPassword was called successfully
    await waitFor(() => {
      expect(mockSignIn).toHaveBeenCalledWith({
        email: mockEmail,
        password: "password123",
      });
    });

    // Verify no error is displayed
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();

    // Session is automatically stored in cookies by Supabase (not localStorage)
    // Verify localStorage is NOT used for session storage
    expect(localStorage.setItem).not.toHaveBeenCalled();
  });

  it("should redirect to dashboard after successful login", async () => {
    const { createClient } = require("@/lib/supabase/client");

    const mockSignIn = jest.fn().mockResolvedValue({
      data: {
        session: {
          access_token: "mock-token",
          refresh_token: "mock-refresh",
        },
        user: {
          id: "mock-user-id",
          email: "test@example.com",
        },
      },
      error: null,
    });

    createClient.mockReturnValue({
      auth: {
        signInWithPassword: mockSignIn,
      },
    });

    render(<LoginForm />);

    const user = userEvent.setup();
    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole("button", { name: /^login$/i });

    await user.type(emailInput, "test@example.com");
    await user.type(passwordInput, "password123");
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/dashboard");
    });
  });

  it("should disable submit button during login", async () => {
    const { createClient } = require("@/lib/supabase/client");

    const mockSignIn = jest.fn().mockImplementation(() =>
      new Promise((resolve) => setTimeout(() => resolve({
        data: {
          session: { access_token: "token", refresh_token: "refresh" },
          user: { id: "id", email: "test@example.com" }
        },
        error: null
      }), 100))
    );

    createClient.mockReturnValue({
      auth: {
        signInWithPassword: mockSignIn,
      },
    });

    render(<LoginForm />);

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole("button", { name: /^login$/i });

    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(passwordInput, { target: { value: "password123" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /logging in/i })).toBeDisabled();
    });
  });

  it("should call Supabase signInWithPassword with correct credentials", async () => {
    const { createClient } = require("@/lib/supabase/client");
    const mockSignIn = jest.fn().mockResolvedValue({
      data: {
        session: { access_token: "token", refresh_token: "refresh" },
        user: { id: "id", email: "test@example.com" },
      },
      error: null,
    });

    createClient.mockReturnValue({
      auth: {
        signInWithPassword: mockSignIn,
      },
    });

    render(<LoginForm />);

    const user = userEvent.setup();
    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole("button", { name: /^login$/i });

    const testEmail = "test@example.com";
    const testPassword = "password123";

    await user.type(emailInput, testEmail);
    await user.type(passwordInput, testPassword);
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockSignIn).toHaveBeenCalledWith({
        email: testEmail,
        password: testPassword,
      });
    });
  });
});
