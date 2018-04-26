using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Dungeons_and_Dragons_Helper.Utilities
{
    class SqLiteUtil
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SqLiteUtil()
        {
            InitializeConnection();
        }

        private SQLiteConnection dbConnection;

        public Boolean InitializeConnection()
        {
            try
            {
                Log.Info("Otwieranie połączenia z bazą danych...");
                dbConnection = new SQLiteConnection("Data Source=Resources/DnD_Helper.sqlite;Version=3;");
                dbConnection.Open();
                Log.Info("Otwarto pomyślnie");
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            Log.Info("Błąd podczas łączenia z bazą SQLite");
            return false;
        }

        public SQLiteDataReader ExecuteQueryDQL(String query)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(query, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                return reader;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return null;
        }

        public Boolean ExecuteQueryDML(String query)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(query, dbConnection);
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return false;
        }
    }
}