using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Dungeons_and_Dragons_Helper.Utilities
{
    public class MainUtility
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SqLiteUtil SQL;

        public MainUtility(SqLiteUtil sql)
        {
            this.SQL = sql;
        }

        public DataTable GetAllFromTable(String table, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = SQL.dbConnection;
                    SQLiteHelper sh = new SQLiteHelper(cmd);
                    String query = $"SELECT * FROM {table}";
                    if (parameters != null)
                    {
                        query += " WHERE";
                        foreach (var keyValuePair in parameters)
                        {
                            query += $" {keyValuePair.Key} = {keyValuePair.Value}";
                            if (parameters.Last().Key != keyValuePair.Key)
                            {
                                query += "AND";
                            }
                        }
                    }
                    query += ";";
                    return sh.Select(query,parameters);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return null;
        }
        public DataTable GetAllAttributes()
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = SQL.dbConnection;
                    SQLiteHelper sh = new SQLiteHelper(cmd);
                    String query = $"SELECT atrybut_id,wartosc,modyfikator,nazwa FROM atrybuty_wartosci JOIN atrybuty a on atrybuty_wartosci.atrybut_id = a.id";
                    query += ";";
                    return sh.Select(query);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return null;
        }
    }
}