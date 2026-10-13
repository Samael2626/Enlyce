import type { TestRun } from '../types'

// suma los totales de cada proyecto en la salida de dotnet test
export function parseDotnetTest(out: string, at: number): TestRun | null {
  const re = /(Passed|Failed)!\s*-\s*Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+)/g
  let run: TestRun | null = null
  for (const m of out.matchAll(re)) {
    run ??= { passed: 0, failed: 0, skipped: 0, at }
    run.failed += Number(m[2])
    run.passed += Number(m[3])
    run.skipped += Number(m[4])
  }
  return run
}

// cuenta archivos cambiados y commits por delante a partir de git status -sb
export function parseGitStatus(out: string): { branch: string; dirty: number; ahead: number } {
  const lines = out.split('\n').filter(l => l.length > 0)
  const head = lines[0] ?? ''
  const branch = /^## ([^.\s]+)/.exec(head)?.[1] ?? '?'
  const ahead = Number(/ahead (\d+)/.exec(head)?.[1] ?? 0)
  return { branch, dirty: Math.max(0, lines.length - 1), ahead }
}
