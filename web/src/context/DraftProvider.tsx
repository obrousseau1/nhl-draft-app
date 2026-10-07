import { DraftContext } from "./DraftContext.tsx"
import { type ReactNode} from "react";
import useDraft from "../hooks/useDraft.ts";

type Props = {
  children: ReactNode
}

export const DraftProvider = (props : Props) => {
  const draft = useDraft();
  const { children } = props;

  return (
    <DraftContext.Provider value={draft}>
      {children}
    </DraftContext.Provider>
  )
}
