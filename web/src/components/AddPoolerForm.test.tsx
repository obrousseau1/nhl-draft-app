import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { AddPoolerForm } from './AddPoolerForm.tsx'

const addButton = { name: 'Add pooler' }

describe('AddPoolerForm', () => {
  it('when a name is added, should send it trimmed and clear the input', async () => {
    const onAdd = vi.fn().mockResolvedValue('ok')
    render(<AddPoolerForm onAdd={onAdd} />)
    const input = screen.getByRole('textbox')

    fireEvent.change(input, { target: { value: '  Alex ' } })
    fireEvent.click(screen.getByRole('button', addButton))

    expect(onAdd).toHaveBeenCalledExactlyOnceWith('Alex')
    await waitFor(() => expect(input).toHaveValue(''))
  })

  it('when the add is refused, should keep the typed name', async () => {
    const onAdd = vi.fn().mockResolvedValue('refused')
    render(<AddPoolerForm onAdd={onAdd} />)
    const input = screen.getByRole('textbox')

    fireEvent.change(input, { target: { value: 'Alex' } })
    fireEvent.click(screen.getByRole('button', addButton))

    await waitFor(() => expect(onAdd).toHaveBeenCalled())
    expect(input).toHaveValue('Alex')
  })

  it('when the form is submitted with Enter, should add the name', () => {
    const onAdd = vi.fn().mockResolvedValue('ok')
    render(<AddPoolerForm onAdd={onAdd} />)
    const input = screen.getByRole('textbox')

    fireEvent.change(input, { target: { value: 'Bob' } })
    fireEvent.submit(input)   // pressing Enter in a form input submits the form

    expect(onAdd).toHaveBeenCalledExactlyOnceWith('Bob')
  })
})
