using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using Microsoft.Office.Interop.Excel;
using NPOI.XSSF.UserModel;
using TemperatureViewer.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel.Charts;
using NPOI.SS.Util;
using NPOI.XWPF.UserModel;

namespace TemperatureViewer.Helpers
{
    public static class ExcelHelper
    {
        public static MemoryStream Create(Dictionary<Sensor, IEnumerable<Measurement>> data)
        {
            CheckEnumerableLengthEqual(data);
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Sheet1");
            WriteHeader(sheet, data.Keys.Select(s => s.Name).OrderBy(s => s));
            WriteCells(sheet, data);
            CreateChart(sheet, data);
            MemoryStream result = new MemoryStream();
            workbook.Write(result);
            return result;
        }
        //public static string Create(Dictionary<Sensor, IEnumerable<Measurement>> data)
        //{
        //    CheckEnumerableLengthEqual(data);
        //    Application app = new Application();
        //    Workbook workbook = app.Workbooks.Add("");
        //    Worksheet worksheet = workbook.ActiveSheet as dynamic;
        //    WriteHeader(worksheet, data.Keys.Select(s => s.Name).OrderBy(s => s));
        //    WriteCells(worksheet, data);
        //    Directory.CreateDirectory("excel");
        //    string fileName = "excel/table.xlsx";
        //    workbook.SaveAs(fileName);
        //    return fileName;
        //}

        //public static void Create(string filePath, Dictionary<string, string> records)
        //{
        //    Application app = new Application();
        //    Workbook workbook = app.Workbooks.Add("");
        //    Worksheet worksheet = workbook.ActiveSheet as dynamic;
        //    WriteHeader(worksheet, records["Termometer"]);
        //    WriteCells(worksheet, records);
        //    CreateChart(worksheet, records.Count - 1, records["Termometer"]);
        //    app.UserControl = false;
        //    workbook.SaveAs(filePath);
        //    workbook.Close();
        //    app.Quit();
        //}

        private static void CreateChart(ISheet sheet, Dictionary<Sensor, IEnumerable<Measurement>> data)
        {
            int categoriesNum = data.First().Value.Count();
            int linesNum = data.Count;

            XSSFDrawing drawing = (XSSFDrawing)sheet.CreateDrawingPatriarch();
            XSSFClientAnchor anchor = (XSSFClientAnchor)drawing.CreateAnchor(0, 0, 0, 0, 0, 5, 10, 15);
            XSSFChart chart = (XSSFChart)drawing.CreateChart(anchor);

            ILineChartData<string, double> lineChartData = chart.ChartDataFactory.CreateLineChartData<string, double>();
            IChartLegend legend = chart.GetOrCreateLegend();
            legend.Position = LegendPosition.Bottom;

            IChartAxis bottomAxis = chart.ChartAxisFactory.CreateCategoryAxis(AxisPosition.Bottom);
            bottomAxis.MajorTickMark = AxisTickMark.None;
            IValueAxis leftAxis = chart.ChartAxisFactory.CreateValueAxis(AxisPosition.Left);
            leftAxis.Crosses = AxisCrosses.AutoZero;
            leftAxis.SetCrossBetween(AxisCrossBetween.Between);


            //IChartDataSource<string> categoryAxis = DataSources.FromStringCellRange(sheet, new CellRangeAddress(0, 0, 1, 1));
            //IChartDataSource<double> valueAxis = DataSources.FromNumericCellRange(sheet, new CellRangeAddress(1, categoriesNum, 1, 1));
            IChartDataSource<string> categoryAxis = DataSources.FromArray(new string[] { "bydlo" });
            IChartDataSource<double> valueAxis = DataSources.FromArray<double>(new double[] { 5.5, 45.2, 23, 29 });
            var serie = lineChartData.AddSeries(categoryAxis, valueAxis);
            serie.SetTitle("Bydlo");

            chart.Plot(lineChartData);
        }

        private static void CheckEnumerableLengthEqual(Dictionary<Sensor, IEnumerable<Measurement>> data)
        {
            if (!data.All(e => e.Value.Count() == data.First().Value.Count()))
                throw new ArgumentException("Data must contain enumerables of measurements of equal length.", nameof(data));
        }

        private static void WriteHeader(ISheet sheet, IEnumerable<string> names)
        {
            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("Время");
            int i = 1;
            foreach (string name in names)
            {
                row.CreateCell(i).SetCellValue(name);
                ++i;
            }
        }

        private static void WriteCells(ISheet sheet, Dictionary<Sensor, IEnumerable<Measurement>> data)
        {
            int i = 1;
            foreach (var measurement in data.First().Value)
            {
                sheet.CreateRow(i).CreateCell(0).SetCellValue(measurement.MeasurementTime.ToString("g", CultureInfo.GetCultureInfo("de-DE")));
                ++i;
            }

            int j = 1;
            foreach (var keyValuePair in data)
            {
                int k = 1;
                foreach (var measurement in keyValuePair.Value)
                {
                    if (measurement != null)
                        sheet.GetRow(k).CreateCell(j).SetCellValue((double)measurement.Temperature);

                    k++;
                }

                j++;
            }
        }

        //private static void WriteHeader(Worksheet worksheet, IEnumerable<string> names)
        //{
        //    worksheet.Cells[1, 1] = "Время";
        //    int i = 2;
        //    foreach (string name in names)
        //    {
        //        worksheet.Cells[1, i] = name;
        //        ++i;
        //    }

        //    (worksheet.Columns[1] as dynamic).ColumnWidth = 20;
        //}

        private static void WriteCells(Worksheet worksheet, Dictionary<Sensor, IEnumerable<Measurement>> data)
        {
            object[,] objects = new object[data.First().Value.Count(), data.Count];
            int i = 0;
            foreach (var measurement in data.First().Value)
            {
                objects[i, 0] = measurement.MeasurementTime.ToString();
                ++i;
            }

            int j = 1;
            foreach (var keyValuePair in data)
            {
                int k = 0;
                foreach (var measurement in keyValuePair.Value)
                {
                    objects[k, j] = measurement.Temperature;
                    k++;
                }

                j++;
            }

            var range = worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[data.First().Value.Count() + 1, data.Count]];
            range.Value2 = objects;
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
            Microsoft.Office.Interop.Excel.Range range = worksheet.Range[$"A2:B{length + 1}"];
            chart.SetSourceData(range);
            (chart.Axes(XlAxisType.xlCategory) as Axis).CategoryType = XlCategoryType.xlCategoryScale;
            chart.ChartType = XlChartType.xlLine;
            chart.ChartWizard(CategoryTitle: "Время", ValueTitle: "Температура");
            chart.HasLegend = false;
            chart.ChartWizard(Title: title);
        }
    }
}
