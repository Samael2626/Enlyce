import { readFileSync, writeFileSync } from "node:fs";

const [source, destination] = process.argv.slice(2);
if (!source || !destination) {
  throw new Error("Uso: node prepare-atomic.mjs <sql-generado> <sql-atomico>");
}

const sql = readFileSync(source, "utf8");
const begins = [...sql.matchAll(/^START TRANSACTION;\r?$/gm)].length;
const commits = [...sql.matchAll(/^COMMIT;\r?$/gm)].length;
const migrations = [...sql.matchAll(/INSERT INTO "__EFMigrationsHistory"/g)].length;
if (!begins || begins !== commits || begins !== migrations) {
  throw new Error(`Cadena EF inesperada: ${begins} inicios, ${commits} commits, ${migrations} registros.`);
}

const atomic = sql
  .replace(/^START TRANSACTION;\r?\n\r?\n/gm, "")
  .replace(/^COMMIT;\r?\n/gm, "");
if (/^(START TRANSACTION;|COMMIT;|ROLLBACK;)\r?$/m.test(atomic)) {
  throw new Error("El SQL conserva limites de transaccion internos.");
}

writeFileSync(destination, atomic);
process.stdout.write(`${migrations} migraciones preparadas en una transaccion externa.\n`);
