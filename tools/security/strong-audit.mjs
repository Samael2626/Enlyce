import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";

const target = new URL(process.env.ENLYCE_SECURITY_TARGET ?? "http://localhost:5029");
const outputPath = resolve(
  process.env.ENLYCE_SECURITY_OUTPUT ?? "security-reports/strong-audit-results.json",
);
const seedPassword = process.env.ENLYCE_SECURITY_SEED_PASSWORD;

if (!['localhost', '127.0.0.1', '::1'].includes(target.hostname)) {
  throw new Error("El harness solo puede atacar un host local.");
}

if (target.port !== "5029" || process.env.ENLYCE_SECURITY_CONFIRM !== "ISOLATED") {
  throw new Error("Falta confirmar el entorno aislado en el puerto 5029.");
}

if (!seedPassword) {
  throw new Error("Falta ENLYCE_SECURITY_SEED_PASSWORD.");
}

async function request(path, options = {}) {
  const response = await fetch(new URL(path, target), {
    signal: AbortSignal.timeout(15_000),
    ...options,
    headers: {
      "content-type": "application/json",
      ...options.headers,
    },
  });
  const text = await response.text();
  let body = null;
  try {
    body = text ? JSON.parse(text) : null;
  } catch {
    body = text.slice(0, 500);
  }
  return { status: response.status, headers: response.headers, body };
}

function json(method, body, headers = {}) {
  return { method, headers, body: JSON.stringify(body) };
}

function statusHistogram(responses) {
  return responses.reduce((result, response) => {
    result[response.status] = (result[response.status] ?? 0) + 1;
    return result;
  }, {});
}

function authToken(response) {
  const cookie = response.headers.get("set-cookie") ?? "";
  return cookie.match(/(?:^|;\s*)_enlyce_auth=([^;]+)/)?.[1] ?? null;
}

function unsignedJwt(token) {
  const [, payload] = token.split(".");
  const header = Buffer.from(JSON.stringify({ alg: "none", typ: "JWT" })).toString("base64url");
  return `${header}.${payload}.`;
}

const startedAt = new Date().toISOString();
const runId = Date.now();
const report = { startedAt, target: target.origin, checks: {} };

report.checks.seed = await request("/api/auth/seed", { method: "POST" });

const login = await request(
  "/api/auth/login",
  json("POST", { correo: "admin@enlyce.com", password: seedPassword }),
);
const token = authToken(login);
if (login.status !== 200 || !token) {
  throw new Error(`No fue posible autenticar el administrador sintético: ${login.status}`);
}
report.checks.validLogin = { status: login.status, returnedTokenInBody: Boolean(login.body?.token) };

const privatePaths = [
  "/api/auth/me",
  "/api/pipeline",
  "/api/alertas/",
  "/api/publicaciones/",
  "/api/billing/pricing",
];
report.checks.privateRoutes = Object.fromEntries(
  await Promise.all(
    privatePaths.map(async (path) => [path, (await request(path)).status]),
  ),
);

const tamperedToken = `${token.slice(0, -1)}${token.endsWith("a") ? "b" : "a"}`;
const authChecks = await Promise.all([
  request("/api/auth/me", { headers: { authorization: `Bearer ${tamperedToken}` } }),
  request("/api/auth/me", { headers: { authorization: `Bearer ${unsignedJwt(token)}` } }),
  request("/api/auth/me", { headers: { authorization: `Bearer ${token}` } }),
]);
report.checks.jwtIntegrity = {
  tampered: authChecks[0].status,
  unsigned: authChecks[1].status,
  valid: authChecks[2].status,
};

const adminHeaders = { authorization: `Bearer ${token}` };
const advisorPassword = `Advisor-${runId}!`;
const advisorAEmail = `advisor-a-${runId}@example.test`;
const advisorBEmail = `advisor-b-${runId}@example.test`;
const advisorARegistration = await request(
  "/api/auth/register",
  json("POST", {
    nombre: "Asesor A",
    correo: advisorAEmail,
    password: advisorPassword,
    rol: "Asesor",
  }, adminHeaders),
);
const advisorBRegistration = await request(
  "/api/auth/register",
  json("POST", {
    nombre: "Asesor B",
    correo: advisorBEmail,
    password: advisorPassword,
    rol: "Asesor",
  }, adminHeaders),
);
const advisorALogin = await request(
  "/api/auth/login",
  json("POST", { correo: advisorAEmail, password: advisorPassword }),
);
const advisorBLogin = await request(
  "/api/auth/login",
  json("POST", { correo: advisorBEmail, password: advisorPassword }),
);
const advisorAId = advisorARegistration.body?.id;
const advisorBId = advisorBRegistration.body?.id;
const advisorAHeaders = { authorization: `Bearer ${authToken(advisorALogin)}` };
const advisorBHeaders = { authorization: `Bearer ${authToken(advisorBLogin)}` };

const scopedLead = await request(
  "/api/leads",
  json("POST", {
    nombre: "Oportunidad asignada",
    email: `scoped-${runId}@example.test`,
    fuente: "security-rbac",
    autorizacionDatos: true,
    tipoOperacion: "Venta",
    canal: "auditoria_local",
  }),
);
const scopedPipeline = await request("/api/pipeline", { headers: adminHeaders });
const scopedLeadId = scopedPipeline.body?.leads?.find(
  (lead) => lead.email === `scoped-${runId}@example.test`,
)?.id;
const assignment = await request(
  `/api/pipeline/${scopedLeadId}/asignar`,
  json("PUT", { asesorId: advisorAId }, adminHeaders),
);
const horizontalChecks = await Promise.all([
  request(`/api/leads/${scopedLeadId}`, { headers: advisorAHeaders }),
  request(`/api/leads/${scopedLeadId}`, { headers: advisorBHeaders }),
  request(
    `/api/pipeline/${scopedLeadId}/mover-etapa`,
    json("PUT", { nuevaEtapa: "Contactado" }, advisorBHeaders),
  ),
  request(
    `/api/pipeline/${scopedLeadId}/asignar`,
    json("PUT", { asesorId: advisorBId }, advisorBHeaders),
  ),
  request(
    "/api/auth/register",
    json("POST", {
      nombre: "Escalada",
      correo: `escalation-${runId}@example.test`,
      password: advisorPassword,
      rol: "Administrador",
    }, advisorBHeaders),
  ),
  request(
    "/api/interacciones",
    json("POST", {
      leadId: scopedLeadId,
      asesorId: advisorAId,
      tipo: "Llamada",
      resumen: "Intento de suplantación",
    }, advisorBHeaders),
  ),
]);
report.checks.horizontalAuthorization = {
  advisorRegistrations: [advisorARegistration.status, advisorBRegistration.status],
  advisorLogins: [advisorALogin.status, advisorBLogin.status],
  leadCreation: scopedLead.status,
  adminAssignment: assignment.status,
  ownerAdvisorRead: horizontalChecks[0].status,
  foreignAdvisorRead: horizontalChecks[1].status,
  foreignAdvisorMove: horizontalChecks[2].status,
  advisorAssignmentAttempt: horizontalChecks[3].status,
  advisorAdminCreationAttempt: horizontalChecks[4].status,
  advisorImpersonationAttempt: horizontalChecks[5].status,
};

const concurrentEmail = `race-${runId}@example.test`;
const leadBody = {
  nombre: "Prueba de concurrencia",
  email: concurrentEmail,
  telefono: "+573001234567",
  fuente: "security-harness",
  autorizacionDatos: true,
  tipoOperacion: "Venta",
  canal: "auditoria_local",
};
const concurrentCreates = await Promise.all(
  Array.from({ length: 30 }, () => request("/api/leads", json("POST", leadBody))),
);
const racePipeline = await request("/api/pipeline", { headers: adminHeaders });
const persistedRaceRecords = racePipeline.body?.leads?.filter(
  (lead) => lead.email === concurrentEmail,
) ?? [];
report.checks.concurrentLeadCreation = {
  requests: concurrentCreates.length,
  statuses: statusHistogram(concurrentCreates),
  persistedRecords: persistedRaceRecords.length,
  deduplicated: persistedRaceRecords.length === 1,
};

const victimEmail = `victim-${runId}@example.test`;
const initial = await request(
  "/api/leads",
  json("POST", { ...leadBody, nombre: "Víctima sintética", email: victimEmail, ownerService: "Sell" }),
);
const recontact = await request(
  "/api/leads",
  json("POST", { ...leadBody, nombre: "Atacante", email: victimEmail, ownerService: "Sell" }),
);
const victimPipeline = await request("/api/pipeline", { headers: adminHeaders });
const victimId = victimPipeline.body?.leads?.find((lead) => lead.email === victimEmail)?.id;
const overwrite = await request(
      `/api/leads/${victimId}/owner-details`,
      json("PUT", {
        email: victimEmail,
        propertyType: "Apartment",
        city: "Medellín",
        neighborhood: "ALTERADO SIN AUTENTICACIÓN",
        expectedPrice: 1,
        message: "Prueba ofensiva aislada",
        preferredContactChannel: "Email",
      }),
    );
report.checks.publicObjectAuthorization = {
  initialStatus: initial.status,
  recontactStatus: recontact.status,
  sameIdDisclosed: Boolean(recontact.body?.id),
  disclosedFields: recontact.body && typeof recontact.body === "object"
    ? Object.keys(recontact.body).sort()
    : [],
  unauthenticatedOverwriteStatus: overwrite.status,
};

const oversized = await request(
  "/api/leads",
  json("POST", {
    ...leadBody,
    email: `oversized-${runId}@example.test`,
    fuente: "A".repeat(1_000_000),
  }),
);
report.checks.oversizedPublicPayload = { status: oversized.status };

const invalidLogins = await Promise.all(
  Array.from({ length: 60 }, (_, index) =>
    request(
      "/api/auth/login",
      json("POST", {
        correo: "admin@enlyce.com",
        password: `wrong-${runId}-${index}`,
      }),
    ),
  ),
);
report.checks.loginBurst = {
  requests: invalidLogins.length,
  statuses: statusHistogram(invalidLogins),
  rateLimited: invalidLogins.some((response) => response.status === 429),
};

const evilPreflight = await fetch(new URL("/api/leads", target), {
  method: "OPTIONS",
  headers: {
    origin: "https://evil.example",
    "access-control-request-method": "POST",
    "access-control-request-headers": "content-type",
  },
});
report.checks.cors = {
  status: evilPreflight.status,
  allowsEvilOrigin: evilPreflight.headers.get("access-control-allow-origin") === "https://evil.example",
  allowsCredentials: evilPreflight.headers.get("access-control-allow-credentials"),
};

const methodOverride = await request("/api/auth/me", {
  method: "POST",
  headers: { "x-http-method-override": "GET" },
});
report.checks.methodOverride = { status: methodOverride.status };

report.finishedAt = new Date().toISOString();
mkdirSync(dirname(outputPath), { recursive: true });
writeFileSync(outputPath, `${JSON.stringify(report, null, 2)}\n`, "utf8");

const summary = {
  loginBurst: report.checks.loginBurst,
  privateRoutes: report.checks.privateRoutes,
  jwtIntegrity: report.checks.jwtIntegrity,
  horizontalAuthorization: report.checks.horizontalAuthorization,
  concurrentLeadCreation: report.checks.concurrentLeadCreation,
  publicObjectAuthorization: report.checks.publicObjectAuthorization,
  oversizedPublicPayload: report.checks.oversizedPublicPayload,
  cors: report.checks.cors,
  methodOverride: report.checks.methodOverride,
  outputPath,
};
console.log(JSON.stringify(summary, null, 2));
