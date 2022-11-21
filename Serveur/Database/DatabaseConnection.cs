using System;
using System.Data;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace Server.Database
{
    static public class DatabaseConnection
    {
        static private String _USERNAME = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "";
        static private String _PASSWORD = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "";

        static public MySqlConnection NewConnection()
        {
            return new MySqlConnection($"Server=127.0.0.1;User ID={_USERNAME};Password={_PASSWORD};Port=3306;Database=bdd_lotd");
        }
    }
}