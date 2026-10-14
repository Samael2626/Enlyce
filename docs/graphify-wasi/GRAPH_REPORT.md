# Graph Report - graphify-wasi  (2026-10-14)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 11 nodes · 12 edges · 2 communities
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `2334e747`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Community 0
- Community 1

## God Nodes (most connected - your core abstractions)
1. `Wasi CRM` - 7 edges
2. `Clientes` - 4 edges
3. `Inmuebles` - 3 edges
4. `Cruce Clientes - Inmuebles` - 2 edges
5. `Cruce Inmuebles - Clientes` - 2 edges
6. `Agenda` - 1 edges
7. `Escritorio (Dashboard)` - 1 edges
8. `Red Inmobiliaria` - 1 edges
9. `Reportes` - 1 edges
10. `Usuarios` - 1 edges

## Surprising Connections (you probably didn't know these)
- `Clientes` --references--> `Wasi CRM`  [EXTRACTED]
  ingenieria-inversa-wasi.md → ingenieria-inversa-wasi.md  _Bridges community 0 → community 1_

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Wasi Matching Engine** — ingenieria_inversa_wasi_cruce_clientes_inmuebles, ingenieria_inversa_wasi_cruce_inmuebles_clientes, ingenieria_inversa_wasi_inmuebles, ingenieria_inversa_wasi_clientes [EXTRACTED 0.90]

## Communities (2 total, 0 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.33
Nodes (6): Agenda, Escritorio (Dashboard), Red Inmobiliaria, Reportes, Usuarios, Wasi CRM

### Community 1 - "Community 1"
Cohesion: 0.50
Nodes (5): Clientes, Cruce Clientes - Inmuebles, Cruce Inmuebles - Clientes, Inmuebles, Embudo de Negocios (Pipeline)

## Knowledge Gaps
- **6 isolated node(s):** `Agenda`, `Escritorio (Dashboard)`, `Red Inmobiliaria`, `Reportes`, `Usuarios` (+1 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 6 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Wasi CRM` connect `Community 0` to `Community 1`?**
  _High betweenness centrality (0.793) - this node is a cross-community bridge._
- **Why does `Clientes` connect `Community 1` to `Community 0`?**
  _High betweenness centrality (0.344) - this node is a cross-community bridge._
- **Why does `Inmuebles` connect `Community 1` to `Community 0`?**
  _High betweenness centrality (0.144) - this node is a cross-community bridge._
- **What connects `Agenda`, `Escritorio (Dashboard)`, `Red Inmobiliaria` to the rest of the system?**
  _6 weakly-connected nodes found - possible documentation gaps or missing edges._