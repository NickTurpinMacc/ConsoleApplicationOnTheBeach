using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
//using System.Data.SqlServerCe.SqlCeEngine;

namespace ConsoleApplicationOnTheBeach
{
    class Program
    {       
        static string strConsoleInput = "";
        //Readonly strings for the SQLcommands.
        //Normally I would write these as stored procedures (for reusability), but as I cannot access the database server on my PC 
        //to write stored procedures, I have dad to use embedded SQL strings instead.
        static readonly string strEmployeeSalaryDetailsCommandText = "SELECT E.name, " +
                  "C.unit as local_Currency, S.annual_amount, S.annual_amount / C.conversion_factor as annual_salary_GBP " +
                  "FROM Employees E, Salaries S, Currencies C " +
                  "Where E.ID = S.employee_id " +
                  "and S.currency = C.id " +
                  "and E.name = '"; //The employee name is concatenated onto the end dynamically in the code.

        static readonly string strAllEmployeeSalaryDetailsCommandText = "SELECT E.name, " +
                  "C.unit as local_Currency, S.annual_amount, S.annual_amount / C.conversion_factor as annual_salary_GBP " +
                  "FROM Employees E, Salaries S, Currencies C, Roles R " +
                  "Where E.ID = S.employee_id " +
                  "and S.currency = C.id " +
                  "and E.role_id = R.id " +
                  "and R.name = 'Staff' " +
                  "order by annual_salary_GBP desc ";
          
        static void Main(string[] args)
        {

            Console.WriteLine("Do you want to dispay All staff Salaries? Y or N");

            //Convert to uppercase to allow for 'Y' and 'y'.
            strConsoleInput = Console.ReadLine().ToUpper();

            if ((strConsoleInput == "Y"))
            {
                displaySalaryDetails(strAllEmployeeSalaryDetailsCommandText);
                
                Console.WriteLine("Press any key to close.");
                Console.ReadKey();
            }
            else
            {
                singleEmployeeSalaryDetails();
            }

        }

        static void singleEmployeeSalaryDetails()
        {
            //Calls the method to display the Employee Salary Details.
            //After each time ask the user if they wish to continue, if no the console Window is closed.
            //It gets the employee name from the user, formats the SQL Command,
            //then calls the common method to get and display the data.
            
            Boolean BLGetEmployeeDetails = true;
            string StrCommandText = "";

            while (BLGetEmployeeDetails)
            {
                Console.WriteLine("Enter Employee Name (case sensitive)");
                
                strConsoleInput = Console.ReadLine();

                StrCommandText = strEmployeeSalaryDetailsCommandText + strConsoleInput + "'";

                displaySalaryDetails(StrCommandText);  
                
                Console.WriteLine("Do you wish to continue? Y or N");
                
                //Convert to uppercase to allow for 'Y' and 'y'.
                strConsoleInput = Console.ReadLine().ToUpper();

                if ((strConsoleInput != "Y"))
                {
                    BLGetEmployeeDetails = false;
                }

            } 
        }

               
        static void displaySalaryDetails(string _StrCommandText)
        {
            //Common method for calling the database and processing the returned data
            //using the passed in Command Text.

            string StrConnStringCI = "Data Source = OnTheBeach.sdf; Persist Security Info=False";
            SqlCeConnection conn = null;
            Boolean BLRecordFound = false;

            try
            {
                //The database engine may need updating to the current version.
                updateDatabaseEngine(StrConnStringCI);

                conn = new SqlCeConnection(StrConnStringCI);
                conn.Open();

                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = _StrCommandText;
                SqlCeDataReader myReader = null;
                myReader = cmd.ExecuteReader();


                while (myReader.Read())
                {
                    BLRecordFound = true;
                    //Extract the data from the Reader, using the column names.
                    string StrEmployeeName = (myReader["Name"].ToString());

                    string StrLocalCurrency = (myReader["local_Currency"].ToString());

                    string StrLocalSalary = (myReader["annual_amount"].ToString());

                    string StrGBPSalary = (myReader["annual_salary_GBP"].ToString());

                    double DblGBPSalary = Math.Round(Convert.ToDouble(StrGBPSalary), 2);
                    
                    //Format output neatly back to the Console.
                    Console.WriteLine(" ");
                    Console.WriteLine("Employee Name - {0}", StrEmployeeName);
                    Console.WriteLine("Local Currency - {0}", StrLocalCurrency);
                    Console.WriteLine("Local Salary - {0}", StrLocalSalary);
                    Console.WriteLine("GBP Salary - £{0}", DblGBPSalary.ToString());
                    Console.WriteLine(" ");
                                                     
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
            }
            finally
            {
                conn.Close();
            }
            //Output a message if no employee was found.
            if (BLRecordFound == false)
            {
                Console.WriteLine("Employee not found.");
            }
        }


        static void updateDatabaseEngine(string _StrConnStringCI)
        {
            //Code for upgrading the database to the correct version, when required.
            //First time through it needs to be updated. But after that it doesn't.
            
            string StrConnStringCS = "Data Source = OnTheBeach.sdf; Case Sensitive=true";

            try
            {
                SqlCeEngine engine = new SqlCeEngine(_StrConnStringCI);
                // The collation of the database will be case sensitive because of 
                // the new connection string used by the Upgrade method.                
                engine.Upgrade(StrConnStringCS);
            }
            catch
            {
                //Nothing to catch.
            }
        }

    }//End of Class
}
