import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    // Built straight into the API's wwwroot: `dotnet run` then serves the app offline, and publish includes it.
    outDir: '../api/NhlDraftApp.Api/wwwroot',
    emptyOutDir: true,
  },
  server: {
    // Calls to /api/... go to the C# API, so fetch('/api/draft') just works in `pnpm dev`.
    // Our own Origin is rewritten too: the API refuses writes whose Origin differs from its host. Foreign ones stay refused.
    proxy: {
      '/api': {
        target: 'http://localhost:5190',
        changeOrigin: true,
        configure: (proxy) => proxy.on('proxyReq', (req, incoming) => {
          if (req.getHeader('origin') === `http://${incoming.headers.host}`) req.setHeader('origin', 'http://localhost:5190')
        }),
      },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
  },
})
