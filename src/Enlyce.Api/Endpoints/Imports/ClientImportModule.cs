using Enlyce.Application.Imports;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Api.Endpoints.Imports;

public static class ClientImportModule
{
    private const long MaxFileBytes = 10 * 1024 * 1024;
    private const int MaxPreviewRows = 100;

    public static void MapClientImport(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/import/clientes")
            .WithTags("Importaciones")
            .RequireAuthorization();

        group.MapPost("/previsualizar", async (
            IFormFile archivo,
            [Microsoft.AspNetCore.Mvc.FromForm] int? nameColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? surnameColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? emailColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? phoneColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? canReceiveEmailColumn,
            HttpContext http,
            IClientImportTableReader reader,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("Administrador") && EndpointAccess.AdvisorId(http.User) is null)
                return Results.Forbid();
            if (archivo.Length is <= 0 or > MaxFileBytes)
                return Results.BadRequest(new { error = "El archivo debe pesar entre 1 byte y 10 MB." });

            ClientImportTable table;
            await using (var stream = archivo.OpenReadStream())
            {
                try
                {
                    table = await reader.ReadAsync(stream, archivo.FileName, ct);
                }
                catch (InvalidDataException exception)
                {
                    return Results.BadRequest(new { error = exception.Message });
                }
                catch (Exception exception) when (exception is IOException or ArgumentException)
                {
                    return Results.BadRequest(new { error = "No se pudo leer el archivo. Revisa su formato." });
                }
            }

            var mapping = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                [ClientImportFields.Name] = nameColumn?.ToString(),
                [ClientImportFields.Surname] = surnameColumn?.ToString(),
                [ClientImportFields.Email] = emailColumn?.ToString(),
                [ClientImportFields.Phone] = phoneColumn?.ToString(),
                [ClientImportFields.CanReceiveAutomatedEmail] = canReceiveEmailColumn?.ToString()
            };
            if (!nameColumn.HasValue)
                return Results.Ok(new ClientImportPreviewResponse(
                    table.Rows.Count, 0, 0, 0, table.Columns, []));

            if (!ValidateMapping(mapping, table.Columns, out var mappingError))
                return Results.BadRequest(new { error = mappingError, sourceColumns = table.Columns });

            var nameIndex = nameColumn!.Value;
            var surnameIndex = surnameColumn ?? -1;
            var emailIndex = emailColumn ?? -1;
            var phoneIndex = phoneColumn ?? -1;
            var canReceiveEmailIndex = canReceiveEmailColumn ?? -1;
            var candidateEmails = table.Rows
                .Select(row => emailIndex < 0 ? null : Cell(row, emailIndex))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(TryEmail)
                .Where(email => email is not null)
                .Select(email => email!)
                .Select(email => email!.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var visibleIds = await EndpointAccess.VisibleContactIdsAsync(http.User, leads, contacts, ct);
            var existing = new HashSet<string>(await contacts.GetExistingEmailsAsync(candidateEmails, visibleIds, ct),
                StringComparer.OrdinalIgnoreCase);
            var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var responseRows = new List<ClientImportPreviewRow>(Math.Min(table.Rows.Count, MaxPreviewRows));
            var valid = 0;
            var invalid = 0;
            var duplicates = 0;
            for (var index = 0; index < table.Rows.Count; index++)
            {
                var row = table.Rows[index];
                var name = Cell(row, nameIndex);
                var surname = Cell(row, surnameIndex);
                var emailText = emailIndex < 0 ? null : Cell(row, emailIndex);
                var phone = phoneIndex < 0 ? null : Cell(row, phoneIndex);
                var canReceiveAutomatedEmail = canReceiveEmailIndex >= 0 &&
                    ParseConsent(Cell(row, canReceiveEmailIndex));
                var email = TryEmail(emailText);
                var parsedPhone = TryPhone(phone);
                var error = ValidateRow(name, surname, emailText, email, phone, parsedPhone);
                var duplicate = error is null && email is not null &&
                    (existing.Contains(email.Value) || !seenInFile.Add(email.Value));
                if (duplicate)
                    duplicates++;
                if (error is null)
                    valid++;
                else
                    invalid++;

                if (responseRows.Count < MaxPreviewRows)
                    responseRows.Add(new ClientImportPreviewRow(index + 2, name, surname, emailText, phone,
                        canReceiveAutomatedEmail, error, duplicate));
            }

            return Results.Ok(new ClientImportPreviewResponse(
                table.Rows.Count, valid, invalid, duplicates, table.Columns, responseRows));
        })
        .DisableAntiforgery()
        .WithName("PreviewClientImport");

        group.MapPost("/confirmar", ConfirmAsync)
            .DisableAntiforgery()
            .WithName("ConfirmClientImport");
    }

    private static async Task<IResult> ConfirmAsync(
        IFormFile archivo,
        [Microsoft.AspNetCore.Mvc.FromForm] int? nameColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? surnameColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? emailColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? phoneColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? canReceiveEmailColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] Guid? advisorId,
        HttpContext http,
        IClientImportTableReader reader,
        IContactRepository contacts,
        IAsesorRepository advisors,
        ILeadRepository leads,
        CancellationToken ct)
    {
        if (archivo.Length is <= 0 or > MaxFileBytes)
            return Results.BadRequest(new { error = "El archivo debe pesar entre 1 byte y 10 MB." });

        var actorAdvisorId = EndpointAccess.AdvisorId(http.User);
        if (!http.User.IsInRole("Administrador") && actorAdvisorId is null)
            return Results.Forbid();
        var ownerAdvisorId = http.User.IsInRole("Administrador") ? advisorId : actorAdvisorId;
        if (ownerAdvisorId.HasValue &&
            await advisors.ObtenerPorIdAsync(ownerAdvisorId.Value) is not { Activo: true, Rol: "Asesor" })
            return Results.BadRequest(new { error = "El asesor asignado no es valido." });

        ClientImportTable table;
        await using (var stream = archivo.OpenReadStream())
        {
            try
            {
                table = await reader.ReadAsync(stream, archivo.FileName, ct);
            }
            catch (InvalidDataException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            catch (Exception exception) when (exception is IOException or ArgumentException)
            {
                return Results.BadRequest(new { error = "No se pudo leer el archivo. Revisa su formato." });
            }
        }

        var mapping = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            [ClientImportFields.Name] = nameColumn?.ToString(),
            [ClientImportFields.Surname] = surnameColumn?.ToString(),
            [ClientImportFields.Email] = emailColumn?.ToString(),
            [ClientImportFields.Phone] = phoneColumn?.ToString(),
            [ClientImportFields.CanReceiveAutomatedEmail] = canReceiveEmailColumn?.ToString()
        };
        if (!ValidateMapping(mapping, table.Columns, out var mappingError))
            return Results.BadRequest(new { error = mappingError, sourceColumns = table.Columns });

        var nameIndex = nameColumn!.Value;
        var surnameIndex = surnameColumn ?? -1;
        var emailIndex = emailColumn ?? -1;
        var phoneIndex = phoneColumn ?? -1;
        var canReceiveEmailIndex = canReceiveEmailColumn ?? -1;
        var candidateEmails = table.Rows.Select(row => emailIndex < 0 ? null : TryEmail(Cell(row, emailIndex)))
            .Where(email => email is not null).Select(email => email!.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var visibleIds = await EndpointAccess.VisibleContactIdsAsync(http.User, leads, contacts, ct);
        var existing = new HashSet<string>(await contacts.GetExistingEmailsAsync(candidateEmails, visibleIds, ct),
            StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var newContacts = new List<Contact>();
        var resultRows = new List<ClientImportPreviewRow>(Math.Min(MaxPreviewRows, table.Rows.Count));
        var invalid = 0;
        var duplicates = 0;

        for (var index = 0; index < table.Rows.Count; index++)
        {
            var row = table.Rows[index];
            var name = Cell(row, nameIndex);
            var surname = Cell(row, surnameIndex);
            var emailText = emailIndex < 0 ? null : Cell(row, emailIndex);
            var phoneText = phoneIndex < 0 ? null : Cell(row, phoneIndex);
            var canReceiveAutomatedEmail = canReceiveEmailIndex >= 0 &&
                ParseConsent(Cell(row, canReceiveEmailIndex));
            var email = TryEmail(emailText);
            var phone = TryPhone(phoneText);
            var error = ValidateRow(name, surname, emailText, email, phoneText, phone);
            var duplicate = email is not null && (existing.Contains(email.Value) || !seen.Add(email.Value));

            if (error is not null)
                invalid++;
            else if (duplicate)
                duplicates++;
            else
                newContacts.Add(Contact.CreateImported(name!, surname, email?.Value, phone, ownerAdvisorId,
                    canReceiveAutomatedEmail));

            if (resultRows.Count < MaxPreviewRows)
                resultRows.Add(new ClientImportPreviewRow(index + 2, name, surname, emailText, phoneText,
                    canReceiveAutomatedEmail,
                    error ?? (duplicate ? "Email duplicado." : null), duplicate));
        }

        try
        {
            await contacts.AddImportedAsync(newContacts, ct);
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new { error = "No se pudo completar la importacion. Revisa el archivo y vuelve a previsualizar." });
        }

        return Results.Ok(new ClientImportConfirmationResponse(
            table.Rows.Count, newContacts.Count, invalid, duplicates, resultRows));
    }

    private static bool ValidateMapping(
        IReadOnlyDictionary<string, string?> mapping,
        IReadOnlyList<string> columns,
        out string? error)
    {
        if (!int.TryParse(mapping[ClientImportFields.Name], out var nameIndex) ||
            nameIndex < 0 || nameIndex >= columns.Count)
        {
            error = "Mapea una columna para el nombre.";
            return false;
        }

        var emailIndex = -1;
        var phoneIndex = -1;
        var surnameIndex = -1;
        var consentIndex = -1;
        if (mapping[ClientImportFields.Email] is { } emailText &&
            (!int.TryParse(emailText, out emailIndex) || emailIndex < 0 || emailIndex >= columns.Count))
        {
            error = "La columna de email no existe.";
            return false;
        }

        if (mapping[ClientImportFields.Surname] is { } surnameText &&
            (!int.TryParse(surnameText, out surnameIndex) || surnameIndex < 0 || surnameIndex >= columns.Count))
        {
            error = "La columna de apellidos no existe.";
            return false;
        }

        if (emailIndex >= 0 && emailIndex == nameIndex)
        {
            error = "Nombre y email deben venir de columnas distintas.";
            return false;
        }

        if (mapping[ClientImportFields.Phone] is { } phoneText &&
            (!int.TryParse(phoneText, out phoneIndex) || phoneIndex < 0 || phoneIndex >= columns.Count))
        {
            error = "La columna de telefono no existe.";
            return false;
        }

        if (mapping[ClientImportFields.CanReceiveAutomatedEmail] is { } consentText &&
            (!int.TryParse(consentText, out consentIndex) || consentIndex < 0 || consentIndex >= columns.Count))
        {
            error = "La columna Recibe correo no existe.";
            return false;
        }

        var assigned = new[] { nameIndex, emailIndex, phoneIndex, surnameIndex, consentIndex }
            .Where(index => index >= 0).ToArray();
        if (assigned.Distinct().Count() != assigned.Length)
        {
            error = "Cada campo debe venir de una columna distinta.";
            return false;
        }

        error = null;
        return true;
    }

    private static string? Cell(IReadOnlyList<string?> row, int index) =>
        index >= 0 && index < row.Count ? row[index]?.Trim() is { Length: > 0 } value ? value : null : null;

    private static Email? TryEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        try
        {
            return Email.Create(value);
        }
        catch (Enlyce.Domain.Errors.DomainError)
        {
            return null;
        }
    }

    private static Telefono? TryPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        try
        {
            return Telefono.Create(value);
        }
        catch (Enlyce.Domain.Errors.DomainError)
        {
            return null;
        }
    }

    private static bool ParseConsent(string? value) => value?.Trim().ToLowerInvariant() is
        "si" or "sí" or "yes" or "true" or "1" or "x" or "acepta";

    private static string? ValidateRow(string? name, string? surname, string? emailText, Email? email,
        string? phoneText, Telefono? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Falta el nombre.";
        if (name.Length > 200)
            return "El nombre supera 200 caracteres.";
        if (surname?.Length > 200)
            return "Los apellidos superan 200 caracteres.";
        if (emailText is not null && email is null)
            return "El email no tiene formato valido.";
        if (phoneText is not null && phone is null)
            return "El telefono no tiene formato valido.";
        return null;
    }
}

public sealed record ClientImportConfirmationResponse(
    int TotalRows, int ImportedRows, int InvalidRows, int DuplicateRows,
    IReadOnlyList<ClientImportPreviewRow> Rows);
