import { describe, expect, it } from 'vitest'
import { draftReducer } from './useDraft.ts'
import { sampleDraft } from '../test-utils.ts'

const duplicate = 'Alex is already in the pooler list.'
const apiDown = 'Failed to fetch'

describe('draftReducer', () => {
  it('when loaded while loading, should become ready with the draft', () => {
    const next = draftReducer({ status: 'loading' }, { type: 'loaded', draft: sampleDraft })

    expect(next).toEqual({ status: 'ready', draft: sampleDraft })
  })

  it('when failed while loading, should become a full-page error', () => {
    const next = draftReducer({ status: 'loading' }, { type: 'failed', message: apiDown })

    expect(next).toEqual({ status: 'error', message: apiDown })
  })

  it('when failed while ready, should keep the draft and add the error', () => {
    const next = draftReducer({ status: 'ready', draft: sampleDraft }, { type: 'failed', message: duplicate })

    expect(next).toEqual({ status: 'ready', draft: sampleDraft, error: duplicate })
  })

  it('when loaded after an error banner, should clear the error', () => {
    const next = draftReducer(
      { status: 'ready', draft: sampleDraft, error: duplicate },
      { type: 'loaded', draft: sampleDraft },
    )

    expect(next).toEqual({ status: 'ready', draft: sampleDraft })
  })

  it('when failed again on a full-page error, should show the new message', () => {
    const next = draftReducer({ status: 'error', message: 'old' }, { type: 'failed', message: apiDown })

    expect(next).toEqual({ status: 'error', message: apiDown })
  })

  it('when failed while ready, should not change the previous state object', () => {
    const before = { status: 'ready', draft: sampleDraft } as const

    draftReducer(before, { type: 'failed', message: duplicate })

    expect(before).toEqual({ status: 'ready', draft: sampleDraft })
  })
})
