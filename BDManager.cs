using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using Microsoft.Data.Sqlite;
public class DBManager 
{
    private SqliteConnection? connection = null;

    private string HashPassword(string password)
    {
        using (var algorithm = SHA256.Create())
        {
            var bytes_hash = algorithm.ComputeHash(Encoding.Unicode.GetBytes(password));
            return Encoding.Unicode.GetString(bytes_hash);
        }
    }

    public bool ConnectToDB(string path)
    {
        Console.WriteLine("Подключение к базе данных...");
        try
        { 
            connection = new SqliteConnection("Data source=" + path);
            connection.Open();

            if(connection.State != System.Data.ConnectionState.Open)
            { Console.WriteLine("Ошибка!"); return false;}
        }
        catch(Exception exp)
        { Console.WriteLine(exp.Message);}
        
        Console.WriteLine("Успешно!");
        return true;
    }

    public void Disconnect()
    {
        if (null == connection) 
            return;
        if (connection.State != System.Data.ConnectionState.Open) 
            return;
        connection.Close();
        Console.WriteLine("Отключение от базы данных");
    } 

    public bool AddUser(string login, string password)
    {
        if (null == connection)
            return false;  
        if (connection.State != System.Data.ConnectionState.Open)
            return false;

        string REQUEST = "INSERT INTO CombSortingUsers (login, password) VALUES ('" + login + "', '" + HashPassword(password) + "')";
        var command = new SqliteCommand(REQUEST, connection);
            int result = 0;

        try
        { result = command.ExecuteNonQuery(); } 
        
        catch(Exception exp)
        { Console.WriteLine(exp.Message); return false; }

        if (1 == result)
            return true;
        else
            return false;
    }

    public bool CheckUser(string login, string password)
    {
        if (null == connection)
            return false; 
  
        if (connection.State != System.Data.ConnectionState.Open)
            return false;
        
        string REQUEST = "SELECT login, password FROM CombSortingUsers WHERE login='" + login + "' AND password= '" + HashPassword(password) + "'";
        var command = new SqliteCommand(REQUEST, connection);
        try
        {
            var reader = command.ExecuteReader();

            if (reader.HasRows)
                return true;
            else
                return false;
        }
        catch(Exception exp)
        { Console.WriteLine(exp.Message); return false; }
    }
}

