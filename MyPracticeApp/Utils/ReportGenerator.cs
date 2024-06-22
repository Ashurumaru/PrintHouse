using System;
using System.Collections.Generic;
using System.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;
using MyPracticeApp.Data;
using MyPracticeApp.Models;
using System.Diagnostics;
using System.Windows;

namespace MyPracticeApp.Utils
{
    public static class ReportGenerator
    {
        public static void GenerateRevenueReport(DateTime startDate, DateTime endDate)
        {
            string filePath = $"RevenueReport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.docx";

            using (var doc = DocX.Create(filePath))
            {
                var context = new PrintingHouseEntities();

                var purchases = context.PurchaseOrders
                    .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate)
                    .Select(po => new
                    {
                        po.OrderDate,
                        po.PurchaseOrderDetails
                    }).ToList();

                var sales = context.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .Select(o => new
                    {
                        o.OrderDate,
                        o.OrderDetails
                    }).ToList();

                var title = doc.InsertParagraph("ООО \"Байкальский Меридиан\"")
                    .FontSize(14)
                    .Bold()
                    .Alignment = Alignment.center;
                doc.InsertParagraph("Отчет по выручке и доходу")
                    .FontSize(12)
                    .Alignment = Alignment.center;
                doc.InsertParagraph($"Период: с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}")
                    .FontSize(12)
                    .Alignment = Alignment.center;
                doc.InsertParagraph("\n");

                int purchaseRows = purchases.Sum(p => p.PurchaseOrderDetails.Count);
                int salesRows = sales.Sum(s => s.OrderDetails.Count);
                int totalRows = purchaseRows + salesRows + 4;

                var table = doc.AddTable(totalRows, 6);
                table.Design = TableDesign.TableGrid;

                table.Rows[0].Cells[0].Paragraphs.First().Append("Дата").Bold();
                table.Rows[0].Cells[1].Paragraphs.First().Append("Тип").Bold();
                table.Rows[0].Cells[2].Paragraphs.First().Append("Наименование").Bold();
                table.Rows[0].Cells[3].Paragraphs.First().Append("Количество").Bold();
                table.Rows[0].Cells[4].Paragraphs.First().Append("Цена за единицу").Bold();
                table.Rows[0].Cells[5].Paragraphs.First().Append("Сумма").Bold();

                int rowIndex = 1;
                double totalPurchaseAmount = 0;
                double totalSalesAmount = 0;

                foreach (var purchase in purchases)
                {
                    foreach (var detail in purchase.PurchaseOrderDetails)
                    {
                        table.Rows[rowIndex].Cells[0].Paragraphs.First().Append(purchase.OrderDate.ToString("dd.MM.yyyy"));
                        table.Rows[rowIndex].Cells[1].Paragraphs.First().Append("Покупка");
                        table.Rows[rowIndex].Cells[2].Paragraphs.First().Append(detail.Products.ProductName);
                        table.Rows[rowIndex].Cells[3].Paragraphs.First().Append(detail.Quantity.ToString());
                        table.Rows[rowIndex].Cells[4].Paragraphs.First().Append(((double)detail.Price).ToString("F2"));
                        table.Rows[rowIndex].Cells[5].Paragraphs.First().Append((detail.Quantity * (double)detail.Price).ToString());

                        totalPurchaseAmount += (int)detail.Quantity * (double)detail.Price;
                        rowIndex++;

                    }
                }

                foreach (var sale in sales)
                {
                    foreach (var detail in sale.OrderDetails)
                    {

                        table.Rows[rowIndex].Cells[0].Paragraphs.First().Append(sale.OrderDate.ToString("dd.MM.yyyy"));
                        table.Rows[rowIndex].Cells[1].Paragraphs.First().Append("Продажа");
                        table.Rows[rowIndex].Cells[2].Paragraphs.First().Append(detail.Products.ProductName);
                        table.Rows[rowIndex].Cells[3].Paragraphs.First().Append(detail.Quantity.ToString());
                        table.Rows[rowIndex].Cells[4].Paragraphs.First().Append(((double)detail.Price).ToString("F2"));
                        table.Rows[rowIndex].Cells[5].Paragraphs.First().Append((detail.Quantity * (double)detail.Price).ToString());

                        totalSalesAmount += (int)detail.Quantity * (double)detail.Price;
                        rowIndex++;

                    }
                }


                table.Rows[rowIndex].Cells[0].Paragraphs.First().Append("Итого Покупки").Bold();
                table.Rows[rowIndex].Cells[5].Paragraphs.First().Append(totalPurchaseAmount.ToString("F2")).Bold();
                rowIndex++;

                table.Rows[rowIndex].Cells[0].Paragraphs.First().Append("Итого Продажи").Bold();
                table.Rows[rowIndex].Cells[5].Paragraphs.First().Append(totalSalesAmount.ToString("F2")).Bold();
                rowIndex++;

                table.Rows[rowIndex].Cells[0].Paragraphs.First().Append("Общий доход").Bold();
                table.Rows[rowIndex].Cells[5].Paragraphs.First().Append((totalSalesAmount - totalPurchaseAmount).ToString("F2")).Bold();

                doc.InsertTable(table);

                doc.InsertParagraph($"Общая сумма покупок: {totalPurchaseAmount:F2} руб.")
                    .FontSize(12)
                    .SpacingAfter(10)
                    .Bold();
                doc.InsertParagraph($"Общая сумма продаж: {totalSalesAmount:F2} руб.")
                    .FontSize(12)
                    .SpacingAfter(10)
                    .Bold();
                doc.InsertParagraph($"Общий итог: {(totalSalesAmount - totalPurchaseAmount):F2} руб.")
                    .FontSize(12)
                    .SpacingAfter(10)
                    .Bold();

                var transactions = purchases.SelectMany(p => p.PurchaseOrderDetails.Select(d => new Transaction
                {
                    OrderDate = p.OrderDate,
                    OrderType = "Покупка",
                    Amount = (int)d.Quantity * (double)d.Price
                })).Concat(sales.SelectMany(s => s.OrderDetails.Select(d => new Transaction
                {
                    OrderDate = s.OrderDate,
                    OrderType = "Продажа",
                    Amount = (int)d.Quantity * (double)d.Price
                }))).ToList();

                var monthlySummary = transactions
                    .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalAmount = g.Sum(x => x.Amount)
                    }).ToList();

                doc.InsertParagraph("Ежемесячные объемы:")
                    .FontSize(12)
                    .SpacingAfter(10)
                    .Bold();

                foreach (var monthSummary in monthlySummary)
                {
                    doc.InsertParagraph($"{monthSummary.Year}-{monthSummary.Month.ToString("D2")}: {monthSummary.TotalAmount:F2} руб.")
                        .FontSize(12)
                        .SpacingAfter(10);
                }

                doc.Save();
            }

            Process.Start("explorer.exe", filePath);
        }





        public static void GenerateEmployeePerformanceReport(DateTime startDate, DateTime endDate)
        {
            string filePath = $"EmployeePerformanceReport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.docx";

            using (var doc = DocX.Create(filePath))
            {
                var context = new PrintingHouseEntities();

                var employees = context.Employees
                    .Include("Orders")
                    .Where(e => e.Orders.Any(o => o.OrderDate >= startDate && o.OrderDate <= endDate))
                    .ToList();

                var title = doc.InsertParagraph("ООО \"Байкальский Меридиан\"")
                    .FontSize(14)
                    .Bold()
                    .Alignment = Alignment.center;
                doc.InsertParagraph("Отчет по производительности сотрудников")
                    .FontSize(12)
                    .Alignment = Alignment.center;
                doc.InsertParagraph($"Период: с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}")
                    .FontSize(12)
                    .Alignment = Alignment.center;
                doc.InsertParagraph("\n");

                var table = doc.AddTable(employees.Count + 2, 4);
                table.Design = TableDesign.TableGrid;

                table.Rows[0].Cells[0].Paragraphs.First().Append("№").Bold();
                table.Rows[0].Cells[1].Paragraphs.First().Append("Сотрудник").Bold();
                table.Rows[0].Cells[2].Paragraphs.First().Append("Кол-во заказов").Bold();
                table.Rows[0].Cells[3].Paragraphs.First().Append("Общая сумма").Bold();

                double totalAmount = 0;
                int totalOrders = 0;

                for (int i = 0; i < employees.Count; i++)
                {
                    var employee = employees[i];
                    var employeeOrders = employee.Orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToList();
                    var employeeTotal = employeeOrders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * (od.Price ?? 0)));

                    table.Rows[i + 1].Cells[0].Paragraphs.First().Append((i + 1).ToString());
                    table.Rows[i + 1].Cells[1].Paragraphs.First().Append($"{employee.FirstName} {employee.LastName}");
                    table.Rows[i + 1].Cells[2].Paragraphs.First().Append(employeeOrders.Count.ToString());
                    table.Rows[i + 1].Cells[3].Paragraphs.First().Append(employeeTotal.ToString());

                    totalOrders += employeeOrders.Count;
                    totalAmount += (int)employeeTotal;
                }

                table.Rows[employees.Count + 1].Cells[0].Paragraphs.First().Append("Итого").Bold();
                table.Rows[employees.Count + 1].Cells[2].Paragraphs.First().Append(totalOrders.ToString()).Bold();
                table.Rows[employees.Count + 1].Cells[3].Paragraphs.First().Append(totalAmount.ToString("F2")).Bold();

                doc.InsertTable(table);

                doc.Save();
            }

            Process.Start("explorer.exe", filePath);
        }
    }
}
