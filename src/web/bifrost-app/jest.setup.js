import '@testing-library/jest-dom'

// Suppress act() warnings in tests - these are expected in async components
// and don't affect test correctness with React 19
const originalError = console.error;
beforeAll(() => {
  console.error = (...args) => {
    const message = args[0];
    if (
      typeof message === 'string' &&
      (message.includes('An update to') && message.includes('was not wrapped in act'))
    ) {
      return;
    }
    originalError.call(console, ...args);
  };
});

afterAll(() => {
  console.error = originalError;
});
