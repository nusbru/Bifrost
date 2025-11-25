/**
 * Application Logger
 * Centralized logging with environment-aware behavior
 */

type LogLevel = "error" | "warn" | "info" | "debug";

interface LogEntry {
  level: LogLevel;
  message: string;
  timestamp: string;
  error?: unknown;
  context?: Record<string, unknown>;
}

/**
 * Format log entry for console output
 */
function formatLogEntry(entry: LogEntry): string {
  const { level, message, timestamp, error, context } = entry;
  let output = `[${timestamp}] ${level.toUpperCase()}: ${message}`;

  if (context) {
    output += `\nContext: ${JSON.stringify(context, null, 2)}`;
  }

  if (error) {
    output += `\nError: ${error instanceof Error ? error.stack : JSON.stringify(error)}`;
  }

  return output;
}

/**
 * Send log to external service (implement as needed)
 * Could be Sentry, LogRocket, Datadog, etc.
 */
function sendToExternalService(entry: LogEntry): void {
  // TODO: Implement external logging service integration
  // Example: Sentry.captureException(entry.error, { extra: entry.context });
}

/**
 * Base logging function
 */
function log(level: LogLevel, message: string, error?: unknown, context?: Record<string, unknown>): void {
  const entry: LogEntry = {
    level,
    message,
    timestamp: new Date().toISOString(),
    error,
    context,
  };

  // Always log to console in development
  if (process.env.NODE_ENV === "development") {
    const formattedMessage = formatLogEntry(entry);

    switch (level) {
      case "error":
        console.error(formattedMessage);
        break;
      case "warn":
        console.warn(formattedMessage);
        break;
      case "info":
        console.info(formattedMessage);
        break;
      case "debug":
        console.debug(formattedMessage);
        break;
    }
  }

  // Send errors to external service in production
  if (process.env.NODE_ENV === "production" && level === "error") {
    sendToExternalService(entry);
  }
}

/**
 * Application logger instance
 */
export const logger = {
  /**
   * Log error message
   * @param message - Error description
   * @param error - Error object or unknown error
   * @param context - Additional context for debugging
   */
  error: (message: string, error?: unknown, context?: Record<string, unknown>) => {
    log("error", message, error, context);
  },

  /**
   * Log warning message
   * @param message - Warning description
   * @param context - Additional context
   */
  warn: (message: string, context?: Record<string, unknown>) => {
    log("warn", message, undefined, context);
  },

  /**
   * Log info message
   * @param message - Information message
   * @param context - Additional context
   */
  info: (message: string, context?: Record<string, unknown>) => {
    log("info", message, undefined, context);
  },

  /**
   * Log debug message (development only)
   * @param message - Debug information
   * @param context - Additional context
   */
  debug: (message: string, context?: Record<string, unknown>) => {
    if (process.env.NODE_ENV === "development") {
      log("debug", message, undefined, context);
    }
  },
};
