// Cuota fija (sistema frances) a partir de una tasa efectiva anual.
export function monthlyPayment(principal: number, annualRate: number, years: number): number {
  const months = years * 12;
  if (principal <= 0 || months <= 0) return 0;
  const monthlyRate = Math.pow(1 + annualRate, 1 / 12) - 1;
  if (monthlyRate === 0) return principal / months;
  return (principal * monthlyRate) / (1 - Math.pow(1 + monthlyRate, -months));
}
