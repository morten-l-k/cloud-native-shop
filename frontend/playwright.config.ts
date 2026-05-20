import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  reporter: 'html',

  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
  },

  webServer: {
    command: 'npm start -- --host 0.0.0.0 --port 4200',
    url: 'http://localhost:4200',
    reuseExistingServer: true,
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});