using ClosedXML.Excel;
using HRManagement.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HRManagement.Services.Payroll
{
    public class PayrollExportService
    {
        public byte[] ExportPayrollExcel(List<PayrollRecord> records, PayrollPeriod period)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add($"Luong_{period.Month:D2}_{period.Year}");

            // 1. Tiêu đề chính
            ws.Cell(1, 1).Value = $"BẢNG LƯƠNG NHÂN VIÊN THÁNG {period.Month}/{period.Year}";
            ws.Range(1, 1, 1, 15).Merge().Style
                .Font.SetBold(true)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Font.SetFontSize(16)
                .Font.SetFontColor(XLColor.FromHtml("#1E40AF"));

            // 2. Header bảng
            var headers = new[]
            {
                "STT", "Mã NV", "Họ tên", "Phòng ban", "Chức vụ",
                "Lương cơ bản", "Ngày công TT", "Lương ngày công", "Phụ cấp", "Lương OT",
                "Thưởng", "Gross Pay", "Bảo hiểm", "Thuế TNCN", "Net Pay"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(3, i + 1);
                cell.Value = headers[i];
                cell.Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#3B82F6"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }

            // 3. Dữ liệu
            int startRow = 4;
            for (int i = 0; i < records.Count; i++)
            {
                var r = records[i];
                int currentRow = startRow + i;

                ws.Cell(currentRow, 1).Value = i + 1;
                ws.Cell(currentRow, 2).Value = r.Employee.EmployeeCode;
                ws.Cell(currentRow, 3).Value = r.Employee.FullName;
                ws.Cell(currentRow, 4).Value = r.Employee.Department?.DepartmentName;
                ws.Cell(currentRow, 5).Value = r.Employee.Position?.PositionName;
                
                ws.Cell(currentRow, 6).Value = (double)r.BaseSalary;
                ws.Cell(currentRow, 7).Value = (double)r.ActualWorkingDays;
                
                var salariedAmount = r.WorkingDays > 0 ? (r.BaseSalary / r.WorkingDays * r.ActualWorkingDays) : 0m;
                ws.Cell(currentRow, 8).Value = (double)salariedAmount;
                
                ws.Cell(currentRow, 9).Value = (double)(r.TotalAllowances - r.OvertimePay); // Phụ cấp không tính OT
                ws.Cell(currentRow, 10).Value = (double)r.OvertimePay;
                ws.Cell(currentRow, 11).Value = (double)r.BonusAmount;
                ws.Cell(currentRow, 12).Value = (double)(r.GrossPay ?? 0m);
                ws.Cell(currentRow, 13).Value = (double)r.InsuranceAmount;
                ws.Cell(currentRow, 14).Value = (double)r.TaxAmount;
                ws.Cell(currentRow, 15).Value = (double)(r.NetPay ?? 0m);

                // Format số (tiền tệ và ngày công)
                ws.Range(currentRow, 6, currentRow, 6).Style.NumberFormat.Format = "#,##0";
                ws.Range(currentRow, 8, currentRow, 15).Style.NumberFormat.Format = "#,##0";
                
                // Border
                ws.Range(currentRow, 1, currentRow, 15).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }

            // 4. Tổng cộng
            int footerRow = startRow + records.Count;
            ws.Cell(footerRow, 3).Value = "TỔNG CỘNG";
            ws.Cell(footerRow, 3).Style.Font.SetBold(true).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            
            // Formula Sum cho các cột tiền
            var columnsToSum = new[] { 8, 9, 10, 11, 12, 13, 14, 15 };
            foreach (var colIdx in columnsToSum) {
                var colLetter = GetColumnLetter(colIdx);
                ws.Cell(footerRow, colIdx).FormulaA1 = $"=SUM({colLetter}{startRow}:{colLetter}{footerRow - 1})";
                ws.Cell(footerRow, colIdx).Style.Font.SetBold(true).NumberFormat.Format = "#,##0";
            }
            ws.Range(footerRow, 1, footerRow, 15).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            // 5. Thẩm mỹ
            ws.Columns().AdjustToContents();
            ws.Column(3).Width = 25; // Họ tên
            ws.Rows().Height = 20;
            ws.Row(1).Height = 35;
            ws.Row(3).Height = 25;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private string GetColumnLetter(int colIndex)
        {
            int div = colIndex;
            string label = "";
            while (div > 0)
            {
                int mod = (div - 1) % 26;
                label = (char)(65 + mod) + label;
                div = (div - mod) / 26;
            }
            return label;
        }
    }
}
