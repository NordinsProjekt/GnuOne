using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Library
{
    public class sqlite
    {
        public static string DbFile
        {
            get { return Environment.CurrentDirectory + "\\gnuone.sqlite"; }
        }

        public static SqliteConnection SimpleDbConnection()
        {
            return new SqliteConnection("Data Source=" + DbFile);
        }
    }
}
