using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiscountsForIC {
    class SystemDataHandle {
		private static string sqlQuerySearch = Properties.Settings.Default.MisSqlSearch;
		private static string sqlQuerySelectDiscounts = Properties.Settings.Default.MisSqlSelectDiscounts;
		private static SystemFirebirdClient firebirdClient = new SystemFirebirdClient(
			Properties.Settings.Default.MisDbAddress,
			Properties.Settings.Default.MisDbName,
			Properties.Settings.Default.MisDbUserName,
			Properties.Settings.Default.MisDbPassword);

		public static void CloseConnection() {
			firebirdClient.Close();
		}

		public static List<ItemIC> SearchForIC(string text) {
			List<ItemIC> list = new List<ItemIC>();


			DataTable dataTable = firebirdClient.GetDataTable(sqlQuerySearch, new Dictionary<string, string> { { "@entered", text } });
			if (dataTable.Rows.Count == 0) {
				list.Add(new ItemIC() { JNAME = "Нет данных" });
				return list;
			}

			foreach (DataRow row in dataTable.Rows) {
				try {
					list.Add(new ItemIC() {
						JNAME = row["JNAME"].ToString(),
						AGNUM = row["AGNUM"].ToString(),
						JID = row["JID"].ToString(),
						AGRID = row["AGRID"].ToString()
					});
				} catch (Exception e) {
					MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Ошибка обработки данных",
						MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			return list;
		}

		public static List<ItemDiscount> SelectDiscount(System.Collections.IList selectedItemsIC) {
			List<ItemDiscount> list = new List<ItemDiscount>();

			string agrids = string.Empty;

			foreach (ItemIC item in selectedItemsIC)
				agrids += item.AGRID + ",";

			agrids = agrids.TrimEnd(',');

			DataTable dataTable = firebirdClient.GetDataTable(sqlQuerySelectDiscounts.Replace("@list", agrids), new Dictionary<string, string>() );
			if (dataTable.Rows.Count == 0) {
				list.Add(new ItemDiscount() { FILIAL = "Нет данных" });
				return list;
			}

			foreach (DataRow row in dataTable.Rows) {
				try {
					DateTime.TryParseExact(row["BEGINDATE"].ToString(), "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime begindate);
					DateTime.TryParseExact(row["ENDDATE"].ToString(), "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime enddate);

					list.Add(new ItemDiscount() {
						BZ_ADID = row["BZ_ADID"].ToString(),
						JID = row["JID"].ToString(),
						AGRID = row["AGRID"].ToString(),
						FILIAL = row["FILIAL"].ToString(),
						ENDLESS = row["ENDLESS"].ToString().Equals("1"),
						BEGINDATE = begindate,
						ENDDATE = enddate,
						AMOUNTRELATION = row["AMOUNTRELATION"].ToString().Equals("1"),
						STARTAMOUNT = row["STARTAMOUNT"].ToString(),
						FINISHAMOUNT = row["FINISHAMOUNT"].ToString(),
						COMMENT = row["COMMENT"].ToString(),
						DISCOUNT = row["DISCOUNT"].ToString()
					});
				} catch (Exception e) {
					MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Ошибка обработки данных",
						MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			
			return list;
		}
	}
}
