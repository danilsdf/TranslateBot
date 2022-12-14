using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace TelegramBotTranslate
{
    static class SqlModel
    {
        public static List<string> GetAllTables()
        {
            var tables = new List<string>();
            using (var con = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    con.Open();
                    using (var command =
                        new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES Where TABLE_NAME <> 'Users' AND TABLE_NAME <> '__MigrationHistory';",
                            con))
                    {
                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                tables.Add(reader.GetValue(0).ToString());
                            }
                        }

                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }

            return tables;
        }

        public static bool Check(string tableName)
        {
            bool exists;
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    con.Open();
                    using (SqlCommand command =
                        new SqlCommand(
                            $"select case when exists((select * from information_schema.tables where table_name = '" +
                            tableName + "')) then 1 else 0 end", con))
                    {

                        Console.WriteLine(command.CommandText);
                        command.ExecuteNonQuery();
                        exists = (int) command.ExecuteScalar() == 1;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    try
                    {
                        exists = true;
                        SqlCommand command = new SqlCommand("select 1 from " + tableName + " where 1 = 0");
                        {
                            Console.WriteLine(command.CommandText);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch
                    {
                        exists = false;
                    }
                }
                finally
                {
                    con.Close();
                }

                return exists;
            }
        }

        public static void CreateTable(string tableName)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("CREATE TABLE " + tableName + "(Russian nvarchar(max),English nvarchar(max))", con))
                    {
                        Console.WriteLine(command.CommandText);
                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }
        public static void RecreateTable(string tableName)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("DROP TABLE " + tableName, con))
                    {
                        Console.WriteLine(command.CommandText);
                        command.ExecuteNonQuery();
                    }
                    CreateTable(tableName);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    CreateTable(tableName);
                }
                finally
                {
                    con.Close();
                }
            }
        }
        public static void InsertTable(string tableName, List<Word> words)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    con.Open();
                    foreach (var item in words)
                    {
                        using (SqlCommand command = new SqlCommand("INSERT INTO " + tableName + $" Values(N'{item.Russian}', N'{item.English}')", con))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }
        public static List<Word> GetWords(string tableName)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                List<Word> words = new List<Word>();
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand($"SELECT * FROM " + tableName, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                            while (reader.Read())
                            {
                                string rus = reader.GetValue(0).ToString();
                                string eng = reader.GetValue(1).ToString();

                                words.Add(new Word() { English = eng, Russian = rus });
                            }
                    }

                    reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                Console.WriteLine(words.Count);
                return words;
            }
        }
        public static string GetConnectionString()
        {
            string returnValue = null;

            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings["DBConnection"];

            if (settings != null)
                returnValue = settings.ConnectionString;

            return returnValue;
        }
    }
}
