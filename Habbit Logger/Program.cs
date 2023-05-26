using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Globalization;

namespace Habbit_Logger
{
    class DBInfo
    {
        static string connectionString = @"Data Source=habit-Tracker.db"; //Datasource case sensetive
        static void Main(string[] args)
        {
            using (var connection = new SqliteConnection(connectionString))//connection while using is for garbage collecting
            {
                connection.Open();//opens the connection

                var tableCmd = connection.CreateCommand();//creates a command for the database

                tableCmd.CommandText = //Creates a table with these data fields if it does not exist.
                    @"CREATE TABLE IF NOT EXISTS drinking_water (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT,
                        Quantity INTEGER
                        )" ;//the command is text
                        

                tableCmd.ExecuteNonQuery();//Database does not return any value aka not querying the db, only creating a table;

                connection.Close(); //closes the connection with db
            }
            GetUserInput();
        }

        static void GetUserInput()//Getting user input for their command
        {
            Console.Clear();
            bool closeApp = false;//check if the user closed the application

            while (closeApp == false)//if the user hasn't closed the app, while loop shows.
            {
                Console.WriteLine("\n\nMAIN MENU");
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("\nType 0 to Close Application.");
                Console.WriteLine("Type 1 to View All Records.");
                Console.WriteLine("Type 2 to Insert Records.");
                Console.WriteLine("Type 3 to Delete Record.");
                Console.WriteLine("Type 4 to Update Record.");
                Console.WriteLine("---------------------------------------------\n");

                string command = Console.ReadLine();//string based command for the application.

                switch(command) 
                {
                    case "0":
                        Console.WriteLine("\nGoodbye!\n");
                        closeApp = true;
                        Environment.Exit(0);
                        break;
                    case "1":
                        GetRecords();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        Delete();
                        break;
                    case "4":
                        Update();
                        break;
                    default:
                        Console.WriteLine("\nInvalid Command. Please type a number from 0 to 4.\n");
                        break;
                
                }
            }
        }

        private static void GetRecords()//Get database records
        {
            Console.Clear();

            using (var connection = new SqliteConnection(connectionString))//Create connection
            {
                connection.Open();//open connection
                var tableCmd = connection.CreateCommand();//create db command
                tableCmd.CommandText =//Selects everything from the database drinking_water
                    $"SELECT * FROM drinking_water";

                List<DrinkingWater> tableData = new();//Storing rows of the table

                SqliteDataReader reader = tableCmd.ExecuteReader();//reads data from the database

                if(reader.HasRows)//if sqlite reader has rows
                {
                    while (reader.Read())//while the reader is reading the lines
                    {
                        tableData.Add(//adds new DrinkingWater object
                        new DrinkingWater
                        {
                            Id = reader.GetInt32(0),//gets ID as int
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo ("en-US")),//gets date with specific format(Prase exact) and culture info as en-US
                            Quantity= reader.GetInt32(2),//gets Quantity as an int in 3rd column

                        });
                    }

                }
                else
                {
                    Console.WriteLine("No rows found");
                }

                connection.Close();

                Console.WriteLine("--------------------------\n");
                foreach(var drinkingWater in tableData)//gets every object in the database
                {
                    Console.WriteLine($"ID: {drinkingWater.Id} - DATE: {drinkingWater.Date.ToString("dd-MMM-yyyy")} - Quantity: {drinkingWater.Quantity} glasses");
                }
                Console.WriteLine("--------------------------\n");
            }

        }

        private static void Insert()
        {
            string date = GetDateInput();//get the date from user input

            int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measurements of your choice (no decimals).\n\n");
            //get quantity from the user input

            using (var connection = new SqliteConnection(connectionString))//create a connection
            {
                connection.Open();//connect
                var tableCmd = connection.CreateCommand();//create a command 
                tableCmd.CommandText =//Inserts the date and quantity into the database
                    $"INSERT INTO drinking_water(date,quantity) VALUES('{date}',{quantity})";

                tableCmd.ExecuteNonQuery();//Database does not return any value aka not querying the db, only creating a table;

                connection.Close();//closes the connection with db
            }

        }

        private static void Delete()//function to remove a record from database
        {
            Console.Clear();
            GetRecords();

            var recordId = GetNumberInput("\n\nType the Id of the record you want to remove or press 0 to go back to main menu\n\n");

            using (var connection = new SqliteConnection(connectionString))//creates connection
            {
                connection.Open();//opens connections            
                var tableCmd = connection.CreateCommand();//creates table command
                tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = '{recordId}'"; //uses a string to get the recordId

                int rowCount = tableCmd.ExecuteNonQuery();//does not return records but returns amount of rows affected by command

                if(rowCount == 0)
                {
                    Console.WriteLine($"\n\nRecord with Id {recordId} does not exist.\n\n");
                    Delete();
                }
            }

            Console.WriteLine($"\n\nRecord with Id {recordId} was deleted \n\n");

            GetUserInput();
        }

        private static void Update()
        {
            Console.Clear();
            GetRecords();

            var recordId = GetNumberInput("\n\nType the Id of the record you want to remove or press 0 to go back to main menu\n\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());//returns the value from the database

                if (checkQuery == 0)//if checkQuery returns 0, records do not exist.
                {
                    Console.WriteLine($"\n\nRecord with Id {recordId} does not exist.\n\n");
                    connection.Close();
                    Update();
                }

                string date = GetDateInput();//Gets the input from user again if record found and update date

                int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measurements of your choice (no decimals).\n\n");
                //Gets input from user again if record found and update quantity

                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";
                //Updates date and quantity from user input

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        internal static string GetDateInput() //Gets input from the user for the Date.
        {
            Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to main menu.\n\n");

            string dateInput = Console.ReadLine();//asks user for date input in dd-mm-yy format
        
            if(dateInput=="0") GetUserInput();//cancels the command

            while(!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None,out _))//If date cannot be parsed as the exact format, repeat.
            {
                Console.WriteLine("\n\nInvalid date. (Format: dd-mm-yy). Type 0 to return to main menu:\n\n");
                dateInput= Console.ReadLine();
            }

            return dateInput;//return inputted user data
        }

        internal static int GetNumberInput(string message) //Gets input from the user 
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();//asks user for quantity input

            if (numberInput == "0") GetUserInput();//cancels the command

            while (!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)//if the user inputs other than a number or positive number, repeat.
            {
                Console.WriteLine("\n\nInvalid number. Try again\n\n");
                numberInput= Console.ReadLine();
            }

            int finalInput = Convert.ToInt32(numberInput);//converts the string to int

            return finalInput;//returns the input
        }

    }

    public class DrinkingWater//drinking water class
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
}

