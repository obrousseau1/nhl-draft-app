// Adds DOM matchers like expect(el).toBeInTheDocument() to every test.
import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

// Vitest globals are off, so Testing Library can't clean up by itself: unmount after each test and undo fetch mocks.
afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
})
