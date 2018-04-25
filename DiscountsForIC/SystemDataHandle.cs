using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiscountsForIC {
    static class SystemDataHandle {
		private static string sqlQuerySearch = Properties.Settings.Default.MisSqlSearch;
		private static string sqlQuerySelectDiscounts = Properties.Settings.Default.MisSqlSelectDiscounts;
		private static string sqlQueryUpdateOrInsertDiscount = Properties.Settings.Default.MisSqlUpdateOrInsertDiscount;
		private static string sqlQueryDeleteDiscounts = Properties.Settings.Default.MisSqlDeleteDiscounts;
		private static string sqlQuerySelectFilials = Properties.Settings.Default.MisSqlSelectFilials;
		private static SystemFirebirdClient firebirdClient = new SystemFirebirdClient(
			Properties.Settings.Default.MisDbAddress,
			Properties.Settings.Default.MisDbName,
			Properties.Settings.Default.MisDbUserName,
			Properties.Settings.Default.MisDbPassword);

		public static void CloseConnection() {
			firebirdClient.Close();
		}

		public static void DeleteDiscounts(List<ItemDiscount> itemsDiscount) {
			foreach (ItemDiscount item in itemsDiscount) {
				if (item.BZ_ADID is null)
					continue;

				firebirdClient.ExecuteUpdateQuery(sqlQueryDeleteDiscounts,
					new Dictionary<string, object> { { "@bz_adids", item.BZ_ADID } });
			}
		}

		public static bool UpdateOrInsertDiscount(List<ItemDiscount> itemsDiscount) {
			foreach (ItemDiscount item in itemsDiscount) {
				try {
					Dictionary<string, object> parameters = new Dictionary<string, object>() {
						{ "@bz_adid", item.BZ_ADID },
						{ "@jid", item.JID },
						{ "@agrid", item.AGRID },
						{ "@filial", item.FILIAL },
						{ "@endless", item.ENDLESS ? 1 : 0 },
						{ "@begindate", item.BEGINDATE },
						{ "@enddate", item.ENDLESS ? (object)null : (object)item.ENDDATE },
						{ "@amountrelation", item.AMOUNTRELATION ? 1 : 0 },
						{ "@startamount", item.AMOUNTRELATION ? (object)item.STARTAMOUNT : (object)null },
						{ "@finishamount", item.AMOUNTRELATION ? (object)item.FINISHAMOUNT : (object)null },
						{ "@comment", item.COMMENT },
						{ "@discount", item.DISCOUNT }
					};
					
					if (!firebirdClient.ExecuteUpdateQuery(sqlQueryUpdateOrInsertDiscount, parameters))
						return false;
				} catch (Exception e) {
					MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, 
						"Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
					return false;
				}
			}

			return true;
		}

		public static List<ItemIC> SearchForIC(string text, List<ItemFilial> itemsFilial, bool displayClosedContracts) {
			List<ItemIC> list = new List<ItemIC>();
			
			DataTable dataTable = firebirdClient.GetDataTable(sqlQuerySearch, new Dictionary<string, string> { { "@entered", text } });
			if (dataTable.Rows.Count == 0)
				return list;

			foreach (DataRow row in dataTable.Rows) {
				try {
					Dictionary<string, int> values = new Dictionary<string, int> {
						{ "FILIAL", 0 },
						{ "JID", 0 },
						{ "AGRID", 0 }
					};

					List<string> keys = values.Keys.ToList();
					foreach (string key in keys)
						if (int.TryParse(row[key].ToString(), out int value))
							values[key] = value;

					DateTime.TryParse(row["AGDATE"].ToString(), out DateTime agdate);
					DateTime.TryParse(row["EDATE"].ToString(), out DateTime dateTime);
					bool isClosed = row["ISCLOSE"].ToString().Equals("1");
					bool autoProlong = row["AUTOPROLONG"].ToString().Equals("1");
					DateTime? eDate = null;
					if (dateTime != new DateTime())
						eDate = dateTime;

					ItemIC itemIC = new ItemIC() {
						FILIAL = values["FILIAL"],
						SHORTNAME = row["SHORTNAME"].ToString(),
						JNAME = row["JNAME"].ToString(),
						AGNUM = row["AGNUM"].ToString(),
						JID = values["JID"],
						AGRID = values["AGRID"],
						AGDATE = agdate,
						EDATE = eDate,
						ISCLOSE = isClosed,
						AUTOPROLONG = autoProlong
					};

					if (!displayClosedContracts) {
						if (itemIC.ISCLOSE)
							continue;

						if (itemIC.EDATE.HasValue && (itemIC.EDATE.Value < DateTime.Now && !autoProlong))
							continue;
					}

					bool isIcFilialInList = itemsFilial.Any(x => x.ID == itemIC.FILIAL);
					bool isMoscowInList = itemsFilial.Any(x => x.ShortName.ToLower().Contains("москва"));

					if (!isIcFilialInList && !(isMoscowInList && itemIC.FILIAL == 99))
						continue;

					list.Add(itemIC);
				} catch (Exception e) {
					MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Ошибка обработки данных",
						MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			return list;
		}

		public static List<ItemDiscount> SelectDiscoutByDates(string sqlQuery, Dictionary<string, string> parameters, 
			List<ItemFilial> itemsFilial, bool showClosedContracts) {
			DataTable dataTable = firebirdClient.GetDataTable(sqlQuery, parameters);
			return ParseDataTableDiscounts(dataTable, itemsFilial, showClosedContracts);
		}

		public static List<ItemDiscount> SelectDiscount(System.Collections.IList selectedItemsIC) {
			List<ItemDiscount> list = new List<ItemDiscount>();

			string agrids = string.Empty;

			for (int i = 0; i < selectedItemsIC.Count; i += 1400) {
				for (int x = 0; x < i + 1400; x++) {
					if (i + x >= selectedItemsIC.Count)
						break;

					agrids += (selectedItemsIC[i + x] as ItemIC).AGRID + ",";
				}

				agrids = agrids.TrimEnd(',');

				DataTable dataTable = firebirdClient.GetDataTable(
					sqlQuerySelectDiscounts.Replace("@list", agrids), 
					new Dictionary<string, string>()
				);

				agrids = string.Empty;

				list.AddRange(ParseDataTableDiscounts(dataTable));
			}

			return list;
		}

		private static List<ItemDiscount> ParseDataTableDiscounts(DataTable dataTable, List<ItemFilial> itemsFilial = null, bool showClosedContracts = false) {
			List<ItemDiscount> list = new List<ItemDiscount>();

			foreach (DataRow row in dataTable.Rows) {
				try {
					DateTime.TryParseExact(
						row["BEGINDATE"].ToString(),
						"dd.MM.yyyy H:mm:ss",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AssumeLocal,
						out DateTime begindate);

					if (!DateTime.TryParseExact(
						row["ENDDATE"].ToString(),
						"dd.MM.yyyy H:mm:ss",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AssumeLocal,
						out DateTime enddate))
						enddate = begindate;

					DateTime? edate = null;
					if (dataTable.Columns.Contains("EDATE") && DateTime.TryParse(row["EDATE"].ToString(), out DateTime dateTimeEdate))
						edate = dateTimeEdate;

					bool isClose = false;
					if (dataTable.Columns.Contains("ISCLOSE"))
						isClose = row["ISCLOSE"].ToString().Equals("1");

					bool autoProlong = false;
					if (dataTable.Columns.Contains("AUTOPROLONG"))
						autoProlong = row["AUTOPROLONG"].ToString().Equals("1");

					Dictionary<string, int> values = new Dictionary<string, int> {
							{ "STARTAMOUNT", 0 },
							{ "FINISHAMOUNT", 0 },
							{ "DISCOUNT", 0 },
							{ "BZ_ADID", 0 },
							{ "JID", 0 },
							{ "AGRID", 0 },
							{ "FILIAL", 0 }
						};

					List<string> keys = values.Keys.ToList();
					foreach (string key in keys)
						if (int.TryParse(row[key].ToString(), out int value))
							values[key] = value;

					ItemDiscount itemDiscount = new ItemDiscount(
						row["SHORTNAME"].ToString(),
						row["JNAME"].ToString(),
						row["AGNUM"].ToString(),
						values["BZ_ADID"],
						values["JID"],
						values["AGRID"],
						values["FILIAL"],
						row["ENDLESS"].ToString().Equals("1"),
						begindate,
						enddate,
						row["AMOUNTRELATION"].ToString().Equals("1"),
						values["STARTAMOUNT"],
						values["FINISHAMOUNT"],
						row["COMMENT"].ToString(),
						values["DISCOUNT"]
					);

					if (itemsFilial != null) {
						bool isIcFilialInList = itemsFilial.Any(x => x.ID == itemDiscount.FILIAL);
						bool isMoscowInList = itemsFilial.Any(x => x.ShortName.ToLower().Contains("москва"));

						if (!isIcFilialInList && !(isMoscowInList && itemDiscount.FILIAL == 99))
							continue;
					}

					if (!showClosedContracts) {
						if (isClose)
							continue;

						if (edate.HasValue && (edate.Value < DateTime.Now && !autoProlong))
							continue;
					}

					itemDiscount.Contract.Add(itemDiscount.ContractPreview);
					list.Add(itemDiscount);
				} catch (Exception e) {
					MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Ошибка обработки данных",
						MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			return list;
		}

		public static List<ItemFilial> GetFilials() {
			List<ItemFilial> itemsFilial = new List<ItemFilial>();

			DataTable dataTable = firebirdClient.GetDataTable(sqlQuerySelectFilials, new Dictionary<string, string>());
			if (dataTable.Rows.Count == 0)
				return itemsFilial;

			foreach (DataRow row in dataTable.Rows) {
				try {
					string id = row["FILID"].ToString();
					string shortName = row["SHORTNAME"].ToString();
					string fullName = row["FULLNAME"].ToString();

					ItemFilial itemFilial = new ItemFilial() {
						ID = int.Parse(id),
						ShortName = shortName,
						FullName = fullName
					};

					if (itemFilial.ID == 97 || itemFilial.ID == 94 || itemFilial.ID == 99)
						continue;

					if (itemFilial.FullName.ToLower().Contains("москва")) {
						if (itemsFilial.Any(p => p.ID == 0))
							continue;

						itemFilial.ID = 0;
						itemFilial.ShortName = "Москва";
						itemFilial.FullName = "Московские клиники (МДМ, СРЕТ, СУЩ, УК)";
					}

					itemsFilial.Add(itemFilial);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}

			itemsFilial = itemsFilial.OrderBy(p => p.ShortName).ToList();

			return itemsFilial;
		}
	}
}
