import { createContext } from 'react'
import type {UseDraft} from "../hooks/useDraft.ts";

export const DraftContext = createContext<UseDraft | undefined>(undefined)
