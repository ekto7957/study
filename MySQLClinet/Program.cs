using System.Data;
using MySqlConnector;

namespace MySQLClinet
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string connectionString = "server=localhost;user=root;password=q1w2e3;database=users";
            MySqlConnection mySqlConnection = new MySqlConnection(connectionString);

            mySqlConnection.Open();
            MySqlCommand mysqlCommnad = new MySqlCommand();

            mysqlCommnad.Connection = mySqlConnection;
            mysqlCommnad.CommandText = "SELECT * FROM users where user_id = @user_id and user_password = @user_password;";
            mysqlCommnad.Prepare();
            mysqlCommnad.Parameters.AddWithValue("@user_id", "admin");
            mysqlCommnad.Parameters.AddWithValue("@user_password", "1234");

            MySqlDataReader dataReader = mysqlCommnad.ExecuteReader();
            while(dataReader.Read())
            {
                Console.WriteLine(dataReader["user_id"]);
                Console.WriteLine(dataReader["user_password"]);
            }
            

            //DataTable userTable = mySqlConnection.GetSchema("users");

            //foreach (DataColumn col in userTable.Columns)
            //{
            //    Console.WriteLine(col);
            //}

            //Task  task = mySqlConnection.OpenAsync();

            //task.Wait();

        }
    }
}
