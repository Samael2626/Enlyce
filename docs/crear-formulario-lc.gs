function createLcForm() {
  const lock = LockService.getScriptLock();
  lock.waitLock(30000);
  try {
    const properties = PropertiesService.getScriptProperties();
    const savedId = properties.getProperty('LC_FORM_ID');
    if (savedId) {
      const existing = FormApp.openById(savedId);
      if (properties.getProperty('LC_FORM_COMPLETE') !== 'true') {
        throw new Error('Hay un formulario incompleto: ' + existing.getEditUrl() + '. Revisarlo antes de continuar; no se crearon duplicados.');
      }
      logLinks(existing);
      return;
    }

    const form = FormApp.create('L&C | Información para preparar la web', false);
    properties.setProperty('LC_FORM_ID', form.getId());
    form.setDescription(
      'Queremos preparar una web que muestre únicamente inmuebles disponibles y autorizados por L&C. ' +
      'Estas 12 preguntas nos ayudan a definir el inventario, los responsables y los pendientes. ' +
      'Si algo no está claro, puedes responder «No sé / Por definir». ' +
      'No incluyas cédulas, contratos, contraseñas ni datos privados de propietarios. ' +
      'Responder este formulario no autoriza por sí solo la publicación de ningún inmueble.'
    ).setCollectEmail(false)
      .setLimitOneResponsePerUser(false)
      .setPublishingSummary(false)
      .setShuffleQuestions(false)
      .setConfirmationMessage('Gracias. Con estas respuestas organizaremos los pendientes y prepararemos el primer lote de inmuebles para revisión de L&C.');

    form.addTextItem().setTitle('1. ¿Cuál es tu nombre y cargo?').setRequired(true);
    addChoice(form, '2. ¿Dónde está hoy el inventario más actualizado?', [
      'WASI', 'CRM interno de L&C', 'Excel o Google Sheets',
      'Está repartido en varias fuentes', 'No sé / Por definir'
    ], true);
    addChoice(form, '3. ¿Ya tienen una lista de inmuebles que desean mostrar en la web?', [
      'Sí, está definida', 'Tenemos algunos elegidos, falta completar la lista',
      'Todavía hay que seleccionarlos', 'No sé / Por definir'
    ]);
    form.addParagraphTextItem()
      .setTitle('4. ¿Qué inmuebles o sectores quieres priorizar para el lanzamiento?')
      .setHelpText('Puedes indicar códigos internos, sectores o tipos de inmueble. Si aún no lo sabes, escribe «Por definir». No incluyas direcciones privadas.')
      .setRequired(true);
    form.addTextItem().setTitle('5. ¿Quién puede confirmar precios y disponibilidad?')
      .setHelpText('Nombre o cargo. Si no hay responsable, escribe «Por definir».').setRequired(true);
    addChoice(form, '6. ¿Tienen autorización documentada para publicar los inmuebles en la web de L&C?', [
      'Sí, para todos los que queremos publicar', 'Solo para algunos',
      'Tenemos permisos, pero falta confirmar si incluyen la web',
      'No está documentada', 'No sé / Por definir'
    ]);
    form.addParagraphTextItem().setTitle('7. ¿Qué soporte de autorización utilizan y quién lo custodia?')
      .setHelpText('Por ejemplo: formato firmado, contrato o comunicación verificable. Indica el tipo de soporte y el responsable, sin copiar documentos. También puedes escribir «No sé / Por definir».')
      .setRequired(true);
    addChoice(form, '8. ¿Cómo están las fotografías de los inmuebles que quieren publicar?', [
      'Están completas y revisadas, con permiso de uso confirmado',
      'Están disponibles, pero falta revisarlas o confirmar el permiso de uso',
      'Solo algunos inmuebles tienen material suficiente',
      'Falta tomar o conseguir fotografías', 'No sé / Por definir'
    ]);
    form.addParagraphTextItem().setTitle('9. ¿Dónde están las fotos y quién puede entregarlas?')
      .setHelpText('Indica la plataforma o carpeta y el responsable. No pegues contraseñas ni enlaces privados. Puedes escribir «Por definir».')
      .setRequired(true);
    form.addTextItem().setTitle('10. ¿Quién dará la aprobación final antes de publicar?')
      .setHelpText('Nombre o cargo. Esta persona revisará la ficha tal como la verá el cliente. Puedes escribir «Por definir».')
      .setRequired(true);
    form.addParagraphTextItem().setTitle('11. ¿Quién avisará cuando un inmueble se venda, se arriende o deba retirarse?')
      .setHelpText('Indica responsable y canal para reportarlo. Si es posible, indica también quién recibirá las consultas de la web. Puedes escribir «Por definir».')
      .setRequired(true);
    addChoice(form, '12. ¿Estás de acuerdo con esta regla de publicación?', [
      'Sí, de acuerdo', 'De acuerdo, pero quiero revisar algunos detalles',
      'Necesitamos conversarlo antes de aprobarla', 'No sé / Por definir'
    ], false, 'Solo publicar inmuebles disponibles, con precio y ubicación confirmados, fotos revisadas, permiso verificable, asesor asignado y aprobación final. Estar en CRM o WASI no basta.');

    const sheet = SpreadsheetApp.create('L&C | Respuestas para preparar la web');
    properties.setProperty('LC_RESPONSES_ID', sheet.getId());
    form.setDestination(FormApp.DestinationType.SPREADSHEET, sheet.getId());
    form.setPublished(true);
    form.setAcceptingResponses(true);
    properties.setProperty('LC_FORM_COMPLETE', 'true');
    logLinks(form);
  } finally {
    lock.releaseLock();
  }
}

function addChoice(form, title, choices, allowOther, help) {
  const item = form.addMultipleChoiceItem().setTitle(title)
    .setChoiceValues(choices).setRequired(true);
  if (allowOther) item.showOtherOption(true);
  if (help) item.setHelpText(help);
}

function logLinks(form) {
  console.log('ENLACE PARA RESPONDER: ' + form.getPublishedUrl());
  console.log('EDITAR FORMULARIO: ' + form.getEditUrl());
  const sheetId = PropertiesService.getScriptProperties().getProperty('LC_RESPONSES_ID');
  if (sheetId) console.log('VER RESPUESTAS: https://docs.google.com/spreadsheets/d/' + sheetId + '/edit');
}
