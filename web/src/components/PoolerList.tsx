import type {Pooler} from "../types.ts";
import {PoolerRow} from "./PoolerRow.tsx";
import useDraftContext from "../hooks/useDraftContext.ts";

type Props = {
  poolers: Pooler[]
}

export type PositionInList = "alone" | "first" | "last" | "middle"

const positionOf = (i: number, numberOfElements: number): PositionInList => {
  return numberOfElements === 1 ? "alone" : i === 0 ? "first": i === numberOfElements - 1 ? "last" : "middle";
}

export const PoolerList = (props: Props) => {
  const { poolers } = props;
  const isEmpty = poolers.length === 0;
  const draftActions = useDraftContext()

  const move = (index: number, delta: -1 | 1) => {
    const ids = poolers.map(po => po.id);
    const target = index + delta;
    [ids[index], ids[target]] = [ids[target], ids[index]];
    void draftActions.reorder(ids);
  }

  return(
    isEmpty ?
      <p>No poolers yet</p> :
      <div>
        <button onClick={() => void draftActions.shuffle()}>Shuffle Order</button>
        <ol>
          {poolers.map((p, i) =>
            <PoolerRow key={p.id}
                       pooler={p}
                       positionInList={positionOf(i, poolers.length)}
                       onMove={delta => move(i, delta)}/>)}
        </ol>
      </div>

  )
}
