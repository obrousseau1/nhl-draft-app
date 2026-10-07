import type {DraftView, Settings} from "../types.ts";
import {useEffect, useReducer } from "react";
import * as api from "../api/client.ts";

export type DraftStatus =
{status:'loading'} |
{status:'error'; message: string} |
{status:'ready'; draft: DraftView, error?: string}

type DraftAction =
  {type:'loaded'; draft: DraftView } |
  {type:'failed'; message: string }

export type DraftActions = {
  addPooler: (name: string) => Promise<void>;
  renamePooler: (id: string, name: string) => Promise<void>;
  removePooler: (id: string) => Promise<void>;
  reorder: (ids: string[]) => Promise<void>;
  shuffle: () => Promise<void>;
  updateSettings: (settings: Settings) => Promise<void>;
  reset: () => Promise<void>;
  startNew: () => Promise<void>;
};

export type UseDraft = DraftActions & { draftState: DraftStatus};

const draftReducer = (draftState: DraftStatus, action: DraftAction) : DraftStatus => {
  switch(action.type) {
    case "loaded":
      return { status: 'ready', draft: action.draft }
    case "failed":
      return draftState.status === 'ready' ? {...draftState, error: action.message} : {status:'error', message:action.message};
    default: { const missing: never = action; return missing }
  }
}

export default function useDraft(): UseDraft {
  const [draftState, dispatch] = useReducer(draftReducer, {status: 'loading'});
  const callApi = (call: Promise<DraftView>) => {

    return call.then(draft => dispatch({ type: 'loaded', draft }))
      .catch((e: Error) => dispatch({ type: 'failed', message: e.message }))
  }

  useEffect(() => {
    void callApi(api.getDraft())
  },[])

  return {
    draftState,
    addPooler:  (name) => callApi(api.addPooler(name)),
    renamePooler:  (id, name) => callApi(api.renamePooler(id, name)),
    removePooler:  (id) => callApi(api.removePooler(id)),
    reorder:  ids => callApi(api.reorder(ids)),
    reset: () => callApi(api.reset()),
    shuffle: () => callApi(api.shuffle()),
    startNew: () => callApi(api.startNew()),
    updateSettings: (settings) => callApi(api.updateSettings(settings)),
  };
}
