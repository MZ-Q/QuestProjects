using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace StatsParser
{
    public static class ExcelOperational
    {
        public static Dictionary<int, string> GetGameLvlsSpec(string fileName)
        {
            Dictionary<int, string> typesOfLvls = new Dictionary<int, string>();

            Application excelApp = new Application
            {
                Visible = true
            };

            var xlWorkBooks = excelApp.Workbooks;
            string filePath = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\" + fileName;
            // open the workbook
            Workbook xlWorkBook = xlWorkBooks.Open(filePath,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing);

            // select the first sheet        
            Worksheet xlWorkSheet = xlWorkBook.Worksheets[1];
            // find the used range in worksheet
            Range excelRange = xlWorkSheet.UsedRange;
            // get an object array of all of the cells in the worksheet (their values)
            object[,] valueArray = (object[,])excelRange.get_Value(
                        XlRangeValueDataType.xlRangeValueDefault);

            // access the cells
            for (int row = 1; row <= xlWorkSheet.UsedRange.Rows.Count; ++row)
                typesOfLvls.Add(Convert.ToInt32(valueArray[row, 1]), valueArray[row, 2].ToString());

            // clean up stuffs
            Marshal.ReleaseComObject(xlWorkSheet);
            xlWorkBook.Close(false, Type.Missing, Type.Missing);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlWorkBooks);
            excelApp.Quit();
            Marshal.FinalReleaseComObject(excelApp);

            return typesOfLvls;
        }

        public static void WriteToExcel(Dictionary<string, List<(string TeamName, TimeSpan TimeResult)>> finalTable, string fileName)
        {
            Application excelApp = new Application
            {
                Visible = true,
                DisplayAlerts = false
            };

            var xlWorkBooks = excelApp.Workbooks;
            var xlWorkBook = xlWorkBooks.Add(Type.Missing);
            var xlWorkSheets = xlWorkBook.Worksheets;
            Worksheet xlWorkSheet = xlWorkSheets.get_Item(1);

            for (int i = 0; i < finalTable.Count; i++)
            {
                xlWorkSheet.Cells[1, i + 2] = finalTable.ElementAt(i).Key.ToString();

                for (int j = 0; j < finalTable.ElementAt(i).Value.Count; j++)
                {
                    if (i == 0)
                    {
                        xlWorkSheet.Cells[j + 2, 1] = finalTable.ElementAt(i).Value[j].TeamName.ToString();
                        xlWorkSheet.Cells[j + 2, 2] = finalTable.ElementAt(i).Value[j].TimeResult.ToString();
                    }
                    else
                    {
                        int columnIndex = finalTable.ElementAt(0).Value.FindIndex(x => x.TeamName == finalTable.ElementAt(i).Value[j].TeamName);
                        xlWorkSheet.Cells[columnIndex + 2, i + 2] = finalTable.ElementAt(i).Value[j].TimeResult.ToString();
                    }
                }

            }

            xlWorkSheet.get_Range("A1", "Z1").Font.Bold = true;
            xlWorkSheet.get_Range("A1", "Z1").VerticalAlignment = XlVAlign.xlVAlignCenter;
            xlWorkSheet.get_Range("A1", "A105").Font.Bold = true;
            xlWorkSheet.get_Range("A1", "A105").VerticalAlignment = XlVAlign.xlVAlignCenter;
            xlWorkSheet.get_Range("A2", "Z100").NumberFormat = "h:mm:ss";

            // mark best and worst time with green or red color
            for (char c = 'B'; c - 'A' <= finalTable.Count; c++)
            {
                // Hope all of the teams passed 0-index type of lvls
                int numberOfTeams = finalTable.ElementAt(0).Value.Count;
                string bestTimeFormula = string.Format("=${0}2=МИН(${0}$2:${0}${1})", c, numberOfTeams + 1);
                string worstTimeFormula = bestTimeFormula.Replace("МИН", "МАКС");

                Range range = xlWorkSheet.get_Range(c + "2", c + (numberOfTeams + 1).ToString());
                FormatConditions fcs = range.FormatConditions;
                FormatCondition fcW = (FormatCondition)fcs.Add
                    (XlFormatConditionType.xlExpression, Type.Missing, worstTimeFormula);
                Interior interiorW = fcW.Interior;
                interiorW.Color = ColorTranslator.ToOle(Color.Red);

                FormatCondition fcB = (FormatCondition)fcs.Add
                    (XlFormatConditionType.xlExpression, Type.Missing, bestTimeFormula);
                Interior interiorB = fcB.Interior;
                interiorB.Color = ColorTranslator.ToOle(Color.Green);
            }

            // save xls
            string safeDirectory = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\Statistics\\";
            xlWorkBook.SaveAs(string.Format(safeDirectory + "Game_ID{0}_stars.xlsx", fileName));

            // clean up stuffs
            Marshal.ReleaseComObject(xlWorkSheet);
            xlWorkBook.Close(false, Type.Missing, Type.Missing);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlWorkBooks);
            excelApp.Quit();
            Marshal.FinalReleaseComObject(excelApp);
        }
    }
}