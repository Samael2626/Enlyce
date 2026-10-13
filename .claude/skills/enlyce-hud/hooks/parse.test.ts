import { expect, test } from 'claude-code/testing'

import { parseDotnetTest, parseGitStatus } from './parse'

test('suma totales de varios proyectos de dotnet test', () => {
  const out = [
    'Passed!  - Failed:     0, Passed:   224, Skipped:     0, Total:   224',
    'Failed!  - Failed:     2, Passed:    51, Skipped:     1, Total:    54',
  ].join('\n')
  expect(parseDotnetTest(out, 1)).toEqual({ passed: 275, failed: 2, skipped: 1, at: 1 })
})

test('sin resumen de dotnet test devuelve null', () => {
  expect(parseDotnetTest('Build succeeded.', 1)).toBe(null)
})

test('lee rama, cambios y commits por delante', () => {
  const out = '## master...origin/master [ahead 3]\n M a.cs\n?? b.md\n'
  expect(parseGitStatus(out)).toEqual({ branch: 'master', dirty: 2, ahead: 3 })
})
