import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen } from '@testing-library/react'
import { SettingsForm } from './SettingsForm.tsx'
import { sampleDraft } from '../test-utils.ts'

const save = { name: 'Save Settings' }

describe('SettingsForm', () => {
  it('when opened, should show the current settings', () => {
    render(<SettingsForm draft={sampleDraft} onSave={vi.fn()} />)

    expect(screen.getByLabelText('Rounds')).toHaveValue(sampleDraft.settings.rounds)
    expect(screen.getByLabelText('Cap')).toHaveValue(sampleDraft.settings.cap)
  })

  it('when a field is changed and saved, should send all settings with that change', () => {
    const onSave = vi.fn()
    render(<SettingsForm draft={sampleDraft} onSave={onSave} />)

    fireEvent.change(screen.getByLabelText('Rounds'), { target: { value: '16' } })
    fireEvent.change(screen.getByLabelText('Cap'), { target: { value: '104.5' } })
    fireEvent.click(screen.getByRole('button', save))

    expect(onSave).toHaveBeenCalledExactlyOnceWith({ ...sampleDraft.settings, rounds: 16, cap: 104.5 })
  })

  it('when a field is empty, should disable Save', () => {
    render(<SettingsForm draft={sampleDraft} onSave={vi.fn()} />)

    fireEvent.change(screen.getByLabelText('Cap'), { target: { value: '' } })

    expect(screen.getByRole('button', save)).toBeDisabled()
  })

  it('when the draft has started, should disable every field and Save', () => {
    render(<SettingsForm draft={{ ...sampleDraft, started: true }} onSave={vi.fn()} />)

    expect(screen.getByLabelText('Rounds')).toBeDisabled()
    expect(screen.getByLabelText('Max. Poolers')).toBeDisabled()
    expect(screen.getByRole('button', save)).toBeDisabled()
  })
})
