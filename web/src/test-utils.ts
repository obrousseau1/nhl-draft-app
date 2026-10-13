// Shared test data and helpers. A .ts file (createElement instead of JSX) so the react-refresh lint rule doesn't apply.
import { createElement, type ReactElement } from 'react'
import { render } from '@testing-library/react'
import { vi } from 'vitest'
import { DraftContext } from './context/DraftContext.tsx'
import type { UseDraft } from './hooks/useDraft.ts'
import type { DraftView, Pooler } from './types.ts'

export const alex: Pooler = { id: 'id-alex', name: 'Alex' }
export const bob: Pooler = { id: 'id-bob', name: 'Bob' }
export const carl: Pooler = { id: 'id-carl', name: 'Carl' }

export const sampleDraft: DraftView = {
  settings: { rounds: 20, cap: 104, defenseMin: 2, goalies: 2, teams: 2, maxPoolers: 12 },
  poolers: [alex, bob, carl],
  picks: [],
  started: false,
}

const action = () => vi.fn().mockResolvedValue('ok')

// A fake useDraft() result: every action is a vi.fn(), so a test can check how it was called.
export function fakeDraft(draft: DraftView = sampleDraft): UseDraft {
  return {
    draftState: { status: 'ready', draft },
    addPooler: action(),
    renamePooler: action(),
    removePooler: action(),
    reorder: action(),
    shuffle: action(),
    updateSettings: action(),
    reset: action(),
    startNew: action(),
  }
}

// Draws a component that reads useDraftContext(), with a fake value instead of the real API.
export function renderWithDraft(ui: ReactElement, draft: UseDraft = fakeDraft()) {
  return { ...render(createElement(DraftContext.Provider, { value: draft }, ui)), draft }
}

// A fetch Response with a JSON body. Each fetch call needs its own: a body can be read only once.
export const jsonResponse = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status })
