using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ERP.Models;

namespace ERP.Services
{

    public class InvoiceDocument : IDocument
    {
        public Invoice Model { get; }

        public InvoiceDocument(Invoice model)
        {
            Model = model;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Header().Row(row =>
                {
                    // BAL OLDAL: Számla adatai és VEVŐ
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"SZÁMLA: {Model.InvoiceNumber}").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().PaddingBottom(10).Text($"Kelt: {Model.IssueDate:yyyy.MM.dd}");
                        col.Item().PaddingBottom(10).Text($"Kelt: {Model.DueDate:yyyy.MM.dd}");

                        col.Item().Text("Vevő adatai:").FontSize(10).SemiBold();
                        col.Item().Text(Model.CustomerName).FontSize(12);
                    });

                    // JOBB OLDAL: Eladó adatai
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("ERP Demo Kft.").Bold();
                        col.Item().Text("1234 Budapest, Fő utca 1.");
                        col.Item().Text("Adószám: 12345678-2-42");
                    });
                });

                page.Content().PaddingVertical(40).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#");
                            header.Cell().Element(CellStyle).Text("Termék megnevezése");
                            header.Cell().Element(CellStyle).AlignRight().Text("Menny.");
                            header.Cell().Element(CellStyle).AlignRight().Text("Nettó ár");
                            header.Cell().Element(CellStyle).AlignRight().Text("Bruttó");
                            header.Cell().Element(CellStyle).AlignRight().Text("Áfa(%)");

                            // Adunk egy kis paddinget alulra, hogy ne csússzon össze a tartalommal
                            static IContainer CellStyle(IContainer container) => container.PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        foreach (var item in Model.Items)
                        {
                            table.Cell().Element(MainCellStyle).Text("1"); // Sorszám
                            table.Cell().Element(MainCellStyle).Text(item.ProductName);
                            table.Cell().Element(MainCellStyle).AlignRight().Text($"{item.Quantity}");
                            table.Cell().Element(MainCellStyle).AlignRight().Text($"{item.UnitPrice:N0} Ft");
                            table.Cell().Element(MainCellStyle).AlignRight().Text($"{item.LineTotal:N0} Ft");
                            table.Cell().Element(MainCellStyle).AlignRight().Text($"{item.TaxRate:N1}");

                            // A PaddingVertical(8) adja meg a sorok közötti távolságot
                            static IContainer MainCellStyle(IContainer container) => container.PaddingVertical(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                        }
                    });

                    // Összesítés eltolása kicsit lejjebb
                    col.Item().PaddingTop(30).AlignRight().Column(c => {
                        c.Item().Text($"Összes Nettó: {Model.TotalNet:N0} Ft").FontSize(10);
                        c.Item().Text($"ÁFA összesen: {Model.TotalTax:N0} Ft").FontSize(10);
                        c.Item().Text($"Fizetendő Bruttó: {Model.TotalGross:N0} Ft").FontSize(16).Bold();
                    });
                });
            });
        }
    }
}
