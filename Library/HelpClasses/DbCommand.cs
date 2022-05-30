using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HelpClasses
{
    //Send a command to Heidi to see if Gnu exist.
    //Radera, onödig funktion?
    public class DbCommand
    {

        public static void CreateCommand(string queryString, string connectionstring)
        {
            try
            {
                using MySqlConnection conn = new MySqlConnection(connectionstring);
                {
                    MySqlCommand comm = conn.CreateCommand();
                    {
                        conn.Open();
                        comm.CommandText = queryString;
                        comm.ExecuteNonQuery();
                    }
                }
            }
              catch (Exception)
            {
                Console.WriteLine("Info: Db already exist/Or mail couldnt be read?");
                return;
            }
        }
    }
}
