import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    // The logic under test is framework-free, so a plain Node environment is
    // enough — no jsdom/browser needed, which keeps the tests fast.
    environment: 'node',
    include: ['src/**/*.spec.ts'],
  },
});
