import {useContext} from "react";
import {DraftContext} from "../context/DraftContext.tsx";

export default function useDraftContext() {
  const context = useContext(DraftContext);

  if(!context) {
    throw new Error("useDraftContext must be used within the context");
  }

  return context;
}
