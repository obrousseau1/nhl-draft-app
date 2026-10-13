import { describe, expect, it } from 'vitest'
import { fireEvent, screen, waitFor } from '@testing-library/react'
import SetupPage from './SetupPage.tsx'
import { renderWithDraft, sampleDraft } from '../test-utils.ts'

const resetMessage = 'Clears all picks. Poolers and settings are kept.'
const newMessage = 'Starts a new draft file. Poolers and settings are kept, picks are empty.'

describe('SetupPage', () => {
  it('when opened, should show no confirmation dialog', () => {
    renderWithDraft(<SetupPage draft={sampleDraft} />)

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('when Reset is confirmed, should reset and close the dialog', async () => {
    const { draft } = renderWithDraft(<SetupPage draft={sampleDraft} />)

    fireEvent.click(screen.getByRole('button', { name: 'Reset Current Draft' }))
    expect(screen.getByText(resetMessage)).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Confirm' }))

    expect(draft.reset).toHaveBeenCalledOnce()
    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument())
  })

  it('when Confirm is double-clicked, should start one new draft', () => {
    const { draft } = renderWithDraft(<SetupPage draft={sampleDraft} />)

    fireEvent.click(screen.getByRole('button', { name: 'Start New Draft' }))
    const confirm = screen.getByRole('button', { name: 'Confirm' })
    fireEvent.click(confirm)
    fireEvent.click(confirm)

    expect(draft.startNew).toHaveBeenCalledOnce()
  })

  it('when New draft is cancelled, should close without starting a new draft', () => {
    const { draft } = renderWithDraft(<SetupPage draft={sampleDraft} />)

    fireEvent.click(screen.getByRole('button', { name: 'Start New Draft' }))
    expect(screen.getByText(newMessage)).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(draft.startNew).not.toHaveBeenCalled()
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('when a pooler is added, should call the draft action', () => {
    const { draft } = renderWithDraft(<SetupPage draft={sampleDraft} />)
    const form = screen.getByRole('button', { name: 'Add pooler' }).closest('form')!

    fireEvent.change(form.querySelector('input')!, { target: { value: 'Dave' } })
    fireEvent.submit(form)

    expect(draft.addPooler).toHaveBeenCalledExactlyOnceWith('Dave')
  })
})
