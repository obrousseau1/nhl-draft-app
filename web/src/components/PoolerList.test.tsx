import { describe, expect, it } from 'vitest'
import { fireEvent, screen } from '@testing-library/react'
import { PoolerList } from './PoolerList.tsx'
import { alex, bob, carl, renderWithDraft } from '../test-utils.ts'

const up = { name: '↑' }
const down = { name: '↓' }

describe('PoolerList', () => {
  it('when there are no poolers, should say so and offer no shuffle', () => {
    renderWithDraft(<PoolerList poolers={[]} />)

    expect(screen.getByText('No poolers yet')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Shuffle Order' })).not.toBeInTheDocument()
  })

  it('when there are poolers, should show them in draft order', () => {
    renderWithDraft(<PoolerList poolers={[alex, bob, carl]} />)

    const names = screen.getAllByRole('textbox').map(input => (input as HTMLInputElement).value)
    expect(names).toEqual(['Alex', 'Bob', 'Carl'])
  })

  it('when showing the arrows, should disable up on the first row and down on the last', () => {
    renderWithDraft(<PoolerList poolers={[alex, bob, carl]} />)

    const ups = screen.getAllByRole('button', up)
    const downs = screen.getAllByRole('button', down)
    expect(ups[0]).toBeDisabled()
    expect(ups[1]).toBeEnabled()
    expect(downs[1]).toBeEnabled()
    expect(downs[2]).toBeDisabled()
  })

  it('when there is a single pooler, should disable both arrows', () => {
    renderWithDraft(<PoolerList poolers={[alex]} />)

    expect(screen.getByRole('button', up)).toBeDisabled()
    expect(screen.getByRole('button', down)).toBeDisabled()
  })

  it('when down is clicked on the first row, should send the order with the first two swapped', () => {
    const { draft } = renderWithDraft(<PoolerList poolers={[alex, bob, carl]} />)

    fireEvent.click(screen.getAllByRole('button', down)[0])

    expect(draft.reorder).toHaveBeenCalledExactlyOnceWith([bob.id, alex.id, carl.id])
  })

  it('when up is clicked on the last row, should send the order with the last two swapped', () => {
    const { draft } = renderWithDraft(<PoolerList poolers={[alex, bob, carl]} />)

    fireEvent.click(screen.getAllByRole('button', up)[2])

    expect(draft.reorder).toHaveBeenCalledExactlyOnceWith([alex.id, carl.id, bob.id])
  })

  it('when Shuffle is clicked, should ask the server to shuffle', () => {
    const { draft } = renderWithDraft(<PoolerList poolers={[alex, bob]} />)

    fireEvent.click(screen.getByRole('button', { name: 'Shuffle Order' }))

    expect(draft.shuffle).toHaveBeenCalledOnce()
  })
})
