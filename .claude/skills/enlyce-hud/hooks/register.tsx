import { atom, read, update } from 'claude-code'
import type { EngineInterface, Register } from 'claude-code'

import type { Snapshot } from '../types'
import { parseDotnetTest, parseGitStatus } from './parse'

const REPO = 'D:/Proyectos/Enlyce'
const PANE = 'enlyce'
const POLL_MS = 20000

const snap = atom({ plugin: 'enlyce-hud', key: 'snap' } as const, null)
const tests = atom({ plugin: 'enlyce-hud', key: 'tests' } as const, null)

const mark = (ok: boolean | null) => (ok === null ? '?' : ok ? 'ok' : 'caida')

async function probe($: EngineInterface): Promise<Snapshot> {
  const [git, db, api] = await Promise.allSettled([
    $.process.run(['git', 'status', '-sb'], { cwd: REPO, timeoutMs: 8000 }),
    $.process.run(['docker', 'inspect', '-f', '{{.State.Running}}', 'enlyce-db'], { timeoutMs: 8000 }),
    $.http.fetch('http://localhost:5000/'),
  ])
  const g = git.status === 'fulfilled' && git.value.exitCode === 0
    ? parseGitStatus(git.value.stdout)
    : { branch: '?', dirty: 0, ahead: 0 }
  return {
    ...g,
    isDbUp: db.status === 'fulfilled' ? db.value.stdout.trim() === 'true' : null,
    isApiUp: api.status === 'fulfilled',
    at: await $.clock.now(),
  }
}

async function refresh($: EngineInterface) {
  const prev = await read($, snap)
  const next = await probe($)
  await update($, snap, () => next)

  // avisos solo en transiciones, no en cada sondeo
  if (prev && prev.isDbUp === true && next.isDbUp === false) $.ui.toast('Enlyce: enlyce-db se cayó')
  if (prev && prev.isDbUp === false && next.isDbUp === true) $.ui.toast('Enlyce: enlyce-db volvió')
  if (prev && prev.isApiUp && !next.isApiUp) $.ui.toast('Enlyce: la API (5000) dejó de responder')

  const t = await read($, tests)
  const testTxt = t ? (t.failed > 0 ? ` · tests ${t.failed} rojos` : ` · tests ${t.passed} ok`) : ''
  $.ui.status(
    `${next.branch}${next.dirty ? ` +${next.dirty}` : ''}${next.ahead ? ` ↑${next.ahead}` : ''}` +
      ` · DB ${mark(next.isDbUp)} · API ${mark(next.isApiUp)}${testTxt}`,
  )
}

export const register: Register = on => {
  on('session.start', async ($, e, next) => {
    await $.command.register({ name: 'enlyce', description: 'Panel de estado de Enlyce' })
    void refresh($)
    $.clock.every(POLL_MS, () => refresh($))
    return next(e)
  })

  on('command.run', { command: 'enlyce' }, async $ => {
    await refresh($)
    await $.ui.open({ id: PANE, title: 'Enlyce' })
    return { text: 'Panel Enlyce abierto.' }
  })

  on('tool.call', { tool: 'Bash' }, async ($, e, next) => {
    const ran = await next(e)
    if (ran.deny !== undefined || !ran.result) return ran
    const out = `${ran.result.stdout ?? ''}\n${ran.result.stderr ?? ''}`

    if (/dotnet\s+test/.test(e.command)) {
      const run = parseDotnetTest(out, await $.clock.now())
      if (run) {
        await update($, tests, () => run)
        $.ui.toast(run.failed > 0 ? `Tests: ${run.failed} fallan, ${run.passed} pasan` : `Tests: ${run.passed} pasan`)
      }
    }
    if (/git\s+push/.test(e.command) && /rejected|error:|fatal:/.test(out)) {
      $.ui.toast('git push falló')
    }
    if (/git\s+(commit|push|checkout|switch|pull)/.test(e.command)) void refresh($)
    return ran
  }).catch(($, e, next) => (next.called ? next(e) : next(e)))

  on('ui.render', { component: 'Pane', requestId: PANE }, async ($, e) => {
    const { Box, Text } = $.ui.resolve(e)
    const s = await read($, snap)
    const t = await read($, tests)
    if (!s) return <Text dimColor>Sondeando...</Text>

    return (
      <Box flexDirection="column">
        <Text>Rama: <Text bold>{s.branch}</Text>  cambios: {s.dirty}  por subir: {s.ahead}</Text>
        <Text color={s.isDbUp ? 'green' : 'red'}>enlyce-db (5436): {mark(s.isDbUp)}</Text>
        <Text color={s.isApiUp ? 'green' : 'yellow'}>API (5000): {mark(s.isApiUp)}</Text>
        {t ? (
          <Text color={t.failed > 0 ? 'red' : 'green'}>
            Tests: {t.passed} pasan, {t.failed} fallan, {t.skipped} omitidos
          </Text>
        ) : (
          <Text dimColor>Tests: sin correr en esta sesión</Text>
        )}
        <Text dimColor>Se actualiza cada {POLL_MS / 1000} s</Text>
      </Box>
    )
  })
}
