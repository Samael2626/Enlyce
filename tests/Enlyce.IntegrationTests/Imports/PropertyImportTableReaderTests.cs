using System.IO.Compression;
using System.Text;
using Enlyce.Infrastructure.Imports;

namespace Enlyce.IntegrationTests.Imports;

public sealed class PropertyImportTableReaderTests
{
    [Fact]
    public async Task ReadAsync_XlsxReadsFirstSheetValues()
    {
        using var file = Workbook("<c r=\"A2\" t=\"inlineStr\"><is><t>APT-01</t></is></c><c r=\"B2\" t=\"inlineStr\"><is><t>Bogotá</t></is></c>");

        var result = await new PropertyImportTableReader().ReadAsync(file, "inmuebles.xlsx");

        Assert.Equal(["Código", "Ciudad"], result.Columns);
        Assert.Equal(["APT-01", "Bogotá"], Assert.Single(result.Rows));
    }

    [Fact]
    public async Task ReadAsync_XlsxRejectsFormulaEvenWithCachedValue()
    {
        using var file = Workbook("<c r=\"A2\"><f>1+1</f><v>2</v></c>");

        var error = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new PropertyImportTableReader().ReadAsync(file, "inmuebles.xlsx"));

        Assert.Contains("fórmulas", error.Message);
    }

    [Fact]
    public async Task ReadAsync_XlsReadsFirstSheetValues()
    {
        await using var file = Fixture("inmuebles-valores.xls");

        var result = await new PropertyImportTableReader().ReadAsync(file, "inmuebles.xls");

        Assert.Equal(["Codigo", "Ciudad"], result.Columns);
        Assert.Equal(["APT-01", "Bogota"], Assert.Single(result.Rows));
    }

    [Fact]
    public async Task ReadAsync_XlsRejectsFormulaEvenWithCachedValue()
    {
        await using var file = Fixture("inmuebles-formula.xls");

        var error = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new PropertyImportTableReader().ReadAsync(file, "inmuebles.xls"));

        Assert.Contains("fórmulas", error.Message);
    }

    private static FileStream Fixture(string name) => File.OpenRead(
        Path.Combine(AppContext.BaseDirectory, "Imports", "Fixtures", name));

    private static MemoryStream Workbook(string rowCells)
    {
        const string spreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            Add(archive, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="xml" ContentType="application/xml"/><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/></Types>
                """);
            Add(archive, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>
                """);
            Add(archive, "xl/workbook.xml", $"<workbook xmlns=\"{spreadsheetNamespace}\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Inmuebles\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
            Add(archive, "xl/_rels/workbook.xml.rels", """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/></Relationships>
                """);
            Add(archive, "xl/worksheets/sheet1.xml", $"<worksheet xmlns=\"{spreadsheetNamespace}\"><sheetData><row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>Código</t></is></c><c r=\"B1\" t=\"inlineStr\"><is><t>Ciudad</t></is></c></row><row r=\"2\">{rowCells}</row></sheetData></worksheet>");
        }
        output.Position = 0;
        return output;
    }

    private static void Add(ZipArchive archive, string path, string xml)
    {
        using var writer = new StreamWriter(archive.CreateEntry(path).Open(), new UTF8Encoding(false));
        writer.Write(xml);
    }
}
