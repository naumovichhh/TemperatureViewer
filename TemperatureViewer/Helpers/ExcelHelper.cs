using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using NPOI.XSSF.UserModel;
using TemperatureViewer.Models.Entities;
using NPOI.SS.UserModel;

namespace TemperatureViewer.Helpers
{
    public static class ExcelHelper
    {
        public static MemoryStream Create(IDictionary<Sensor, IEnumerable<Measurement>> data, IEnumerable<DateTime> measurementTimes)
        {
            CheckEnumerableLengthEqual(data);
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Sheet1");
            WriteHeader(sheet, data.Keys.Select(s => s.Name).OrderBy(s => s));
            WriteCells(sheet, data, measurementTimes);
            MemoryStream result = new MemoryStream();
            workbook.Write(result);
            return result;
        }

        private static void CheckEnumerableLengthEqual(IDictionary<Sensor, IEnumerable<Measurement>> data)
        {
            if (!data.All(e => e.Value.Count() == data.First().Value.Count()))
                throw new ArgumentException("Data must contain enumerables of measurements of equal length.", nameof(data));
        }

        private static void WriteHeader(ISheet sheet, IEnumerable<string> names)
        {
            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("Время");
            sheet.SetColumnWidth(0, 7000);
            int i = 1;
            foreach (string name in names)
            {
                row.CreateCell(i).SetCellValue(name);
                sheet.SetColumnWidth(i, 7000);
                ++i;
            }
        }

        private static void WriteCells(ISheet sheet, IDictionary<Sensor, IEnumerable<Measurement>> data, IEnumerable<DateTime> measurementTimes)
        {
            int i = 1;
            foreach (var dateTime in measurementTimes)
            {
                sheet.CreateRow(i).CreateCell(0).SetCellValue(dateTime.ToString("g", CultureInfo.GetCultureInfo("de-DE")));
                ++i;
            }

            int j = 1;
            foreach (var keyValuePair in data.OrderBy(s => s.Key.Name))
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
    }
}
