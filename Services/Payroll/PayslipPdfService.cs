using HRManagement.DTOs.SystemSettings;
using HRManagement.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace HRManagement.Services.Payroll
{
    public class PayslipPdfService
    {
        public PayslipPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GeneratePdf(PayrollRecord record, PayrollPeriod period, CompanySettingsDto? company = null)
        {
            var address = company?.Address ?? "";

            // ── Calculations ──────────────────────────────────────────────────
            var salariedAmount = record.WorkingDays > 0
                ? Math.Round(record.BaseSalary / record.WorkingDays * record.ActualWorkingDays, 0)
                : 0m;
            var grossPay = salariedAmount + record.TotalAllowances + record.OvertimePay + record.BonusAmount;
            var manualDeductions = record.PayrollDeductions
                .Where(d => d.DeductionType != "Insurance" && d.DeductionType != "Tax")
                .Sum(d => d.Amount);
            var totalDeductions = record.InsuranceAmount + record.TaxAmount + manualDeductions;
            var netPay = grossPay - totalDeductions;

            var monthName   = new DateTime(2000, period.Month, 1).ToString("MMMM", CultureInfo.GetCultureInfo("en-US"));
            var periodLabel = $"{monthName}-{period.Year}";
            var dateFrom    = period.StartDate.ToString("MM/dd/yyyy");
            var dateTo      = period.EndDate.ToString("MM/dd/yyyy");

            var allowances      = record.PayrollAllowances.Where(a => a.AllowanceType != "Overtime").ToList();
            var otherDeductions = record.PayrollDeductions
                .Where(d => d.DeductionType != "Insurance" && d.DeductionType != "Tax").ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        // ── 1. Title ──────────────────────────────────────────
                        col.Item().Text("Pay Slip").Bold().FontSize(22);
                        col.Item().PaddingBottom(10)
                            .Text($"Salary Slip of {record.Employee.FullName} for {periodLabel}")
                            .FontSize(10);

                        // ── 2. Employee info table ────────────────────────────
                        col.Item().Border(1).BorderColor(Colors.Grey.Medium).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(1.8f);
                                cols.RelativeColumn(3.2f);
                                cols.RelativeColumn(2f);
                                cols.RelativeColumn(3f);
                            });

                            InfoRow(table, "Name",      record.Employee.FullName,    "Designation", record.Employee.Position?.PositionName ?? "");
                            InfoRow(table, "Address",   address,                     "",            "");
                            InfoRow(table, "Email",     record.Employee.Email,       "Identification No", "");
                            InfoRow(table, "Reference", $"SLIP/{record.PayrollRecordId:D3}", "Bank Account", "");

                            // Last row — no bottom border
                            table.Cell().Padding(5).Text("Date From").Bold();
                            table.Cell().BorderRight(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(dateFrom);
                            table.Cell().Padding(5).Text("Date To").Bold();
                            table.Cell().Padding(5).Text(dateTo);
                        });

                        col.Item().PaddingTop(18);

                        // ── 3. Details by Salary Rule Category ───────────────
                        col.Item().PaddingBottom(4).Text("Details by Salary Rule Category").Bold().FontSize(13);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(65);
                                cols.RelativeColumn();
                                cols.ConstantColumn(140);
                            });

                            table.Header(h =>
                            {
                                h.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5).Text("Code").Bold();
                                h.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5).Text("Salary Rule Category").Bold();
                                h.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignRight().Text("Total").Bold();
                            });

                            SalaryRuleRow(table, "BASIC", "Basic",          salariedAmount);
                            SalaryRuleRow(table, "BASIC", "..Basic Salary", salariedAmount);
                            SalaryRuleRow(table, "GROSS", "Gross",          grossPay);
                            SalaryRuleRow(table, "GROSS", "..Gross",        grossPay);
                            SalaryRuleRow(table, "NET",   "Net",            netPay);
                            SalaryRuleRow(table, "NET",   "..Net Salary",   netPay);
                        });

                        col.Item().PaddingTop(18);

                        // ── 4. Payslip Lines by Contribution Register ─────────
                        col.Item().PaddingBottom(4).Text("Payslip Lines by Contribution Register").Bold().FontSize(13);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(65);
                                cols.RelativeColumn();
                                cols.ConstantColumn(90);
                                cols.ConstantColumn(120);
                                cols.ConstantColumn(120);
                            });

                            table.Header(h =>
                            {
                                h.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5).Text("Code").Bold();
                                h.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5).Text("Name").Bold();
                                h.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5).Text("Quantity/rate").Bold();
                                h.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignRight().Text("Amount").Bold();
                                h.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignRight().Text("Total").Bold();
                            });

                            // Basic salary
                            ContribRow(table, "BASIC", $"Basic Salary ({record.ActualWorkingDays}/{record.WorkingDays} ngày)",
                                "1.0", Fmt(salariedAmount), Fmt(salariedAmount));

                            // Allowances
                            foreach (var a in allowances)
                                ContribRow(table, "ALLOW", a.AllowanceName, "1.0", Fmt(a.Amount), Fmt(a.Amount));

                            // Overtime
                            if (record.OvertimePay > 0)
                                ContribRow(table, "OT", "Overtime Pay", "1.0", Fmt(record.OvertimePay), Fmt(record.OvertimePay));

                            // Bonus
                            if (record.BonusAmount > 0)
                                ContribRow(table, "BONUS", "Bonus", "1.0", Fmt(record.BonusAmount), Fmt(record.BonusAmount));

                            // Gross subtotal separator
                            ContribRow(table, "", "", "0", "", Fmt(grossPay));

                            // Insurance
                            if (record.InsuranceAmount > 0)
                                ContribRow(table, "INS", "Insurance (BHXH/BHYT/BHTN)", "1.0",
                                    Fmt(record.InsuranceAmount), Fmt(record.InsuranceAmount));

                            // Tax
                            if (record.TaxAmount > 0)
                                ContribRow(table, "TAX", "Personal Income Tax", "1.0",
                                    Fmt(record.TaxAmount), Fmt(record.TaxAmount));

                            // Other deductions
                            foreach (var d in otherDeductions)
                                ContribRow(table, "DED", d.DeductionName, "1.0", Fmt(d.Amount), Fmt(d.Amount));

                            // Net Salary
                            ContribRow(table, "NET", "Net Salary", "1.0",
                                netPay.ToString("N0"), Fmt(netPay));
                        });

                        // ── 5. Authorized signature ───────────────────────────
                        col.Item().PaddingTop(20).AlignRight().Text("Authorized signature").Bold();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void InfoRow(TableDescriptor table,
            string label1, string value1, string label2, string value2)
        {
            var borderColor = Colors.Grey.Lighten1;
            table.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text(label1).Bold();
            table.Cell().BorderBottom(1).BorderRight(1).BorderColor(borderColor).Padding(5).Text(value1);
            table.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text(label2).Bold();
            table.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text(value2);
        }

        private static void SalaryRuleRow(TableDescriptor table, string code, string name, decimal amount)
        {
            var border = Colors.Grey.Lighten2;
            table.Cell().BorderBottom(1).BorderColor(border).Padding(5).Text(code);
            table.Cell().BorderBottom(1).BorderColor(border).Padding(5).Text(name);
            table.Cell().BorderBottom(1).BorderColor(border).Padding(5).AlignRight().Text(Fmt(amount));
        }

        private static void ContribRow(TableDescriptor table,
            string code, string name, string qty, string amount, string total)
        {
            var border = Colors.Grey.Lighten2;
            table.Cell().BorderBottom(1).BorderColor(border).Padding(5).Text(code);
            table.Cell().BorderBottom(1).BorderColor(border).Padding(5).Text(name);
            table.Cell().BorderBottom(1).BorderColor(border).Padding(5).AlignRight().Text(qty);
            table.Cell().BorderBottom(1).BorderColor(border).Padding(5).AlignRight().Text(amount);
            table.Cell().BorderBottom(1).BorderColor(border).Padding(5).AlignRight().Text(total);
        }

        private static string Fmt(decimal amount)
            => amount.ToString("N2", new CultureInfo("vi-VN")) + " đ";
    }
}
