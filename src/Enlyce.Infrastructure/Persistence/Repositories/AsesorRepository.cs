using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class AsesorRepository : IAsesorRepository
{
    private readonly EnlyceDbContext _context;

    public AsesorRepository(EnlyceDbContext context)
    {
        _context = context;
    }

    public async Task<Asesor?> ObtenerPorCorreoAsync(string correo)
    {
        return await _context.Asesores
            .FirstOrDefaultAsync(a => a.Correo.Value == correo);
    }

    public async Task<Asesor?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Asesores.FindAsync(id);
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        return await _context.Asesores.AnyAsync(a => a.Correo.Value == correo);
    }

    public async Task AgregarAsync(Asesor asesor)
    {
        await _context.Asesores.AddAsync(asesor);
        await _context.SaveChangesAsync();
    }

    public async Task GuardarAsync(Asesor asesor)
    {
        _context.Asesores.Update(asesor);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Asesor>> ObtenerTodosAsync()
    {
        return await _context.Asesores.ToListAsync();
    }

    public async Task<Guid?> ObtenerAsesorConMenosOportunidadesAbiertasAsync(CancellationToken ct = default)
    {
        return await _context.Asesores
            .AsNoTracking()
            .Where(advisor => advisor.Activo && advisor.Rol == "Asesor")
            .Select(advisor => new
            {
                advisor.Id,
                advisor.FechaCreacion,
                Workload = _context.Leads.Count(lead =>
                    lead.AsesorAsignadoId == advisor.Id &&
                    lead.Activo &&
                    lead.EtapaPipeline != EtapasPipeline.CerradoGanado &&
                    lead.EtapaPipeline != EtapasPipeline.CerradoPerdido)
            })
            .OrderBy(advisor => advisor.Workload)
            .ThenBy(advisor => advisor.FechaCreacion)
            .ThenBy(advisor => advisor.Id)
            .Select(advisor => (Guid?)advisor.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Guid?> ObtenerSiguienteAsesorEnRotacionAsync(CancellationToken ct = default)
    {
        await _context.Database.OpenConnectionAsync(ct);
        long sequence;
        try
        {
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "INSERT INTO \"LeadDistributionSettings\" (\"Id\", \"Rule\", \"RoundRobinCursor\") VALUES (1, 'LeastOpenLeads', 0) ON CONFLICT (\"Id\") DO NOTHING";
            if (_context.Database.CurrentTransaction is { } currentTransaction)
                command.Transaction = currentTransaction.GetDbTransaction();
            await command.ExecuteNonQueryAsync(ct);

            command.CommandText = "UPDATE \"LeadDistributionSettings\" SET \"RoundRobinCursor\" = \"RoundRobinCursor\" + 1 WHERE \"Id\" = 1 RETURNING \"RoundRobinCursor\"";
            var result = await command.ExecuteScalarAsync(ct);
            if (result is null or DBNull)
                throw new InvalidOperationException("Falta la configuracion inicial del reparto de leads.");
            sequence = Convert.ToInt64(result);
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
        var advisorIds = await _context.Asesores.AsNoTracking()
            .Where(advisor => advisor.Activo && advisor.Rol == "Asesor")
            .OrderBy(advisor => advisor.FechaCreacion)
            .ThenBy(advisor => advisor.Id)
            .Select(advisor => advisor.Id)
            .ToListAsync(ct);

        return advisorIds.Count == 0 ? null : advisorIds[(int)((sequence - 1) % advisorIds.Count)];
    }
}
