using System;
using System.Data;
using System.Collections.Generic;
using FirebirdSql.Data.FirebirdClient;

namespace LoyaltySurvey {
    class SystemFirebirdClient {
        private FbConnection connection;

		public SystemFirebirdClient(string ipAddress, string baseName, string user, string pass) {
			SystemLogging.LogMessageToFile("Создание подключения к базе FB: " + 
				ipAddress + ":" + baseName);

			FbConnectionStringBuilder cs = new FbConnectionStringBuilder();
            cs.DataSource = ipAddress;
            cs.Database = baseName;
            cs.UserID = user;
            cs.Password = pass;
            cs.Charset = "NONE";
            cs.Pooling = false;

            connection = new FbConnection(cs.ToString());
		}

		public DataTable GetDataTable(string query) {
			DataTable dataTable = new DataTable();

			try {
				connection.Open();
				FbCommand command = new FbCommand(query, connection);

				FbDataAdapter fbDataAdapter = new FbDataAdapter(command);
				fbDataAdapter.Fill(dataTable);
			} catch (Exception e) {
				SystemLogging.LogMessageToFile("GetDataTable exception: " + query + 
					Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
			} finally {
				connection.Close();
			}

			return dataTable;
		}

		public bool ExecuteUpdateQuery(string query, Dictionary<string, object> parameters) {
			bool updated = false;
			try {
				connection.Open();
				FbCommand update = new FbCommand(query, connection);

				if (parameters.Count > 0) {
					foreach (KeyValuePair<string, object> parameter in parameters)
						update.Parameters.AddWithValue(parameter.Key, parameter.Value);
				}

				updated = update.ExecuteNonQuery() > 0 ? true : false;
			} catch (Exception e) {
				SystemLogging.LogMessageToFile("ExecuteUpdateQuery exception: " + query +
					Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
			} finally {
				connection.Close();
			}

			return updated;
		}
	}
}
