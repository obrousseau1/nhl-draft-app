import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ErrorBoundary } from './ErrorBoundary.tsx'

const fallback = 'Something went wrong. Your picks are saved on the server. Reload to continue.'

function Bomb(): never {
  throw new Error('boom')
}

describe('ErrorBoundary', () => {
  it('when nothing breaks, should show its children', () => {
    render(<ErrorBoundary><p>Board</p></ErrorBoundary>)

    expect(screen.getByText('Board')).toBeInTheDocument()
    expect(screen.queryByText(fallback)).not.toBeInTheDocument()
  })

  it('when a child throws while drawing, should show the fallback and log the error', () => {
    const log = vi.spyOn(console, 'error').mockImplementation(() => {})   // keeps the test output clean

    render(<ErrorBoundary><Bomb /></ErrorBoundary>)

    expect(screen.getByText(fallback)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Reload' })).toBeInTheDocument()
    expect(log).toHaveBeenCalledWith('Crash while drawing:', expect.any(Error), expect.any(String))
  })
})
