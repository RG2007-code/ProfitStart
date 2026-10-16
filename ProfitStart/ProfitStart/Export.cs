using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace ProfitStart
{
    public static class Export
    {
       
        public static bool Items<T>(string defaultFileName,
                                    string sheetName,
                                    List<string> headers,
                                    IEnumerable<T> items,
                                    Func<T, List<object>> projector,
                                    Window owner)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Сохранить как Excel",
                Filter = "Excel-файлы (*.xlsx)|*.xlsx",
                FileName = defaultFileName,
                DefaultExt = ".xlsx"
            };

            if (dlg.ShowDialog() != true) return false;

            try
            {
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add(
                        string.IsNullOrEmpty(sheetName) ? "Данные" : sheetName);

                    
                    for (int c = 0; c < headers.Count; c++)
                    {
                        var cell = ws.Cell(1, c + 1);
                        cell.Value = headers[c];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#5B5BF5");
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    
                    int row = 2;
                    foreach (var item in items)
                    {
                        var values = projector(item);

                        for (int c = 0; c < values.Count; c++)
                        {
                            var v = values[c];
                            var cell = ws.Cell(row, c + 1);

                            if (v == null)
                            {
                                cell.Value = "";
                            }
                            else if (v is int i)
                            {
                                cell.Value = i;
                            }
                            else if (v is long l)
                            {
                                cell.Value = l;
                            }
                            else if (v is double d)
                            {
                                cell.Value = d;
                            }
                            else if (v is decimal m)
                            {
                                cell.Value = (double)m;
                            }
                            else if (v is DateTime dt)
                            {
                                cell.Value = dt;
                                cell.Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
                            }
                            else if (v is bool b)
                            {
                                cell.Value = b ? "Да" : "Нет";
                            }
                            else
                            {
                                cell.Value = v.ToString();
                            }
                        }

                        row++;
                    }

                    
                    ws.Columns().AdjustToContents();

                   
                    ws.Range(1, 1, row - 1, headers.Count).SetAutoFilter();

                    
                    wb.SaveAs(dlg.FileName);
                }

                AppMessage.Success(owner, "Файл сохранён:\n" + dlg.FileName);
                return true;
            }
            catch (Exception ex)
            {
                AppMessage.Error(owner, "Ошибка экспорта: " + ex.Message);
                return false;
            }
        }
    }
}