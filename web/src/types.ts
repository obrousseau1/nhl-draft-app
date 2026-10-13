export type Pooler = { readonly id: string, name: string };
export type Pick = { readonly poolerId:string, round:number, entryId:string };
export type DraftView = {settings:Settings, poolers:Pooler[], picks:Pick[], started:boolean};
export type Problem = {status:number, title:string, detail?:string};
export type Settings = {
  rounds:number,
  cap:number,
  defenseMin:number,
  goalies:number,
  teams:number,
  maxPoolers:number
};
