import type {Pooler} from "../types.ts";
import { useRef, useState} from "react";
import type {PositionInList} from "./PoolerList.tsx";
import useDraftContext from "../hooks/useDraftContext.ts";

type Props = {
  pooler: Pooler,
  positionInList: PositionInList,
  onMove: (delta: -1 | 1) => void;
}

export const PoolerRow = (props: Props) => {
  const {pooler, positionInList, onMove} = props;
  const draftActions = useDraftContext()
  const [poolerName, setPoolerName] = useState(pooler.name);
  const inputRef = useRef<HTMLInputElement>(null);

  const save = () => {
    const name = poolerName.trim();
    if (name === pooler.name) return;
    void draftActions.renamePooler(pooler.id, name);
  }

  const cancel = () => {
    setPoolerName(pooler.name);
  }

  return (
    <li>
      <input
        value={poolerName}
        ref={inputRef}
        onChange={e => setPoolerName(e.target.value) }
        onBlur={save}
        onKeyDown={e => {
          if (e.key === "Enter") inputRef.current?.blur();
          if (e.key === "Escape") cancel();
        }}/>
      <button onClick={() => draftActions.removePooler(pooler.id)}>Remove</button>
      <button disabled={positionInList === 'first' || positionInList === 'alone'} onClick={() => onMove(-1)}>↑</button>
      <button disabled={positionInList === 'last' || positionInList === 'alone'} onClick={() => onMove(1)}>↓</button>
    </li>
  )
}
