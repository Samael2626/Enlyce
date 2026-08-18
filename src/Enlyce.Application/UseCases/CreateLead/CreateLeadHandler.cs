using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Application.UseCases.CreateLead;

public class CreateLeadHandler : ICommandHandler<CreateLeadCommand, CreateLeadResponse>
{
    private readonly ILeadRepository _leadRepo;
    private readonly IConsentimientoRepository _consentimientoRepo;
    private readonly IPoliticaTratamientoRepository _politicaRepo;
    private readonly IEmailSender _emailSender;

    public CreateLeadHandler(
        ILeadRepository leadRepo,
        IConsentimientoRepository consentimientoRepo,
        IPoliticaTratamientoRepository politicaRepo,
        IEmailSender emailSender)
    {
        _leadRepo = leadRepo;
        _consentimientoRepo = consentimientoRepo;
        _politicaRepo = politicaRepo;
        _emailSender = emailSender;
    }

    public async Task<CreateLeadResponse> HandleAsync(CreateLeadCommand command, CancellationToken ct = default)
    {
        var email = Email.Create(command.Email);
        var telefono = command.Telefono is not null ? Telefono.Create(command.Telefono) : null;

        if (await _leadRepo.ExistsByEmailAsync(email))
            throw new InvalidOperationException($"Ya existe un lead con email {command.Email}");

        var lead = Lead.Crear(command.Nombre, email, telefono, command.Fuente ?? "Manual",
            command.AutorizacionDatos, command.TipoOperacion);

        var saved = await _leadRepo.SaveAsync(lead);

        if (command.AutorizacionDatos)
        {
            var politica = await _politicaRepo.ObtenerActivaAsync();
            if (politica is not null)
            {
                var consentimiento = Consentimiento.Registrar(
                    saved.Id,
                    politica.TextoCompleto,
                    politica.Version,
                    "formulario_web");
                await _consentimientoRepo.AgregarAsync(consentimiento);
            }
        }

        await _emailSender.SendAsync(
            command.Email,
            "Bienvenido a Enlyce",
            $"Hola {command.Nombre}, tu lead fue registrado exitosamente.");

        return new CreateLeadResponse(
            saved.Id, saved.Nombre, saved.Email.Value,
            saved.Estado.ToString(), saved.FechaCreacion);
    }
}
