import React from "react";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import "@testing-library/jest-dom";
import { SignUpForm } from "@/components/sign-up-form";
import { authService } from "@/lib/api/auth";
import { useRouter } from "next/navigation";

// Mock the auth service
jest.mock("@/lib/api/auth", () => ({
  authService: {
    register: jest.fn(),
  },
}));

// Mock Next.js router
jest.mock("next/navigation", () => ({
  useRouter: jest.fn(),
}));

describe("SignUpForm", () => {
  const mockPush = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({
      push: mockPush,
    });
  });

  it("renders sign up form with all fields", () => {
    render(<SignUpForm />);

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/repeat password/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /sign up/i })).toBeInTheDocument();
  });

  it("shows error when passwords do not match", async () => {
    render(<SignUpForm />);

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/^password$/i);
    const repeatPasswordInput = screen.getByLabelText(/repeat password/i);
    const submitButton = screen.getByRole("button", { name: /sign up/i });

    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(passwordInput, { target: { value: "password123" } });
    fireEvent.change(repeatPasswordInput, { target: { value: "password456" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument();
    });

    expect(authService.register).not.toHaveBeenCalled();
  });

  it("successfully registers a user with valid credentials", async () => {
    const mockAuthResponse = {
      accessToken: "mock-token",
      refreshToken: "mock-refresh-token",
      expiresIn: 3600,
      tokenType: "Bearer",
      user: {
        id: "user-123",
        email: "test@example.com",
        createdAt: new Date().toISOString(),
      },
    };

    (authService.register as jest.Mock).mockResolvedValue(mockAuthResponse);

    render(<SignUpForm />);

    const emailInput = screen.getByLabelText(/email/i);
    const fullNameInput = screen.getByLabelText(/full name/i);
    const passwordInput = screen.getByLabelText(/^password$/i);
    const repeatPasswordInput = screen.getByLabelText(/repeat password/i);
    const submitButton = screen.getByRole("button", { name: /sign up/i });

    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(fullNameInput, { target: { value: "Test User" } });
    fireEvent.change(passwordInput, { target: { value: "password123" } });
    fireEvent.change(repeatPasswordInput, { target: { value: "password123" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(authService.register).toHaveBeenCalledWith({
        email: "test@example.com",
        password: "password123",
        fullName: "Test User",
      });
      expect(mockPush).toHaveBeenCalledWith("/dashboard");
    });
  });

  it("shows error message when registration fails", async () => {
    const errorMessage = "Email already exists";
    (authService.register as jest.Mock).mockRejectedValue(new Error(errorMessage));

    render(<SignUpForm />);

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/^password$/i);
    const repeatPasswordInput = screen.getByLabelText(/repeat password/i);
    const submitButton = screen.getByRole("button", { name: /sign up/i });

    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(passwordInput, { target: { value: "password123" } });
    fireEvent.change(repeatPasswordInput, { target: { value: "password123" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });

    expect(mockPush).not.toHaveBeenCalled();
  });

  it("disables submit button while loading", async () => {
    (authService.register as jest.Mock).mockImplementation(
      () => new Promise((resolve) => setTimeout(resolve, 100))
    );

    render(<SignUpForm />);

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/^password$/i);
    const repeatPasswordInput = screen.getByLabelText(/repeat password/i);
    const submitButton = screen.getByRole("button", { name: /sign up/i });

    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(passwordInput, { target: { value: "password123" } });
    fireEvent.change(repeatPasswordInput, { target: { value: "password123" } });
    fireEvent.click(submitButton);

    expect(submitButton).toBeDisabled();
    expect(screen.getByText(/creating an account/i)).toBeInTheDocument();

    await waitFor(() => {
      expect(submitButton).not.toBeDisabled();
    });
  });

  it("registers with null fullName when field is empty", async () => {
    const mockAuthResponse = {
      accessToken: "mock-token",
      refreshToken: "mock-refresh-token",
      expiresIn: 3600,
      tokenType: "Bearer",
      user: {
        id: "user-123",
        email: "test@example.com",
        createdAt: new Date().toISOString(),
      },
    };

    (authService.register as jest.Mock).mockResolvedValue(mockAuthResponse);

    render(<SignUpForm />);

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/^password$/i);
    const repeatPasswordInput = screen.getByLabelText(/repeat password/i);
    const submitButton = screen.getByRole("button", { name: /sign up/i });

    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(passwordInput, { target: { value: "password123" } });
    fireEvent.change(repeatPasswordInput, { target: { value: "password123" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(authService.register).toHaveBeenCalledWith({
        email: "test@example.com",
        password: "password123",
        fullName: null,
      });
    });
  });
});
