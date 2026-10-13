import { describe, expect, it, vi } from 'vitest'
import { fireEvent, screen, waitFor } from '@testing-library/react'
import { PoolerRow } from './PoolerRow.tsx'
import { bob, renderWithDraft } from '../test-utils.ts'

const newName = 'Bobby'

function renderRow() {
  const onMove = vi.fn()
  const result = renderWithDraft(<PoolerRow pooler={bob} positionInList="middle" onMove={onMove} />)
  return { ...result, onMove, input: screen.getByDisplayValue(bob.name) }
}

describe('PoolerRow', () => {
  it('when a new name is typed and the field is left, should rename', () => {
    const { draft, input } = renderRow()

    fireEvent.change(input, { target: { value: `  ${newName} ` } })
    fireEvent.blur(input)

    expect(draft.renamePooler).toHaveBeenCalledExactlyOnceWith(bob.id, newName)
  })

  it('when the rename is refused, should restore the saved name', async () => {
    const { draft, input } = renderRow()
    vi.mocked(draft.renamePooler).mockResolvedValue('refused')

    fireEvent.change(input, { target: { value: newName } })
    fireEvent.blur(input)

    await waitFor(() => expect(input).toHaveValue(bob.name))
  })

  it('when Enter is pressed, should leave the field and rename once', () => {
    const { draft, input } = renderRow()
    input.focus()

    fireEvent.change(input, { target: { value: newName } })
    fireEvent.keyDown(input, { key: 'Enter' })

    expect(input).not.toHaveFocus()
    expect(draft.renamePooler).toHaveBeenCalledExactlyOnceWith(bob.id, newName)
  })

  it('when Escape is pressed, should restore the name without renaming', () => {
    const { draft, input } = renderRow()
    input.focus()   // focused, so a blur() inside Escape would really fire onBlur

    fireEvent.change(input, { target: { value: newName } })
    fireEvent.keyDown(input, { key: 'Escape' })
    fireEvent.blur(input)

    expect(input).toHaveValue(bob.name)
    expect(draft.renamePooler).not.toHaveBeenCalled()
  })

  it('when the field is left unchanged, should not rename', () => {
    const { draft, input } = renderRow()

    fireEvent.blur(input)

    expect(draft.renamePooler).not.toHaveBeenCalled()
  })

  it('when Remove is clicked, should remove this pooler', () => {
    const { draft } = renderRow()

    fireEvent.click(screen.getByRole('button', { name: 'Remove' }))

    expect(draft.removePooler).toHaveBeenCalledExactlyOnceWith(bob.id)
  })

  it('when the arrows are clicked, should report the direction', () => {
    const { onMove } = renderRow()

    fireEvent.click(screen.getByRole('button', { name: '↑' }))
    fireEvent.click(screen.getByRole('button', { name: '↓' }))

    expect(onMove.mock.calls).toEqual([[-1], [1]])
  })
})
