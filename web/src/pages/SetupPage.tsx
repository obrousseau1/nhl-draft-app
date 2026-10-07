import {AddPoolerForm} from "../components/AddPoolerForm.tsx";
import {PoolerList} from "../components/PoolerList.tsx";
import { SettingsForm } from "../components/SettingsForm.tsx";
import { useState, type JSX } from "react";
import {Modal} from "../components/Modal.tsx";
import useDraftContext from "../hooks/useDraftContext.ts";
import type {DraftView} from "../types.ts";

type Props = {
  draft: DraftView
}

export default function SetupPage({draft}: Props): JSX.Element {
  const draftContext = useDraftContext();
  const [confirming, setConfirming] = useState<'reset' | 'new'| null>(null);

  return (
    <div>
      <AddPoolerForm onAdd={draftContext.addPooler}/>
      <PoolerList poolers={draft.poolers} />
      <SettingsForm draft={draft} onSave={draftContext.updateSettings}/>
      <button onClick={() => setConfirming('new')}>Start New Draft</button>
      <button onClick={() => setConfirming('reset')}>Reset Current Draft</button>
      {confirming === 'new' && <Modal onConfirm={async () => { await draftContext.startNew(); setConfirming(null) }} onClose={() => setConfirming(null)}>Starts a new draft file. Poolers and settings are kept, picks are empty.</Modal>}
      {confirming === 'reset' && <Modal onConfirm={async () => { await draftContext.reset(); setConfirming(null) }} onClose={() => setConfirming(null)}>Clears all picks. Poolers and settings are kept.</Modal>}
    </div>
  )
}
