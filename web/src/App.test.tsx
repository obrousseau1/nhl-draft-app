// The whole flow: real DraftProvider, useDraft and client.ts. Only fetch is fake.
import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen } from '@testing-library/react'
import App from './App.tsx'
import { DraftProvider } from './context/DraftProvider.tsx'
import { jsonResponse, sampleDraft } from './test-utils.ts'

const duplicate = 'Alex is already in the pooler list.'

function renderApp() {
  return render(<DraftProvider><App /></DraftProvider>)
}

function addPooler(name: string) {
  const form = screen.getByRole('button', { name: 'Add pooler' }).closest('form')!
  fireEvent.change(form.querySelector('input')!, { target: { value: name } })
  fireEvent.submit(form)
}

describe('App', () => {
  it('when the draft loads, should show Loading, then the poolers', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(jsonResponse(sampleDraft))

    renderApp()

    expect(screen.getByText('Loading...')).toBeInTheDocument()
    expect(await screen.findByDisplayValue('Alex')).toBeInTheDocument()
    expect(fetch).toHaveBeenCalledWith('/api/draft', undefined)
  })

  it('when an add is refused, should show the server message and keep the list', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(jsonResponse(sampleDraft))
      .mockResolvedValueOnce(jsonResponse({ status: 400, title: 'Bad Request', detail: duplicate }, 400))
    renderApp()
    await screen.findByDisplayValue('Alex')

    addPooler('alex')

    expect(await screen.findByRole('alert')).toHaveTextContent(duplicate)
    expect(screen.getByDisplayValue('Alex')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenLastCalledWith('/api/poolers', expect.objectContaining({
      method: 'POST',
      body: JSON.stringify({ name: 'alex' }),
    }))
  })

  it('when an add succeeds after a refusal, should show the new draft and clear the message', async () => {
    const withDave = { ...sampleDraft, poolers: [...sampleDraft.poolers, { id: 'id-dave', name: 'Dave' }] }
    vi.spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(jsonResponse(sampleDraft))
      .mockResolvedValueOnce(jsonResponse({ status: 400, title: 'Bad Request', detail: duplicate }, 400))
      .mockResolvedValueOnce(jsonResponse(withDave))
    renderApp()
    await screen.findByDisplayValue('Alex')

    addPooler('alex')
    await screen.findByRole('alert')
    addPooler('Dave')

    expect(await screen.findByDisplayValue('Dave')).toBeInTheDocument()
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('when the first load fails with no JSON body, should show a full-page error', async () => {
    vi.spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(new Response('down', { status: 503, statusText: 'Service Unavailable' }))

    renderApp()

    expect(await screen.findByText('Service Unavailable')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Setup' })).not.toBeInTheDocument()
  })

  it('when the API is unreachable, should show the network error', async () => {
    vi.spyOn(globalThis, 'fetch').mockRejectedValueOnce(new TypeError('Failed to fetch'))

    renderApp()

    expect(await screen.findByText('Failed to fetch')).toBeInTheDocument()
  })

  it('when Board is clicked, should switch screens', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(jsonResponse(sampleDraft))
    renderApp()
    await screen.findByDisplayValue('Alex')

    fireEvent.click(screen.getByRole('button', { name: 'Board' }))

    expect(screen.getByText('Board', { selector: 'p' })).toBeInTheDocument()
    expect(screen.queryByDisplayValue('Alex')).not.toBeInTheDocument()
  })
})
