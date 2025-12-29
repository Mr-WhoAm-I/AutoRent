using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing; // System.Drawing.Primitives для цветов
using CarRental.Domain.DTO;

using ExcelLicense = OfficeOpenXml.LicenseContext;
using QColors = QuestPDF.Helpers.Colors;
using QDocument = QuestPDF.Fluent.Document;
using QContainer = QuestPDF.Infrastructure.IContainer;

using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarRental.BLL.Services
{
    public class ExportService
    {
        public ExportService()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Master");
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ==================== EXCEL: УНИВЕРСАЛЬНЫЙ ====================

        public void ExportToExcel<T>(IEnumerable<T> data, string filePath, string title, Dictionary<string, string> headerMap)
        {
            if (data == null) return;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Отчет");

            // 1. Заголовок
            ws.Cells["A1"].Value = title;
            ws.Cells["A1:E1"].Merge = true;
            ws.Cells["A1"].Style.Font.Size = 16;
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // 2. Данные
            var range = ws.Cells["A3"].LoadFromCollection(data, true);

            // 3. Стилизация и Русские заголовки
            if (range != null)
            {
                var headerRow = ws.Cells[3, 1, 3, range.Columns];
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                headerRow.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // Переименовываем колонки
                for (int col = 1; col <= range.Columns; col++)
                {
                    string oldHeader = ws.Cells[3, col].Text;
                    if (headerMap.ContainsKey(oldHeader))
                    {
                        ws.Cells[3, col].Value = headerMap[oldHeader];
                    }
                }

                // Границы для всей таблицы
                var fullRange = ws.Cells[3, 1, range.End.Row, range.Columns];
                fullRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                fullRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                fullRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                fullRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            ws.Cells.AutoFitColumns();
            File.WriteAllBytes(filePath, package.GetAsByteArray());
        }

        // ==================== EXCEL: ФИНАНСОВЫЙ (С ГРУППИРОВКОЙ) ====================

        public void ExportFinanceToExcel(List<PaymentReportItem> data, string filePath, string title)
        {
            if (data == null) return;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Финансы");

            // Заголовок
            ws.Cells["A1"].Value = title;
            ws.Cells["A1:D1"].Merge = true;
            ws.Cells["A1"].Style.Font.Size = 16;
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Шапка таблицы
            ws.Cells[3, 1].Value = "Дата";
            ws.Cells[3, 2].Value = "Автомобиль";
            ws.Cells[3, 3].Value = "Тип";
            ws.Cells[3, 4].Value = "Сумма (BYN)";
            ws.Cells[3, 1, 3, 4].Style.Font.Bold = true;
            ws.Cells[3, 1, 3, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[3, 1, 3, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

            int row = 4;
            decimal grandTotal = 0;

            // Группировка по месяцам
            var groups = data.GroupBy(x => new { x.Date.Year, x.Date.Month });

            foreach (var group in groups)
            {
                decimal monthTotal = 0;
                string monthName = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM yyyy");

                // Данные месяца
                foreach (var item in group)
                {
                    ws.Cells[row, 1].Value = item.Date.ToString("dd.MM.yyyy");
                    ws.Cells[row, 2].Value = item.CarInfo;
                    ws.Cells[row, 3].Value = item.Type;
                    ws.Cells[row, 4].Value = item.Amount;

                    monthTotal += item.Amount;
                    row++;
                }

                // Подвал месяца (Подшивка)
                ws.Cells[row, 1].Value = $"ИТОГО за {monthName}:";
                ws.Cells[row, 1, row, 3].Merge = true;
                ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[row, 1].Style.Font.Bold = true;

                ws.Cells[row, 4].Value = monthTotal;
                ws.Cells[row, 4].Style.Font.Bold = true;
                ws.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 255, 240)); // Светло-зеленый

                grandTotal += monthTotal;
                row++;
                row++; // Пустая строка между месяцами
            }

            // ОБЩИЙ ИТОГ
            ws.Cells[row, 1].Value = "ОБЩАЯ ПРИБЫЛЬ ЗА ПЕРИОД:";
            ws.Cells[row, 1, row, 3].Merge = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 12;

            ws.Cells[row, 4].Value = grandTotal;
            ws.Cells[row, 4].Style.Font.Bold = true;
            ws.Cells[row, 4].Style.Font.Size = 12;
            ws.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

            // Границы
            var fullRange = ws.Cells[3, 1, row, 4];
            fullRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            fullRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            fullRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            fullRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            ws.Cells.AutoFitColumns();
            File.WriteAllBytes(filePath, package.GetAsByteArray());
        }

        // ==================== PDF: УНИВЕРСАЛЬНЫЙ ====================

        public void ExportToPdf<T>(IEnumerable<T> data, string filePath, string title, string[] headers, Func<T, string[]> mapper)
        {
            QDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1.5f, Unit.Centimetre);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.SegoeUI));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(title).FontSize(20).SemiBold().FontColor("#2196F3");
                            col.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).FontColor("#9E9E9E");
                        });
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols => { foreach (var h in headers) cols.RelativeColumn(); });

                        table.Header(h => {
                            foreach (var head in headers) h.Cell().Element(HeaderStyle).Text(head);
                        });

                        foreach (var item in data)
                        {
                            var vals = mapper(item);
                            foreach (var v in vals) table.Cell().Element(CellStyle).Text(v);
                        }
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Стр. "); x.CurrentPageNumber(); });
                });
            }).GeneratePdf(filePath);
        }

        // ==================== PDF: ФИНАНСОВЫЙ (С ГРУППИРОВКОЙ) ====================

        public void ExportFinanceToPdf(List<PaymentReportItem> data, string filePath, string title)
        {
            decimal grandTotal = 0;

            QDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1.5f, Unit.Centimetre);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.SegoeUI));

                    page.Header().Text(title).FontSize(20).SemiBold().FontColor("#2196F3");

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        var groups = data.GroupBy(x => new { x.Date.Year, x.Date.Month });

                        foreach (var group in groups)
                        {
                            string monthName = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM yyyy");
                            decimal monthTotal = group.Sum(x => x.Amount);
                            grandTotal += monthTotal;

                            // Заголовок месяца
                            col.Item().PaddingTop(15).Text(monthName).FontSize(14).Bold().FontColor("#4CAF50");

                            // Таблица месяца
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(80); // Дата
                                    c.RelativeColumn();   // Авто
                                    c.ConstantColumn(100);// Тип
                                    c.ConstantColumn(100);// Сумма
                                });

                                // Шапка
                                table.Header(h =>
                                {
                                    h.Cell().Element(HeaderStyle).Text("Дата");
                                    h.Cell().Element(HeaderStyle).Text("Автомобиль");
                                    h.Cell().Element(HeaderStyle).Text("Тип");
                                    h.Cell().Element(HeaderStyle).Text("Сумма");
                                });

                                // Строки
                                foreach (var item in group)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Date.ToString("dd.MM.yyyy"));
                                    table.Cell().Element(CellStyle).Text(item.CarInfo);
                                    table.Cell().Element(CellStyle).Text(item.Type);
                                    table.Cell().Element(CellStyle).Text($"{item.Amount:N2}");
                                }

                                // Итог месяца (внутри таблицы или сразу под ней)
                                table.Footer(f =>
                                {
                                    f.Cell().ColumnSpan(3).Element(FooterStyle).AlignRight().Text("Итого за месяц:").Bold();
                                    f.Cell().Element(FooterStyle).Text($"{monthTotal:N2} BYN").Bold();
                                });
                            });
                        }

                        // ОБЩИЙ ИТОГ
                        col.Item().PaddingTop(20).Background("#E8F5E9").Padding(10).Row(r =>
                        {
                            r.RelativeItem().AlignRight().Text("ОБЩАЯ ПРИБЫЛЬ ЗА ВЕСЬ ПЕРИОД:  ").FontSize(14).Bold();
                            r.ConstantItem(150).Text($"{grandTotal:N2} BYN").FontSize(14).Bold().FontColor("#2E7D32");
                        });
                    });

                    page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); });
                });
            }).GeneratePdf(filePath);
        }

        // Стили
        static QContainer CellStyle(QContainer c) => c.BorderBottom(1).BorderColor("#E0E0E0").Padding(5);
        static QContainer HeaderStyle(QContainer c) => c.Background("#F5F5F5").BorderBottom(1).BorderColor("#BDBDBD").Padding(5).PaddingVertical(8).DefaultTextStyle(x => x.SemiBold());
        static QContainer FooterStyle(QContainer c) => c.BorderTop(1).BorderColor("#000000").Padding(5).Background("#FAFAFA");
    }
}