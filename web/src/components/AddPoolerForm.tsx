import {type SubmitEventHandler, useState} from "react";

type Props = { onAdd: (name: string) => void }

export const AddPoolerForm = ({ onAdd } : Props) => {
  const [poolerName, setPoolerName] = useState<string>('');
  const handleSubmit : SubmitEventHandler<HTMLFormElement>  = (event) => {
    event?.preventDefault();
    onAdd(poolerName.trim())
    setPoolerName('')
  }

  return(
    <form onSubmit={handleSubmit}>
      <input value={poolerName} onChange={(e) => setPoolerName(e.target.value)}/>
      <button type="submit">Add pooler</button>
    </form>
  )
}
