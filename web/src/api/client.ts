import type {DraftView, Problem, Settings} from "../types.ts";

export async function getDraft() : Promise<DraftView> {
  return await request<DraftView>('/api/draft');
}

export async function addPooler(name:string) : Promise<DraftView> {
  return await request<DraftView>('/api/poolers', {
    method: 'POST',
    body: JSON.stringify({name: name})
  });
}

export async function renamePooler(id:string, newName:string) : Promise<DraftView> {
  return await request<DraftView>(`/api/poolers/${id}`, {
    method: 'PUT',
    body: JSON.stringify({name: newName})
  })
}

export async function removePooler(id:string) : Promise<DraftView> {
  return await request<DraftView>(`/api/poolers/${id}`, {
    method: 'DELETE',
  })
}

export async function reorder(ids:string[]) : Promise<DraftView> {
  return await request<DraftView>('/api/poolers/order', {
    method: 'PUT',
    body: JSON.stringify(ids)
  })
}

export async function shuffle() : Promise<DraftView> {
  return await request<DraftView>('/api/poolers/shuffle', {
    method: 'POST',
  })
}

export async function updateSettings(newSettings:Settings) : Promise<DraftView> {
  return await request<DraftView>('/api/draft/settings', {
    method: 'PUT',
    body: JSON.stringify(newSettings)
  })
}

export async function reset() : Promise<DraftView> {
  return await request<DraftView>('/api/draft/reset', {
    method: 'POST',
  })
}

export async function startNew() : Promise<DraftView> {
  return await request<DraftView>('/api/draft/new', {
    method: 'POST',
  })
}

async function request<T>(url:string, options?:RequestInit) {
  const init = options?.body ? { ...options, headers: { 'Content-Type': 'application/json' } } : options;
  const response = await fetch(url, init);
  if(!response.ok) {
    let error;

    try {
      error = await response.json() as Problem;
    }
    catch {
      throw new Error(response.statusText);
    }

    throw new Error(error.detail || error.title || response.statusText);
  }

  return await response.json() as T;
}
