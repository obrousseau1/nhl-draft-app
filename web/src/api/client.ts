import type {DraftView, Problem, Settings} from "../types.ts";

class ApiError extends Error {
  public status: number;

  constructor(status:number, detail:string) {
    super(detail);
    this.status = status;
  }
}

export async function getDraft() : Promise<DraftView> {
  return await request<DraftView>('/api/draft');
}

export async function addPooler(name:string) : Promise<DraftView> {
  return await request<DraftView>('/api/poolers', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({name: name})
  });
}

export async function renamePooler(id:string, newName:string) : Promise<DraftView> {
  return await request<DraftView>(`/api/poolers/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({name: newName})
  })
}

export async function removePooler(id:string) : Promise<DraftView> {
  return await request<DraftView>(`/api/poolers/${id}`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' }
  })
}

export async function reorder(ids:string[]) : Promise<DraftView> {
  return await request<DraftView>('/api/poolers/order', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(ids)
  })
}

export async function shuffle() : Promise<DraftView> {
  return await request<DraftView>('/api/poolers/shuffle', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' }
  })
}

export async function updateSettings(newSettings:Settings) : Promise<DraftView> {
  return await request<DraftView>('/api/draft/settings', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(newSettings)
  })
}

export async function reset() : Promise<DraftView> {
  return await request<DraftView>('/api/draft/reset', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' }
  })
}

export async function startNew() : Promise<DraftView> {
  return await request<DraftView>('/api/draft/new', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' }
  })
}

async function request<T>(url:string, options?:RequestInit) {
  const response = await fetch(url, options);
  if(!response.ok) {
    let error;

    try {
      error = await response.json() as Problem;
    }
    catch {
      throw new ApiError(response.status, response.statusText);
    }

    throw new ApiError(error.status, error.detail);
  }

  return await response.json() as T;
}
