"use client";

import { useId, useState } from "react";
import { monthlyPayment } from "@/lib/finance";
import { formatPrice } from "@/lib/format";

type Props = { operation: string; price: number; administration?: number };

export function CostCalculator({ operation, price, administration = 0 }: Props) {
  const id = useId();
  const [downPct, setDownPct] = useState(30);
  const [rate, setRate] = useState(12);
  const [years, setYears] = useState(20);

  if (operation === "Permuta") return null;

  if (operation === "Arriendo") {
    return (
      <section aria-labelledby={`${id}-t`} className="cost-calculator">
        <h2 id={`${id}-t`} className="text-2xl">Costo mensual</h2>
        <dl>
          <div><dt>Canon</dt><dd>{formatPrice(price)}</dd></div>
          <div><dt>Administración</dt><dd>{administration ? formatPrice(administration) : "No informada"}</dd></div>
          <div className="cost-total"><dt>Total al mes</dt><dd>{formatPrice(price + administration)}</dd></div>
        </dl>
        <p className="cost-note">No incluye servicios públicos ni estudio de arrendamiento.</p>
      </section>
    );
  }

  const loan = price * (1 - downPct / 100);
  const payment = monthlyPayment(loan, rate / 100, years);

  return (
    <section aria-labelledby={`${id}-t`} className="cost-calculator">
      <h2 id={`${id}-t`} className="text-2xl">Simula tu crédito</h2>
      <div className="cost-fields">
        <label>
          <span>Cuota inicial: {downPct}% ({formatPrice(Math.round((price * downPct) / 100))})</span>
          <input type="range" min={10} max={90} step={5} value={downPct} onChange={(event) => setDownPct(Number(event.target.value))} />
        </label>
        <label>
          <span>Tasa efectiva anual (%)</span>
          <input
            type="number"
            inputMode="decimal"
            min={0}
            max={40}
            step={0.1}
            value={rate}
            onChange={(event) => setRate(Math.min(40, Math.max(0, Number(event.target.value) || 0)))}
          />
        </label>
        <label>
          <span>Plazo</span>
          <select value={years} onChange={(event) => setYears(Number(event.target.value))}>
            {[5, 10, 15, 20, 25, 30].map((value) => <option key={value} value={value}>{value} años</option>)}
          </select>
        </label>
      </div>
      <dl>
        <div><dt>Monto a financiar</dt><dd>{formatPrice(Math.round(loan))}</dd></div>
        <div className="cost-total"><dt>Cuota mensual estimada</dt><dd>{formatPrice(Math.round(payment))}</dd></div>
      </dl>
      <p className="cost-note">
        Cálculo referencial con cuota fija. No incluye seguros ni constituye oferta de crédito; la tasa real la define tu banco.
      </p>
    </section>
  );
}
