import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen } from '@testing-library/react'
import { Modal } from './Modal.tsx'

const message = 'Clears all picks.'

function renderModal() {
  const onConfirm = vi.fn()
  const onClose = vi.fn()
  const result = render(<Modal onConfirm={onConfirm} onClose={onClose}>{message}</Modal>)
  return { ...result, onConfirm, onClose }
}

describe('Modal', () => {
  it('when opened, should show its children and focus Confirm', () => {
    renderModal()

    expect(screen.getByText(message)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Confirm' })).toHaveFocus()
  })

  it('when opened, should draw outside its parent through the portal', () => {
    const { container } = renderModal()

    expect(container).toBeEmptyDOMElement()                  // nothing where it was declared…
    expect(screen.getByRole('dialog').closest('body')).toBe(document.body)   // …it's under <body>
  })

  it('when Escape is pressed, should close once', () => {
    const { onClose } = renderModal()

    fireEvent.keyDown(window, { key: 'Escape' })

    expect(onClose).toHaveBeenCalledOnce()
  })

  it('when the backdrop is clicked, should close', () => {
    const { onClose } = renderModal()

    fireEvent.click(screen.getByRole('dialog').parentElement!)

    expect(onClose).toHaveBeenCalledOnce()
  })

  it('when the dialog itself is clicked, should stay open', () => {
    const { onClose } = renderModal()

    fireEvent.click(screen.getByRole('dialog'))

    expect(onClose).not.toHaveBeenCalled()
  })

  it('when Cancel is clicked, should close without confirming', () => {
    const { onClose, onConfirm } = renderModal()

    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onClose).toHaveBeenCalledOnce()
    expect(onConfirm).not.toHaveBeenCalled()
  })

  it('when Confirm is clicked, should confirm', () => {
    const { onConfirm } = renderModal()

    fireEvent.click(screen.getByRole('button', { name: 'Confirm' }))

    expect(onConfirm).toHaveBeenCalledOnce()
  })

  it('when unmounted, should stop listening to Escape', () => {
    const { onClose, unmount } = renderModal()

    unmount()
    fireEvent.keyDown(window, { key: 'Escape' })

    expect(onClose).not.toHaveBeenCalled()
  })
})
