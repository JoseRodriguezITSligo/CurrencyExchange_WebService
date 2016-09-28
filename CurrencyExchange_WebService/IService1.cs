using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CurrencyExchange_WebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        double ExchangeRateCalculator(string currencyFrom, string currencyTo,DateTime dateOfConvertion);



        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class Currency {
        int currencyID;
        string name;
        string abbreviation;
        string symbol;

        [DataMember]
        public int getSetCurrencyID
        {
            get { return currencyID; }
            set { currencyID = value; }
        }

        [DataMember]
        public string getSetName {
            get { return name;}
            set { name = value; }
        }

        [DataMember]
        public string getSetAbbreviation
        {
            get { return abbreviation; }
            set { abbreviation = value; }
        }

        [DataMember]
        public string getSetSymbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        //Constructor
        public Currency(int currencyID,string name, string abbreviation, string symbol) {
            this.currencyID = currencyID;
            this.name = name;
            this.abbreviation = abbreviation;
            this.symbol = symbol;
        }

    }// End of class
    //public class CompositeType
    //{
    //    bool boolValue = true;
    //    string stringValue = "Hello ";

    //    [DataMember]
    //    public bool BoolValue
    //    {
    //        get { return boolValue; }
    //        set { boolValue = value; }
    //    }

    //    [DataMember]
    //    public string StringValue
    //    {
    //        get { return stringValue; }
    //        set { stringValue = value; }
    //    }
    //}
}
