using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Configuration;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory("Oracle.DataAccess.Client");
            var conn = factory.CreateConnection();

            conn.ConnectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

            bool wasOpened = false;
            try
            {
                conn.Open();
                wasOpened = conn.State == System.Data.ConnectionState.Open;
                conn.Close();
            }
            catch (Exception)
            {
                throw;
            }

            Console.WriteLine("The Connection {0} opened.", wasOpened ? "was" : "wasn´t");
            Console.Read();
        }
    }
}
