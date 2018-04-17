using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiscountsForIC {
	class NpoiExcel {
		public static string WriteItemsDiscountToExcel(List<ItemDiscount> discountItems) {
			string templateFile = Environment.CurrentDirectory + "\\Template.xlsx";
			string resultFilePrefix = "Информация по скидкам для страховых компаний";
			foreach (char item in Path.GetInvalidFileNameChars())
				resultFilePrefix = resultFilePrefix.Replace(item, '-');

			if (!File.Exists(templateFile)) 
				return "Не удалось найти файл шаблона: " + templateFile;

			string resultPath = Path.Combine(Environment.CurrentDirectory, "Results\\" + DateTime.Now.ToString("yyyyMMdd"));
			if (!Directory.Exists(resultPath))
				Directory.CreateDirectory(resultPath);

			string resultFile = Path.Combine(resultPath, resultFilePrefix + "_" +
				DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
			
			IWorkbook workbook;
			using (FileStream stream = new FileStream(templateFile, FileMode.Open, FileAccess.Read))
				workbook = new XSSFWorkbook(stream);

			int rowNumber = 1;
			int columnNumber = 0;

			ISheet sheet = workbook.GetSheet("Data");
			
			foreach (ItemDiscount discountItem in discountItems) {
				IRow row = sheet.CreateRow(rowNumber);
				string[] values = new string[] {
					discountItem.SHORTNAME,
					discountItem.JNAME,
					discountItem.AGNUM,
					discountItem.BEGINDATE.ToShortDateString(),
					discountItem.ENDLESS ? "Да" : "Нет",
					discountItem.ENDLESS ? string.Empty : discountItem.ENDDATE.ToShortDateString(),
					//discountItem.AMOUNTRELATION ? "Да" : "Нет",
					//discountItem.STARTAMOUNT.HasValue ? discountItem.STARTAMOUNT.Value.ToString() : string.Empty,
					//discountItem.FINISHAMOUNT.HasValue ? discountItem.FINISHAMOUNT.Value.ToString() : string.Empty,
					discountItem.DISCOUNT.ToString(),
					discountItem.COMMENT
				};

				foreach (string value in values) {
					ICell cell = row.CreateCell(columnNumber);
					
					if (double.TryParse(value, out double result))
						cell.SetCellValue(result);
					else
						cell.SetCellValue(value);

					columnNumber++;
				}

				columnNumber = 0;
				rowNumber++;
			}

			using (FileStream stream = new FileStream(resultFile, FileMode.Create, FileAccess.Write))
				workbook.Write(stream);

			workbook.Close();

			return resultFile;
		}
	}
}
