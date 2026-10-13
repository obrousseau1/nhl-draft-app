import {type SubmitEventHandler, useState} from "react";
import type {ApiOutcome} from "../hooks/useDraft.ts";

type Props = { onAdd: (name: string) => Promise<ApiOutcome> }

export const AddPoolerForm = ({ onAdd } : Props) => {
  const [poolerName, setPoolerName] = useState<string>('');
  const handleSubmit : SubmitEventHandler<HTMLFormElement>  = async (event) => {
    event?.preventDefault();
    if (await onAdd(poolerName.trim()) === 'ok') setPoolerName('')
  }

  return(
    <form onSubmit={handleSubmit}>
      <input value={poolerName} onChange={(e) => setPoolerName(e.target.value)}/>
      <button type="submit">Add pooler</button>
    </form>
  )
}
