using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CurrencyExchange_WebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
        
    {       
        //Connection string used to stablish conneciton with the WebService database
        string connectionString = "Data Source=MILLENIUMHAWCK\\HDIP;Initial Catalog=WebService_DB;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //Static variable to define start date which will be allocated as day 0 for all exchange currency polinomial models
        // This date will be used to calculate the number of days between that date and the date passed in as parameter of the WebService
        private static DateTime initialDate = new DateTime(2016, 05, 18);

        //Definition of method to calculate exchange rate by passing in two currency names and date of convertion
        public  double ExchangeRateCalculator(string currencyFrom, string currencyTo, DateTime dateOfConvertion) {
            double exchangeRate = 0;
            Currency currObjFrom;
            Currency currObjTo;
            
            //Get a list of currency stored in the database
            List<Currency> currencyList = new List<Currency>();

            //Invoke method to get list of currencies from DB and save it into list above 
            currencyList = GetCurrencyList();
            //Check that both currencies are  stored in the database and store the currency in a Currency object for later workout. Show error if one of them is not valid
            if (IsCurrencyInDB(currencyFrom, currencyList,out currObjFrom))
            {
                //Check that second currency does exist in database
                if (IsCurrencyInDB(currencyTo, currencyList,out currObjTo)) {
                    /*Send new query to database in order to get the realitionship record for currencyFrom and currencyTo.
                     As a relationship can be expressed in terms of one currency or the other(for instance: USD/EUR or EUR/USD), only one relationship
                     is strored in the database and in case the other relationsip is required it will be enough to calculate the 
                     exchange rate with the relationship stored in the database and then calculate the inverse of the result.*/

                    exchangeRate = CalculateExchangeRate(currObjFrom, currObjTo,dateOfConvertion);

                }
                else {
                    //Return an error message
                }
            }
            else {
                //Return an error message
            }
           
            //

            

            return exchangeRate;
        }


        #region HELPER METHODS
        //Method to create a list of Currency objects.  These objects are created by using 
        //details coming from the Currency Table in the WebService_DB database

        private List<Currency> GetCurrencyList()
        {
            List<Currency> currencyList = new List<Currency>();
            int currencyID;
            string name;
            string abbreviation;
            string symbol;
            // Connect to database
            // Sqlconnection object
            SqlConnection connection = new SqlConnection(connectionString);
            // Sqlcommand object
            SqlCommand cmd = new SqlCommand();

            try
            {
                //Open the connection to WebService database
                connection.Open();
                //Configure the connection for the command object
                cmd.Connection = connection;
                //Establish the query to be sent to the database and execute the query
                cmd.CommandText = "SELECT * FROM Currency";
                //4 sqlDataReader object
                SqlDataReader dataRetrieved;
                dataRetrieved = cmd.ExecuteReader();

                while (dataRetrieved.Read())
                {
                    currencyID = Convert.ToInt32(dataRetrieved["CurrencyID"].ToString());
                    name = dataRetrieved["Name"].ToString().Trim();
                    abbreviation = dataRetrieved["Abbreviation"].ToString();
                    symbol = dataRetrieved["Symbol"].ToString();
                    Currency currency = new Currency(currencyID, name, abbreviation, symbol);
                    currencyList.Add(currency);
                }//End of while loop to read the result set and save currency names into the local list
                return currencyList;
            }// End of try block
            catch { return null; }
            finally
            {
                //Close connection to dabase
                connection.Close();
            }
        }//End of DB connection method


        //Method to check if one currency is in the list by providing its name. If the currency is stored in the 
        //database, the corresponding Currency object will be stores in the currObjFrom or currObjTo so they
        //can be used later on.
        private bool IsCurrencyInDB(string currency,List<Currency> currencyList, out Currency currencyFound) {
            //Define variables to be used during the method
            int i = 0; // Counter
            bool found = false;// Boolean flag to advertise the currency was found
            Currency currencyTemp;// Currency object to referency different items in the currencyList
            currencyFound = null;
            //Check the list is not null
            if (currencyList != null)
            {   //Check while the counter is less the list size or the name is not found
                while (i < currencyList.Count && !found)
                {   //Point the temporal object to current item in the list
                    currencyTemp = currencyList.ElementAt(i);
                    if (currencyTemp != null)
                    {
                        //Check name property against string passed in
                        if (currencyTemp.getSetName.Trim().ToLower().Equals(currency.Trim().ToLower()))
                        {
                            //If names mathc, Currency object parameter passed by reference is assigned to current item in list and 
                            //boolean flag is set to true
                            currencyFound = currencyTemp;
                            found = true;
                        }//Otherwise, parameter passed by reference is set to null and booleang flag to false
                        else
                        {
                            currencyFound = null;
                            found = false;
                        }// End if else statement to check if current currency item name is equal to the currency name to look for
                        //Increase counter regardless the item name is equal or not to searched name
                        i++;
                    }//End of while loop to go through currencyList
                }
            }// End of if statement to check the currency is not null
            else
            {
                currencyFound = null;
                found = false;
            }// End of if statement to check the list is not null
            return found;
         } // End IsCurrencyInDB method

        /* Method to look for a relationship record between two currencies passed in as arguments.
	       As a relationship can be expressed in terms of one currency or the other(for instance: USD/EUR or EUR/USD), only one relationship
	       is strored in the database and in case the other relationsip is required it will be enough to calculate the 
	       exchange rate with the relationship stored in the database and then calculate the inverse of the result.*/

        private double CalculateExchangeRate(Currency currFrom, Currency currTo,DateTime dateOfConvertion)
        {
            double exchangeRate=0;// value to be retrieved. If no match is found zero is returned
            double A;
            double B;
            double C;
            double D;
            //connect to database
            //2 sqlconnection object
            SqlConnection connection = new SqlConnection(connectionString);
            //3 sqlcommand object
            SqlCommand cmd = new SqlCommand();

            try
            {
                //Open the connection to WebService database
                connection.Open();
                //Configure the connection for the command object
                cmd.Connection = connection;
                //Establish the query to be sent to the database and execute the query
                //This sql query will rertieve the polynomial used to define the exchange rate between currFrom and currTo.
                // If the sotred relationship function is in the same order as required (currencyTo/currencyFrom) the sql below must be used
                cmd.CommandText = "spGetPolynomial";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@CurrencyFrom", currFrom.getSetCurrencyID));
                cmd.Parameters.Add(new SqlParameter("@CurrencyTo", currTo.getSetCurrencyID));
                // sqlDataReader object
                SqlDataReader polynomialCoeficients;
                polynomialCoeficients = cmd.ExecuteReader();
                if (polynomialCoeficients.Read())
                {
                    //Get the coeficients to calculate the exchange rate
                    A = (double)polynomialCoeficients["CoeficientA"];
                    B = (double)polynomialCoeficients["CoeficientB"];
                    C = (double)polynomialCoeficients["CoeficientC"];
                    D = (double)polynomialCoeficients["CoeficientD"];
                    //Calculate the exchange rate
                    exchangeRate = CalculatePolynomial(A, B, C, D,dateOfConvertion);

                }
                else
                {
                    //Close connection previous to dabase
                    connection.Close();
                    //Open the connection to WebService database
                    connection.Open();
                    //Configure the connection for the command object
                    cmd.Connection = connection;
                    /*If no data was retrieved the sql query has to be redifined and sent back to the Database to check if relationship
                      is stored in the inverse mode (currencyFrom/currencyTo). Keep in mind if this is the case the inverse of result must 
                      be calculated before returning the currencyExchange variable.*/
                    cmd.CommandText = "spGetPolynomial";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@CurrencyFrom", currTo.getSetCurrencyID));
                    cmd.Parameters.Add(new SqlParameter("@CurrencyTo", currFrom.getSetCurrencyID));
                    polynomialCoeficients = cmd.ExecuteReader();
                    if (polynomialCoeficients.Read())
                    {
                        A = (double)polynomialCoeficients["CoeficientA"];
                        B = (double)polynomialCoeficients["CoeficientB"];
                        C = (double)polynomialCoeficients["CoeficientC"];
                        D = (double)polynomialCoeficients["CoeficientD"];
                        //Calculate the inverse of the exchange rate for relationship (currFrom/currTo)
                        exchangeRate = 1 / CalculatePolynomial(A, B, C, D,dateOfConvertion);
                    }//End of if statement that checks relatonship order currencyFrom/currencyTo
                }// End of if statement that checks relatonship order currencyTo/currencyFrom
                return exchangeRate;
            }//End of try block
            catch { return 0; }
            finally
            {
                //Close connection to dabase
                connection.Close();
            }
        }//End of DB connection method

        //Method to calculate a polynomial based on the coeficients passed in. Up to degree three polynomial can be calculated!
        private double CalculatePolynomial(double A, double B, double C, double D,DateTime dateOfConveriton)
        {
            double result;
            DateTime initialDate = Service1.initialDate;
            int days = (int) (dateOfConveriton - initialDate).TotalDays;
            return result = A * Math.Pow(days, 3) + B * Math.Pow(days, 2) + C * days + D;
        }//End of method to calculate a polynomial
        
        #endregion

       
    }//End of IService1 class
}// End of namespace
