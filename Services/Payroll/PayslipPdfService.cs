using HRManagement.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Globalization;

namespace HRManagement.Services.Payroll
{
    public class PayslipPdfService
    {
        public PayslipPdfService()
        {
            // QuestPDF requires setting the license. 
            // For community projects, it is free.
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GeneratePdf(PayrollRecord record, PayrollPeriod period)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        // 1. Header: Công ty
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("CÔNG TY CỔ PHẦN HR SYSTEM").Bold().FontSize(15).FontColor(Colors.Blue.Medium);
                                c.Item().Text("Địa chỉ: Toà nhà Innovation, Quận 12, TP. Hồ Chí Minh");
                                c.Item().Text("Điện thoại: (028) 1234 5678 | Email: hr@hrsystem.com");
                            });
                        });

                        col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // 2. Tiêu đề phiếu lương
                        col.Item().AlignCenter().PaddingVertical(10).Column(c =>
                        {
                            c.Item().AlignCenter().Text($"PHIẾU LƯƠNG THÁNG {period.Month}/{period.Year}").Bold().FontSize(16);
                            c.Item().AlignCenter().Text($"Mã số phiếu: PS-{period.Year}{period.Month:D2}-{record.PayrollRecordId:D5}").FontSize(9).Italic();
                        });

                        // 3. Thông tin nhân viên
                        col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t => { t.Span("Họ và tên: ").Bold(); t.Span(record.Employee.FullName); });
                                c.Item().Text(t => { t.Span("Phòng ban: ").Bold(); t.Span(record.Employee.Department?.DepartmentName ?? "N/A"); });
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t => { t.Span("Mã nhân viên: ").Bold(); t.Span(record.Employee.EmployeeCode); });
                                c.Item().Text(t => { t.Span("Chức vụ: ").Bold(); t.Span(record.Employee.Position?.PositionName ?? "N/A"); });
                            });
                        });

                        col.Item().PaddingVertical(15);

                        // 4. Bảng chi tiết thu nhập & khấu trừ
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3); // Diễn giải
                                cols.RelativeColumn(2); // Số tiền thu nhập
                                cols.RelativeColumn(3); // Diễn giải khấu trừ
                                cols.RelativeColumn(2); // Số tiền khấu trừ
                            });

                            // Header Bảng
                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(5).Text("THU NHẬP (A)").Bold();
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text("SỐ TIỀN").Bold();
                                h.Cell().Background(Colors.Red.Lighten4).Padding(5).Text("KHẤU TRỪ (B)").Bold();
                                h.Cell().Background(Colors.Red.Lighten4).Padding(5).AlignRight().Text("SỐ TIỀN").Bold();
                            });

                            // Nội dung 1: Lương cơ bản & Bảo hiểm
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Lương cơ bản");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(FormatCurrency(record.BaseSalary));
                            
                            var insurance = record.PayrollDeductions.FirstOrDefault(d => d.DeductionType == "Insurance");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Bảo hiểm (10.5%)");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(FormatCurrency(insurance?.Amount ?? 0));

                            // Nội dung 2: Lương ngày công & Thuế
                            var salariedText = $"Lương ngày công ({record.ActualWorkingDays}/{record.WorkingDays})";
                            var salariedAmount = record.WorkingDays > 0 ? (record.BaseSalary / record.WorkingDays * record.ActualWorkingDays) : 0;
                            
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(salariedText);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(FormatCurrency(salariedAmount));

                            var tax = record.PayrollDeductions.FirstOrDefault(d => d.DeductionType == "Tax");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Thuế TNCN");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(FormatCurrency(tax?.Amount ?? 0));

                            // Nội dung 3: Phụ cấp & Khấu trừ khác
                            var allowances = record.PayrollAllowances.Where(a => a.AllowanceType != "Overtime").ToList();
                            var deductions = record.PayrollDeductions.Where(d => d.DeductionType != "Insurance" && d.DeductionType != "Tax").ToList();
                            
                            int maxRows = Math.Max(allowances.Count, deductions.Count);
                            if (record.OvertimePay > 0) maxRows++;
                            if (record.BonusAmount > 0) maxRows++;

                            // Merge OT và Bonus vào hàng thu nhập
                            int currentRow = 0;
                            if (record.OvertimePay > 0) {
                                table.Cell().Padding(5).Text("Lương làm thêm giờ");
                                table.Cell().Padding(5).AlignRight().Text(FormatCurrency(record.OvertimePay));
                                table.Cell().Padding(5).Text(""); table.Cell().Padding(5).Text("");
                                currentRow++;
                            }
                            if (record.BonusAmount > 0) {
                                table.Cell().Padding(5).Text("Thưởng / Bonus");
                                table.Cell().Padding(5).AlignRight().Text(FormatCurrency(record.BonusAmount));
                                table.Cell().Padding(5).Text(""); table.Cell().Padding(5).Text("");
                                currentRow++;
                            }

                            for (int i = 0; i < Math.Max(allowances.Count, deductions.Count); i++)
                            {
                                var a = i < allowances.Count ? allowances[i] : null;
                                var d = i < deductions.Count ? deductions[i] : null;

                                table.Cell().Padding(5).Text(a?.AllowanceName ?? "");
                                table.Cell().Padding(5).AlignRight().Text(a != null ? FormatCurrency(a.Amount) : "");
                                table.Cell().Padding(5).Text(d?.DeductionName ?? "");
                                table.Cell().Padding(5).AlignRight().Text(d != null ? FormatCurrency(d.Amount) : "");
                            }

                            // Footer Bảng: Tổng
                            table.Footer(f =>
                            {
                                f.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("TỔNG THU NHẬP").Bold();
                                f.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(FormatCurrency(record.GrossPay)).Bold();
                                f.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("TỔNG KHẤU TRỪ").Bold();
                                f.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(FormatCurrency(record.TotalDeductions)).Bold();
                            });
                        });

                        col.Item().PaddingVertical(15);

                        // 5. Thực lĩnh (Net Pay)
                        col.Item().Background(Colors.Green.Lighten5).Padding(10).Border(1).BorderColor(Colors.Green.Medium).Row(row =>
                        {
                            row.RelativeItem().Text("THỰC LĨNH (NET PAY)").Bold().FontSize(13).FontColor(Colors.Green.Medium);
                            row.RelativeItem().AlignRight().Text(FormatCurrency(record.NetPay)).Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                        });

                        col.Item().PaddingTop(5).Text($"Bằng chữ: {NumberToVietnameseText((long)record.NetPay)}").Italic().FontSize(9);

                        // 6. Chữ ký
                        col.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Nhân viên xác nhận").Bold();
                                c.Item().PaddingTop(40).Text("(Ký và ghi rõ họ tên)");
                            });
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Phòng Nhân sự").Bold();
                                c.Item().PaddingTop(40).Text("(Ký và đóng dấu)");
                            });
                        });
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

        private string FormatCurrency(decimal amount)
        {
            return amount.ToString("N0", new CultureInfo("vi-VN")) + " đ";
        }

        private string NumberToVietnameseText(long number)
        {
            if (number == 0) return "Không đồng";
            if (number < 0) return "Âm " + NumberToVietnameseText(Math.Abs(number));

            string[] units = { "", "mươi", "trăm", "nghìn", "mươi", "trăm", "triệu", "mươi", "trăm", "tỷ" };
            // Simple implementation for demo
            return $"{number:N0} đồng chẵn."; 
        }
    }
}
