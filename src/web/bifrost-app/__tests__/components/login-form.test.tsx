import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import "@testing-library/jest-dom";
import userEvent from "@testing-library/user-event";
import { LoginForm } from "@/components/login-form";
import { useRouter } from "next/navigation";
import { authService } from "@/lib/api/auth";

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
    const mockLogin = authService.login as jest.MockedFunction<typeof authService.login>;
    mockLogin.mockRejectedValue(new Error("Invalid login credentials"));

    render(<LoginForm />);

    const user = userEvent.setup();
    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole("button", { name: /^login$/i });

    await user.type(emailInput, "test@example.com");
    await user.type(passwordInput, "wrongpassword");
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/invalid email or password/i)).toBeInTheDocument();
    });
  });

  it("should complete login successfully without errors", async () => {
    const mockToken = "mock-jwt-token-12345";
    const mockRefreshToken = "mock-refresh-token-67890";
    const mockUserId = "mock-user-id-uuid";
    const mockEmail = "test@example.com";

    const mockLogin = authService.login as jest.MockedFunction<typeof authService.login>;
    mockLogin.mockResolvedValue({
      accessToken: mockToken,
      refreshToken: mockRefreshToken,
      expiresIn: 3600,
      tokenType: "Bearer",
      user: {
        id: mockUserId,
        email: mockEmail,
        createdAt: new Date().toISOString(),
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

    // Verify that authService.login was called successfully
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        email: mockEmail,
        password: "password123",
      });
    });

    // Verify no error is displayed
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it("should redirect to dashboard after successful login", async () => {
    const mockLogin = authService.login as jest.MockedFunction<typeof authService.login>;
    mockLogin.mockResolvedValue({
      accessToken: "mock-token",
      refreshToken: "mock-refresh",
      expiresIn: 3600,
      tokenType: "Bearer",
      user: {
        id: "mock-user-id",
        email: "test@example.com",
        createdAt: new Date().toISOString(),
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
    const mockLogin = authService.login as jest.MockedFunction<typeof authService.login>;
    mockLogin.mockImplementation(() =>
      new Promise((resolve) => setTimeout(() => resolve({
        accessToken: "token",
        refreshToken: "refresh",
        expiresIn: 3600,
        tokenType: "Bearer",
        user: { id: "id", email: "test@example.com", createdAt: new Date().toISOString() }
      }), 100))
    );

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

  it("should call authService.login with correct credentials", async () => {
    const mockLogin = authService.login as jest.MockedFunction<typeof authService.login>;
    mockLogin.mockResolvedValue({
      accessToken: "token",
      refreshToken: "refresh",
      expiresIn: 3600,
      tokenType: "Bearer",
      user: { id: "id", email: "test@example.com", createdAt: new Date().toISOString() },
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
      expect(mockLogin).toHaveBeenCalledWith({
        email: testEmail,
        password: testPassword,
      });
    });
  });
});
