﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotTranslate
{
    static class SqlModel
    {
        public static bool Check(string tableName)
        {
            bool exists;
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand($"select case when exists((select * from information_schema.tables where table_name = '" + tableName + "')) then 1 else 0 end", con))
                    {

                        Console.WriteLine(command.CommandText);
                        command.ExecuteNonQuery();
                        exists = (int)command.ExecuteScalar() == 1;
                    }
                }
                catch (Exception ex)
                {
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
            }
            return exists;
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
            }
        }
        public static void InsertTable(string tableName, string eng, string rus)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("INSERT INTO " + tableName + $" Values(N'{rus}', N'{eng}')", con))
                    {
                        Console.WriteLine(command.CommandText);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
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
                        Console.WriteLine(command.CommandText);
                        SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                            while (reader.Read())
                            {
                                string rus = reader.GetValue(0).ToString();
                                string eng = reader.GetValue(1).ToString();
                                Console.WriteLine($"{rus} {eng}");

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
