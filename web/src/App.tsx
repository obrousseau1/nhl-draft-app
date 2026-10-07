import {useState} from "react";
import SetupPage from "./pages/SetupPage.tsx";
import useDraftContext from "./hooks/useDraftContext.ts";

type View = "setup" | "board" | "roster"

export default function App() {
  const [view, setView] = useState<View>("setup")
  const { draftState } = useDraftContext()

  if (draftState.status === 'loading') return <p>Loading...</p>
  if (draftState.status === 'error') return <p>{draftState.message}</p>

  const draft = draftState.draft;

  const page = () => {
    switch(view) {
      case "setup": return <SetupPage draft={draft} />
      case "board": return <p>Board</p>
      case "roster": return <p>Roster</p>
      default: { const missing: never = view; return missing }
    }
  }

  return (
    <div>
      {draftState.error && <p role="alert" className="alert">{draftState.error}</p>}
      <button onClick={()=>setView("setup")}>Setup</button>
      <button onClick={()=>setView("board")}>Board</button>
      <button onClick={()=>setView("roster")}>Roster</button>
      {page()}
    </div>


  )
}
