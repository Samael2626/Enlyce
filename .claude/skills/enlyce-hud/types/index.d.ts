export type TestRun = { passed: number; failed: number; skipped: number; at: number }
export type Snapshot = {
  branch: string
  dirty: number
  ahead: number
  isDbUp: boolean | null
  isApiUp: boolean | null
  at: number
}

declare module 'claude-code' {
  interface PluginState {
    'enlyce-hud': { snap: Snapshot | null; tests: TestRun | null }
  }
}
