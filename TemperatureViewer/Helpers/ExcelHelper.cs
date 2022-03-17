using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using Microsoft.Office.Interop.Excel;
using TemperatureViewer.Models;

namespace TemperatureViewer.Helpers
{
    internal class ExcelHelper
    {
        public static void Create(Dictionary<Sensor, IEnumerable<Measurement>> data)
        {
            Application app = new Application();
            Workbook workbook = app.Workbooks.Add("");
            Worksheet worksheet = workbook.ActiveSheet as dynamic;
            WriteHeader(worksheet, data.Keys.Select(s => ));
        }

        public static void Create(string filePath, Dictionary<string, string> records)
        {
            Application app = new Application();
            Workbook workbook = app.Workbooks.Add("");
            Worksheet worksheet = workbook.ActiveSheet as dynamic;
            WriteHeader(worksheet, records["Termometer"]);
            WriteCells(worksheet, records);
            CreateChart(worksheet, records.Count - 1, records["Termometer"]);
            app.UserControl = false;
            workbook.SaveAs(filePath);
            workbook.Close();
            app.Quit();
        }

        private static void WriteHeader(Worksheet worksheet, IEnumerable<string> keys)
        {
            worksheet.Cells[1, 1] = "Время";
            worksheet.Cells[1, 2] = col2;
            (worksheet.Columns[1] as dynamic).ColumnWidth = 20;
        }

        private static void WriteCells(Worksheet worksheet, Dictionary<string, string> records)
        {
            object[,] data = new object[records.Count - 1, 2];
            int i = 0;
            foreach (KeyValuePair<string, string> pair in records)
            {
                if (pair.Key == "Termometer")
                    continue;

                data[i, 0] = pair.Key;
                data[i, 1] = double.Parse(pair.Value, CultureInfo.InvariantCulture.NumberFormat);
                ++i;
            }

            var range = worksheet.Range[$"A2:B{records.Count}"];
            range.Value2 = data;
        }

        private static void CreateChart(Worksheet worksheet, int length, string title)
        {
            ChartObjects charts = worksheet.ChartObjects() as ChartObjects;
            ChartObject chartObject = charts.Add(160, 40, 600, 300) as ChartObject;
            Chart chart = chartObject.Chart;
            Range range = worksheet.Range[$"A2:B{length + 1}"];
            chart.SetSourceData(range);
            (chart.Axes(XlAxisType.xlCategory) as Axis).CategoryType = XlCategoryType.xlCategoryScale;
            chart.ChartType = XlChartType.xlLine;
            chart.ChartWizard(CategoryTitle: "Время", ValueTitle: "Температура");
            chart.HasLegend = false;
            chart.ChartWizard(Title: title);
        }
    }
}
