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
            [Microsoft.AspNetCore.Mvc.FromForm] int? customerTypeColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? customerStatusColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? addressColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? countryColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? departmentColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? cityColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? landlineColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? dateOfBirthColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? identificationNumberColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? referredByColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? acquisitionMediumColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? additionalDataColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? customerLabelsColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int? customerFunnelColumn,
            [Microsoft.AspNetCore.Mvc.FromForm] int previewPage,
            [Microsoft.AspNetCore.Mvc.FromForm] int previewPageSize,
            HttpContext http,
            IClientImportTableReader reader,
            IContactRepository contacts,
            ICustomerFunnelStageRepository funnelStages,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("Administrador") && EndpointAccess.AdvisorId(http.User) is null)
                return Results.Forbid();
            if (archivo.Length is <= 0 or > MaxFileBytes)
                return Results.BadRequest(new { error = "El archivo debe pesar entre 1 byte y 10 MB." });
            if (previewPage < 1 || previewPageSize is < 1 or > MaxPreviewRows)
                return Results.BadRequest(new { error = "La pagina debe ser positiva y el tamano debe estar entre 1 y 100 filas." });

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
                [ClientImportFields.CanReceiveAutomatedEmail] = canReceiveEmailColumn?.ToString(),
                [ClientImportFields.CustomerType] = customerTypeColumn?.ToString(),
                [ClientImportFields.CustomerStatus] = customerStatusColumn?.ToString(),
                [ClientImportFields.Address] = addressColumn?.ToString(),
                [ClientImportFields.Country] = countryColumn?.ToString(),
                [ClientImportFields.Department] = departmentColumn?.ToString(),
                [ClientImportFields.City] = cityColumn?.ToString(),
                [ClientImportFields.Landline] = landlineColumn?.ToString(),
                [ClientImportFields.DateOfBirth] = dateOfBirthColumn?.ToString(),
                [ClientImportFields.IdentificationNumber] = identificationNumberColumn?.ToString(),
                [ClientImportFields.ReferredBy] = referredByColumn?.ToString(),
                [ClientImportFields.AcquisitionMedium] = acquisitionMediumColumn?.ToString(),
                [ClientImportFields.AdditionalData] = additionalDataColumn?.ToString(),
                [ClientImportFields.CustomerLabels] = customerLabelsColumn?.ToString(),
                [ClientImportFields.CustomerFunnel] = customerFunnelColumn?.ToString()
            };
            if (!nameColumn.HasValue)
                return Results.Ok(new ClientImportPreviewResponse(
                    table.Rows.Count, 0, 0, 0, table.Columns, [], previewPage, previewPageSize));

            if (!ValidateMapping(mapping, table.Columns, out var mappingError))
                return Results.BadRequest(new { error = mappingError, sourceColumns = table.Columns });

            var nameIndex = nameColumn!.Value;
            var surnameIndex = surnameColumn ?? -1;
            var emailIndex = emailColumn ?? -1;
            var phoneIndex = phoneColumn ?? -1;
            var canReceiveEmailIndex = canReceiveEmailColumn ?? -1;
            var customerTypeIndex = customerTypeColumn ?? -1;
            var customerStatusIndex = customerStatusColumn ?? -1;
            var addressIndex = addressColumn ?? -1;
            var countryIndex = countryColumn ?? -1;
            var departmentIndex = departmentColumn ?? -1;
            var cityIndex = cityColumn ?? -1;
            var landlineIndex = landlineColumn ?? -1;
            var dateOfBirthIndex = dateOfBirthColumn ?? -1;
            var identificationNumberIndex = identificationNumberColumn ?? -1;
            var referredByIndex = referredByColumn ?? -1;
            var acquisitionMediumIndex = acquisitionMediumColumn ?? -1;
            var additionalDataIndex = additionalDataColumn ?? -1;
            var customerLabelsIndex = customerLabelsColumn ?? -1;
            var customerFunnelIndex = customerFunnelColumn ?? -1;
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
            var stages = await funnelStages.GetAllAsync(ct);
            var validStages = new HashSet<string>(stages.Select(stage => StageKey(stage.FunnelName, stage.Name)), StringComparer.Ordinal);
            var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var responseRows = new List<ClientImportPreviewRow>(Math.Min(table.Rows.Count, MaxPreviewRows));
            var firstPreviewRow = (long)(previewPage - 1) * previewPageSize;
            if (firstPreviewRow >= table.Rows.Count && table.Rows.Count > 0)
                return Results.BadRequest(new { error = "La pagina de vista previa no existe." });
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
                var landline = landlineIndex < 0 ? null : Cell(row, landlineIndex);
                var dateOfBirth = dateOfBirthIndex < 0 ? null : Cell(row, dateOfBirthIndex);
                var identificationNumber = identificationNumberIndex < 0 ? null : Cell(row, identificationNumberIndex);
                var referredBy = referredByIndex < 0 ? null : Cell(row, referredByIndex);
                var acquisitionMedium = acquisitionMediumIndex < 0 ? null : Cell(row, acquisitionMediumIndex);
                var additionalData = additionalDataIndex < 0 ? null : Cell(row, additionalDataIndex);
                var labels = customerLabelsIndex < 0 ? [] : ParseCustomerLabels(Cell(row, customerLabelsIndex));
                var funnel = customerFunnelIndex < 0 ? "Clientes" : Cell(row, customerFunnelIndex)?.Trim();
                if (string.IsNullOrWhiteSpace(funnel)) funnel = "Clientes";
                var canReceiveAutomatedEmail = canReceiveEmailIndex >= 0 &&
                    ParseConsent(Cell(row, canReceiveEmailIndex));
                var customerTypes = customerTypeIndex < 0 ? [] : ParseCustomerTypes(Cell(row, customerTypeIndex));
                var customerType = customerTypes.FirstOrDefault();
                var rawCustomerStatus = customerStatusIndex < 0 ? null : Cell(row, customerStatusIndex);
                var customerStatus = string.IsNullOrWhiteSpace(rawCustomerStatus)
                    ? DefaultStageForFunnel(funnel, stages)
                    : CanonicalStageName(funnel, rawCustomerStatus, stages);
                customerType = NormalizeCustomerType(customerType);
                customerStatus = NormalizeCustomerStatus(customerStatus);
                var address = addressIndex < 0 ? null : Cell(row, addressIndex);
                var country = countryIndex < 0 ? "Colombia" : Cell(row, countryIndex) ?? "Colombia";
                var department = departmentIndex < 0 ? null : Cell(row, departmentIndex);
                var city = cityIndex < 0 ? null : Cell(row, cityIndex);
                var email = TryEmail(emailText);
                var parsedPhone = TryPhone(phone);
                var error = HasUnsafeNumericIdentity(table, index, phoneIndex, landlineIndex, identificationNumberIndex)
                    ? "Excel guarda esta identificacion o telefono como numero de mas de 15 digitos y pudo redondearlo. Corrige la celda en el archivo y vuelve a importar."
                    : ValidateRow(name, surname, emailText, email, phone, parsedPhone,
                        customerTypes, customerStatus, address, country, department, city,
                        landline, TryPhone(landline), dateOfBirth, TryDateOfBirth(dateOfBirth),
                        identificationNumber, referredBy, acquisitionMedium, additionalData, labels, funnel, validStages);
                var duplicate = error is null && email is not null &&
                    (existing.Contains(email.Value) || !seenInFile.Add(email.Value));
                if (error is not null)
                    invalid++;
                else if (duplicate)
                    duplicates++;
                else
                    valid++;

                if (index >= firstPreviewRow && responseRows.Count < previewPageSize)
                    responseRows.Add(new ClientImportPreviewRow(index + 2, name, surname, emailText, phone,
                        canReceiveAutomatedEmail, GetMappedValues(row, mapping), error, duplicate));
            }

            return Results.Ok(new ClientImportPreviewResponse(
                table.Rows.Count, valid, invalid, duplicates, table.Columns, responseRows, previewPage, previewPageSize));
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
        [Microsoft.AspNetCore.Mvc.FromForm] int? customerTypeColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? customerStatusColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? addressColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? countryColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? departmentColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? cityColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? landlineColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? dateOfBirthColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? identificationNumberColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? referredByColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? acquisitionMediumColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? additionalDataColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? customerLabelsColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] int? customerFunnelColumn,
        [Microsoft.AspNetCore.Mvc.FromForm] Guid? advisorId,
        [Microsoft.AspNetCore.Mvc.FromForm] Guid importRequestId,
        HttpContext http,
        IClientImportTableReader reader,
        IContactRepository contacts,
        ICustomerFunnelStageRepository funnelStages,
        IAsesorRepository advisors,
        ILeadRepository leads,
        CancellationToken ct)
    {
        if (archivo.Length is <= 0 or > MaxFileBytes)
            return Results.BadRequest(new { error = "El archivo debe pesar entre 1 byte y 10 MB." });
        if (importRequestId == Guid.Empty)
            return Results.BadRequest(new { error = "Falta la clave de reintento de importacion." });

        var actorAdvisorId = EndpointAccess.AdvisorId(http.User);
        if (!http.User.IsInRole("Administrador") && actorAdvisorId is null)
            return Results.Forbid();
        var ownerAdvisorId = http.User.IsInRole("Administrador") ? advisorId : actorAdvisorId;
        if (ownerAdvisorId.HasValue &&
            await advisors.ObtenerPorIdAsync(ownerAdvisorId.Value) is not { Activo: true, Rol: "Asesor" })
            return Results.BadRequest(new { error = "El asesor asignado no es valido." });
        var actorId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? http.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(actorId))
            return Results.Forbid();

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
            [ClientImportFields.CanReceiveAutomatedEmail] = canReceiveEmailColumn?.ToString(),
            [ClientImportFields.CustomerType] = customerTypeColumn?.ToString(),
            [ClientImportFields.CustomerStatus] = customerStatusColumn?.ToString(),
            [ClientImportFields.Address] = addressColumn?.ToString(),
            [ClientImportFields.Country] = countryColumn?.ToString(),
            [ClientImportFields.Department] = departmentColumn?.ToString(),
            [ClientImportFields.City] = cityColumn?.ToString(),
            [ClientImportFields.Landline] = landlineColumn?.ToString(),
            [ClientImportFields.DateOfBirth] = dateOfBirthColumn?.ToString(),
            [ClientImportFields.IdentificationNumber] = identificationNumberColumn?.ToString(),
            [ClientImportFields.ReferredBy] = referredByColumn?.ToString(),
            [ClientImportFields.AcquisitionMedium] = acquisitionMediumColumn?.ToString(),
            [ClientImportFields.AdditionalData] = additionalDataColumn?.ToString(),
            [ClientImportFields.CustomerLabels] = customerLabelsColumn?.ToString(),
            [ClientImportFields.CustomerFunnel] = customerFunnelColumn?.ToString()
        };
        if (!ValidateMapping(mapping, table.Columns, out var mappingError))
            return Results.BadRequest(new { error = mappingError, sourceColumns = table.Columns });

        string fileHash;
        await using (var hashStream = archivo.OpenReadStream())
            fileHash = Convert.ToHexString(await System.Security.Cryptography.SHA256.HashDataAsync(hashStream, ct));
        var payloadDescriptor = System.Text.Json.JsonSerializer.Serialize(new
        {
            FileHash = fileHash,
            Mapping = mapping.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value),
            OwnerAdvisorId = ownerAdvisorId
        });
        var payloadHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(payloadDescriptor)));
        if (await contacts.GetImportBatchAsync(actorId, importRequestId, ct) is { } previousBatch)
        {
            if (!string.Equals(previousBatch.PayloadHash, payloadHash, StringComparison.OrdinalIgnoreCase))
                return Results.Conflict(new { error = "La clave de reintento ya se uso con otro archivo o mapeo. Inicia una importacion nueva." });
            return Results.Ok(ToResponse(previousBatch));
        }

        var nameIndex = nameColumn!.Value;
        var surnameIndex = surnameColumn ?? -1;
        var emailIndex = emailColumn ?? -1;
        var phoneIndex = phoneColumn ?? -1;
        var canReceiveEmailIndex = canReceiveEmailColumn ?? -1;
        var customerTypeIndex = customerTypeColumn ?? -1;
        var customerStatusIndex = customerStatusColumn ?? -1;
        var addressIndex = addressColumn ?? -1;
        var countryIndex = countryColumn ?? -1;
        var departmentIndex = departmentColumn ?? -1;
        var cityIndex = cityColumn ?? -1;
        var landlineIndex = landlineColumn ?? -1;
        var dateOfBirthIndex = dateOfBirthColumn ?? -1;
        var identificationNumberIndex = identificationNumberColumn ?? -1;
        var referredByIndex = referredByColumn ?? -1;
        var acquisitionMediumIndex = acquisitionMediumColumn ?? -1;
        var additionalDataIndex = additionalDataColumn ?? -1;
        var customerLabelsIndex = customerLabelsColumn ?? -1;
        var customerFunnelIndex = customerFunnelColumn ?? -1;
        var candidateEmails = table.Rows.Select(row => emailIndex < 0 ? null : TryEmail(Cell(row, emailIndex)))
            .Where(email => email is not null).Select(email => email!.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var visibleIds = await EndpointAccess.VisibleContactIdsAsync(http.User, leads, contacts, ct);
        var existing = new HashSet<string>(await contacts.GetExistingEmailsAsync(candidateEmails, visibleIds, ct),
            StringComparer.OrdinalIgnoreCase);
        var stages = await funnelStages.GetAllAsync(ct);
        var validStages = new HashSet<string>(stages.Select(stage => StageKey(stage.FunnelName, stage.Name)), StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var newContacts = new List<Contact>();
        var importedContactLinks = new List<ClientImportContactLink>(table.Rows.Count);
        var resultRows = new List<ClientImportPreviewRow>(Math.Min(MaxPreviewRows, table.Rows.Count));
        var errorRows = new List<ClientImportErrorRow>();
        var invalid = 0;
        var duplicates = 0;

        for (var index = 0; index < table.Rows.Count; index++)
        {
            var row = table.Rows[index];
            var name = Cell(row, nameIndex);
            var surname = Cell(row, surnameIndex);
            var emailText = emailIndex < 0 ? null : Cell(row, emailIndex);
            var phoneText = phoneIndex < 0 ? null : Cell(row, phoneIndex);
            var landlineText = landlineIndex < 0 ? null : Cell(row, landlineIndex);
            var dateOfBirthText = dateOfBirthIndex < 0 ? null : Cell(row, dateOfBirthIndex);
            var identificationNumber = identificationNumberIndex < 0 ? null : Cell(row, identificationNumberIndex);
            var referredBy = referredByIndex < 0 ? null : Cell(row, referredByIndex);
            var acquisitionMedium = acquisitionMediumIndex < 0 ? null : Cell(row, acquisitionMediumIndex);
            var additionalData = additionalDataIndex < 0 ? null : Cell(row, additionalDataIndex);
            var customerLabels = customerLabelsIndex < 0 ? [] : ParseCustomerLabels(Cell(row, customerLabelsIndex));
            var customerFunnel = customerFunnelIndex < 0 ? "Clientes" : Cell(row, customerFunnelIndex)?.Trim();
            if (string.IsNullOrWhiteSpace(customerFunnel)) customerFunnel = "Clientes";
            // Un indicador afirmativo del archivo no es evidencia auditable del consentimiento.
            var canReceiveAutomatedEmail = false;
            var customerTypes = customerTypeIndex < 0 ? [] : ParseCustomerTypes(Cell(row, customerTypeIndex));
            var customerType = customerTypes.FirstOrDefault();
            var rawCustomerStatus = customerStatusIndex < 0 ? null : Cell(row, customerStatusIndex);
            var customerStatus = string.IsNullOrWhiteSpace(rawCustomerStatus)
                ? DefaultStageForFunnel(customerFunnel, stages)
                : CanonicalStageName(customerFunnel, rawCustomerStatus, stages);
            customerType = NormalizeCustomerType(customerType);
            customerStatus = NormalizeCustomerStatus(customerStatus);
            var address = addressIndex < 0 ? null : Cell(row, addressIndex);
            var country = countryIndex < 0 ? "Colombia" : Cell(row, countryIndex) ?? "Colombia";
            var department = departmentIndex < 0 ? null : Cell(row, departmentIndex);
            var city = cityIndex < 0 ? null : Cell(row, cityIndex);
            var email = TryEmail(emailText);
            var phone = TryPhone(phoneText);
            var landline = TryPhone(landlineText);
            var dateOfBirth = TryDateOfBirth(dateOfBirthText);
            var error = HasUnsafeNumericIdentity(table, index, phoneIndex, landlineIndex, identificationNumberIndex)
                ? "Excel guarda esta identificacion o telefono como numero de mas de 15 digitos y pudo redondearlo. Corrige la celda en el archivo y vuelve a importar."
                : ValidateRow(name, surname, emailText, email, phoneText, phone,
                    customerTypes, customerStatus, address, country, department, city,
                    landlineText, landline, dateOfBirthText, dateOfBirth, identificationNumber,
                    referredBy, acquisitionMedium, additionalData, customerLabels, customerFunnel, validStages);
            var duplicate = error is null && email is not null &&
                (existing.Contains(email.Value) || !seen.Add(email.Value));
            if (error is not null)
                invalid++;
            else if (duplicate)
                duplicates++;
            else
            {
                var contact = Contact.CreateImported(name!, surname, email?.Value, phone, ownerAdvisorId,
                    canReceiveAutomatedEmail, customerType, customerStatus, address, country, department, city,
                    landline, dateOfBirth, identificationNumber, referredBy, acquisitionMedium,
                    additionalData, customerFunnel, customerTypes, customerLabels);
                newContacts.Add(contact);
                importedContactLinks.Add(new ClientImportContactLink(index + 2, contact.Id));
            }

            if (error is not null || duplicate)
                errorRows.Add(new ClientImportErrorRow(index + 2,
                    error ?? "Email duplicado en contactos visibles o en el archivo."));

            if (resultRows.Count < MaxPreviewRows)
                resultRows.Add(new ClientImportPreviewRow(index + 2, name, surname, emailText, phoneText,
                    canReceiveAutomatedEmail, GetMappedValues(row, mapping),
                    error ?? (duplicate ? "Email duplicado." : null), duplicate));
        }

        try
        {
            var response = new ClientImportConfirmationResponse(
                table.Rows.Count, newContacts.Count, invalid, duplicates, resultRows, errorRows, importedContactLinks);
            var batch = ContactImportBatch.Create(importRequestId, actorId, payloadHash,
                response.TotalRows, response.ImportedRows, response.InvalidRows, response.DuplicateRows,
                System.Text.Json.JsonSerializer.Serialize(new ClientImportStoredResult(
                    response.Errors, response.ImportedContacts)));
            await contacts.AddImportedAsync(newContacts, batch, ct);
            return Results.Ok(response);
        }
        catch (DbUpdateException)
        {
            if (await contacts.GetImportBatchAsync(actorId, importRequestId, ct) is { } winningBatch &&
                string.Equals(winningBatch.PayloadHash, payloadHash, StringComparison.OrdinalIgnoreCase))
                return Results.Ok(ToResponse(winningBatch));
            return Results.Conflict(new { error = "No se pudo completar la importacion. Revisa el archivo y vuelve a previsualizar." });
        }
    }

    private static ClientImportConfirmationResponse ToResponse(ContactImportBatch batch)
    {
        try
        {
            if (System.Text.Json.JsonSerializer.Deserialize<ClientImportStoredResult>(batch.ErrorsJson) is { } stored)
                return new ClientImportConfirmationResponse(batch.TotalRows, batch.ImportedRows, batch.InvalidRows,
                    batch.DuplicateRows, [], stored.Errors, stored.ImportedContacts);
        }
        catch (System.Text.Json.JsonException)
        {
            // Lotes anteriores guardaban solo la lista de errores.
        }

        return new ClientImportConfirmationResponse(batch.TotalRows, batch.ImportedRows, batch.InvalidRows,
            batch.DuplicateRows, [],
            System.Text.Json.JsonSerializer.Deserialize<IReadOnlyList<ClientImportErrorRow>>(batch.ErrorsJson) ?? [], []);
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
        var customerTypeIndex = -1;
        var statusIndex = -1;
        var addressIndex = -1;
        var countryIndex = -1;
        var departmentIndex = -1;
        var cityIndex = -1;
        var landlineIndex = -1;
        var dateOfBirthIndex = -1;
        var identificationNumberIndex = -1;
        var referredByIndex = -1;
        var acquisitionMediumIndex = -1;
        var additionalDataIndex = -1;
        var customerLabelsIndex = -1;
        var customerFunnelIndex = -1;
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

        var optionalFields = new (string Key, string Label, Action<int> Set)[]
        {
            (ClientImportFields.CustomerType, "tipo de cliente", index => customerTypeIndex = index),
            (ClientImportFields.CustomerStatus, "estado de cliente", index => statusIndex = index),
            (ClientImportFields.Address, "direccion", index => addressIndex = index),
            (ClientImportFields.Country, "pais", index => countryIndex = index),
            (ClientImportFields.Department, "departamento", index => departmentIndex = index),
            (ClientImportFields.City, "ciudad", index => cityIndex = index),
            (ClientImportFields.Landline, "telefono fijo", index => landlineIndex = index),
            (ClientImportFields.DateOfBirth, "fecha de nacimiento", index => dateOfBirthIndex = index),
            (ClientImportFields.IdentificationNumber, "identificacion", index => identificationNumberIndex = index),
            (ClientImportFields.ReferredBy, "referido por", index => referredByIndex = index),
            (ClientImportFields.AcquisitionMedium, "medio de captacion", index => acquisitionMediumIndex = index),
            (ClientImportFields.AdditionalData, "datos adicionales", index => additionalDataIndex = index),
            (ClientImportFields.CustomerLabels, "etiquetas", index => customerLabelsIndex = index),
            (ClientImportFields.CustomerFunnel, "embudo", index => customerFunnelIndex = index)
        };
        foreach (var (key, label, set) in optionalFields)
        {
            if (mapping[key] is not { } columnText)
                continue;
            if (!int.TryParse(columnText, out var index) || index < 0 || index >= columns.Count)
            {
                error = $"La columna de {label} no existe.";
                return false;
            }
            set(index);
        }

        var assigned = new[] { nameIndex, emailIndex, phoneIndex, surnameIndex, consentIndex,
                customerTypeIndex, statusIndex, addressIndex, countryIndex, departmentIndex, cityIndex,
                landlineIndex, dateOfBirthIndex, identificationNumberIndex, referredByIndex,
                acquisitionMediumIndex, additionalDataIndex, customerLabelsIndex, customerFunnelIndex }
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

    private static bool HasUnsafeNumericIdentity(ClientImportTable table, int rowIndex,
        int phoneIndex, int landlineIndex, int identificationNumberIndex) =>
        table.UnsafeNumericIdentityCells is { } cells &&
        (phoneIndex >= 0 && cells.Contains((rowIndex, phoneIndex)) ||
         landlineIndex >= 0 && cells.Contains((rowIndex, landlineIndex)) ||
         identificationNumberIndex >= 0 && cells.Contains((rowIndex, identificationNumberIndex)));

    private static IReadOnlyDictionary<string, string?> GetMappedValues(
        IReadOnlyList<string?> row, IReadOnlyDictionary<string, string?> mapping) =>
        mapping.Where(pair => int.TryParse(pair.Value, out var index) && index >= 0)
            .ToDictionary(pair => pair.Key, pair => Cell(row, int.Parse(pair.Value!)));

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

    private static DateOnly? TryDateOfBirth(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string[] formats = ["yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy"];
        return DateOnly.TryParseExact(value.Trim(), formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var date) ? date : null;
    }

    private static bool ParseConsent(string? value) => value?.Trim().ToLowerInvariant() is
        "si" or "sí" or "yes" or "true" or "1" or "x" or "acepta";

    private static string? NormalizeCustomerType(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "comprador" => "Comprador",
        "arrendatario" or "inquilino" => "Arrendatario",
        "propietario" => "Propietario",
        "arrendador" => "Arrendador",
        null or "" => null,
        _ => value.Trim()
    };

    private static string[] ParseCustomerTypes(string? value) => value?.Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(NormalizeCustomerType).Where(type => type is not null).Select(type => type!).Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    private static string[] ParseCustomerLabels(string? value) => value?.Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    private static string NormalizeCustomerStatus(string value) => value.Trim().ToLowerInvariant() switch
    {
        "en proceso" or "enproceso" => "EnProceso",
        "convertido" => "Convertido",
        "recuperado" => "Recuperado",
        "perdido" => "Perdido",
        _ => value.Trim()
    };

    private static string DefaultStageForFunnel(string funnelName, IReadOnlyList<CustomerFunnelStage> stages) =>
        stages.Where(stage => string.Equals(stage.FunnelName, funnelName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(stage => stage.IsDefaultForInbound)
            .ThenBy(stage => stage.SortOrder)
            .Select(stage => stage.Name)
            .FirstOrDefault() ?? "Nuevo";

    private static string CanonicalStageName(string funnelName, string stageName,
        IReadOnlyList<CustomerFunnelStage> stages)
    {
        var normalized = NormalizeCustomerStatus(stageName);
        return stages.FirstOrDefault(stage =>
            string.Equals(stage.FunnelName, funnelName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(stage.Name, normalized, StringComparison.OrdinalIgnoreCase))?.Name ?? normalized;
    }

    private static string? ValidateRow(string? name, string? surname, string? emailText, Email? email,
        string? phoneText, Telefono? phone, string[] customerTypes, string customerStatus,
        string? address, string country, string? department, string? city,
        string? landlineText, Telefono? landline, string? dateOfBirthText, DateOnly? dateOfBirth,
        string? identificationNumber, string? referredBy, string? acquisitionMedium,
        string? additionalData, string[] customerLabels, string customerFunnel, ISet<string> validStages)
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
        if (landlineText is not null && landline is null)
            return "El telefono fijo no tiene formato valido.";
        if (dateOfBirthText is not null && dateOfBirth is null)
            return "La fecha de nacimiento debe usar AAAA-MM-DD o DD/MM/AAAA.";
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            return "La fecha de nacimiento no puede estar en el futuro.";
        if (customerTypes.Length > 10 || customerTypes.Any(type => type.Length > 30))
            return "Cada cliente admite hasta 10 tipos de máximo 30 caracteres.";
        if (customerStatus.Length > 100 || !validStages.Contains(StageKey(customerFunnel, customerStatus)))
            return "La etapa no existe en el embudo seleccionado.";
        if (customerLabels.Length > 20 || customerLabels.Any(label => label.Length > 50))
            return "Cada cliente admite hasta 20 etiquetas de maximo 50 caracteres.";
        if (string.IsNullOrWhiteSpace(customerFunnel) || customerFunnel.Length > 100)
            return "El embudo debe tener entre 1 y 100 caracteres.";
        if (address?.Length > 500 || country.Length > 100 || department?.Length > 100 || city?.Length > 100)
            return "Uno de los campos de ubicacion supera la longitud permitida.";
        if (identificationNumber?.Length > 50 || referredBy?.Length > 200 || acquisitionMedium?.Length > 200 || additionalData?.Length > 4_000)
            return "Uno de los campos del cliente supera la longitud permitida.";
        return null;
    }

    private static string StageKey(string funnel, string stage) =>
        $"{funnel.Trim().ToUpperInvariant()}\0{stage.Trim().ToUpperInvariant()}";
}

public sealed record ClientImportConfirmationResponse(
    int TotalRows, int ImportedRows, int InvalidRows, int DuplicateRows,
    IReadOnlyList<ClientImportPreviewRow> Rows, IReadOnlyList<ClientImportErrorRow> Errors,
    IReadOnlyList<ClientImportContactLink> ImportedContacts);
public sealed record ClientImportErrorRow(int RowNumber, string Error);
public sealed record ClientImportContactLink(int RowNumber, Guid ContactId);
public sealed record ClientImportStoredResult(
    IReadOnlyList<ClientImportErrorRow> Errors, IReadOnlyList<ClientImportContactLink> ImportedContacts);
