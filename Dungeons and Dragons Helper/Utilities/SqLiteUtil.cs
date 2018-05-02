using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Dungeons_and_Dragons_Helper.Utilities
{
public class SqLiteUtil
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SqLiteUtil()
        {
            InitializeConnection();
        }

        public SQLiteConnection dbConnection;

        public Boolean InitializeConnection()
        {
            try
            {
                Log.Info("Otwieranie połączenia z bazą danych...");
               var p =Path.GetFullPath("Resources/DnD_Helper.sqlite");
                dbConnection = new SQLiteConnection("Data Source=" + Path.GetFullPath("Resources/DnD_Helper.sqlite"));

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
    }
}