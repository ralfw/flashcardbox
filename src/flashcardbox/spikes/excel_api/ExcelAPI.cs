using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Xunit;
using Xunit.Abstractions;

namespace spikes
{
    public class ExcelAPI
    {
        public class FlashcardRecord {
            public string Id { get; set; }
            public string Question { get; set; }
            public string Answer { get; set; }
            public string Tags { get; set; }
            public string BinIndex { get; set; }
        }
        
        
        
        private readonly ITestOutputHelper _testOutputHelper;

        public ExcelAPI(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        
        [Fact]
        public void Read_write_xlsx()
        {
            using (var wb = new XLWorkbook("flashcards.xlsx"))
            {
                var records = new List<FlashcardRecord>();
                
                var ws = wb.Worksheets.First();
                var rowNumber = 0;
                foreach (var row in ws.RowsUsed()) {
                    if (rowNumber > 0) {
                        records.Add(new FlashcardRecord {
                            Id = GetCell(row, "E"),
                            Question = GetCell(row, "A"),
                            Answer = GetCell(row, "B"),
                            Tags = GetCell(row, "C"),
                            BinIndex = GetCell(row, "D")
                        });
                    }
                    rowNumber++;
                }
                
                foreach(var rec in records)
                    _testOutputHelper.WriteLine($"{rec.Id}: {rec.Question}={rec.Answer}, {rec.Tags}/{rec.BinIndex}");

                foreach (var rec in records)
                {
                    if (rec.Id == "")
                        rec.Id = Guid.NewGuid().ToString();
                    if (rec.BinIndex == "")
                        rec.BinIndex = Guid.NewGuid().ToString().Substring(0, 2);
                }

                rowNumber = 0;
                foreach (var row in ws.RowsUsed()) {
                    if (rowNumber > 0)
                    {
                        var fc = records[rowNumber - 1];
                        row.Cell("E").Value = fc.Id;
                        row.Cell("A").Value = fc.Question;
                        row.Cell("B").Value = fc.Answer;
                        row.Cell("C").Value = fc.Tags;
                        row.Cell("D").Value = fc.BinIndex;
                    }
                    rowNumber++;
                }
                 
                wb.Save();
            }


            string GetCell(IXLRow row, string columnName)
            {
                try
                {
                    return row.Cell(columnName).GetString().Replace("\n", "$");
                }
                catch
                {
                    return "";
                }
            }
        }
    }
}