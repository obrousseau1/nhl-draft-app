import {type SubmitEventHandler, useState} from "react";
import type {DraftView, Settings} from "../types.ts";

type Props = {
  draft: DraftView,
  onSave: (settings: Settings) => void
}

export const SettingsForm = ({draft, onSave}: Props) => {
  const [currentSettings, setCurrentSettings] = useState<Settings>(draft.settings);

  function update<K extends keyof Settings>(key: K, value: Settings[K]) {
    setCurrentSettings({ ...currentSettings, [key]: value })
  }

  const isDraftStarted = draft.started;
  const handleSubmit : SubmitEventHandler<HTMLFormElement>  = (event) => {
    event?.preventDefault();
    onSave(currentSettings)
  }
  const hasNaN = Object.values(currentSettings).some(Number.isNaN)

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Rounds<input type="number" disabled={isDraftStarted} value={currentSettings.rounds} onChange={(e) => update('rounds', e.target.valueAsNumber) }/></label>
        <label>Cap<input type="number" disabled={isDraftStarted} value={currentSettings.cap} step="any" onChange={(e) => update('cap', e.target.valueAsNumber) }/></label>
        <label>Min. Defensemen<input type="number" disabled={isDraftStarted} value={currentSettings.defenseMin} onChange={(e) => update('defenseMin', e.target.valueAsNumber) }/></label>
        <label>Min. Goalies<input type="number" disabled={isDraftStarted} value={currentSettings.goalies} onChange={(e) => update('goalies', e.target.valueAsNumber) }/></label>
        <label>Min. Teams<input type="number" disabled={isDraftStarted} value={currentSettings.teams} onChange={(e) => update('teams', e.target.valueAsNumber) }/></label>
        <label>Max. Poolers<input type="number" disabled={isDraftStarted} value={currentSettings.maxPoolers} onChange={(e) => update('maxPoolers', e.target.valueAsNumber) }/></label>
        <button type="submit" disabled={isDraftStarted || hasNaN}>Save Settings</button>
      </div>
    </form>
  )
}
