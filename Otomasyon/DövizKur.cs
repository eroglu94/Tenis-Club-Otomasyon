using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Otomasyon
{
    class DövizKur
    {
        string today = "http://www.tcmb.gov.tr/kurlar/today.xml";

        public string USD()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(today);
            DateTime exchangeDate = Convert.ToDateTime(xmlDoc.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);
            string USD = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='USD']/BanknoteSelling").InnerXml;
            return (string.Format("USD: {0}",USD));
        }

        public string USD_Date()
        {
            
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(today);
            DateTime exchangeDate = Convert.ToDateTime(xmlDoc.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);
            string USD = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='USD']/BanknoteSelling").InnerXml;
            return (string.Format("Tarih {0} || USD     : {1}", exchangeDate.ToShortDateString(), USD));
        }

        public string EURO()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(today);
            DateTime exchangeDate = Convert.ToDateTime(xmlDoc.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);
            string EURO = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='EUR']/BanknoteSelling").InnerXml;
            return (string.Format("EURO: {0}",EURO));
        }

        public string EURO_Date()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(today);
            DateTime exchangeDate = Convert.ToDateTime(xmlDoc.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);
            string EURO = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='EUR']/BanknoteSelling").InnerXml;
            return (string.Format("Tarih {0} || EURO   : {1}", exchangeDate.ToShortDateString(), EURO));
        }
            

        public string POUND()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(today);
            DateTime exchangeDate = Convert.ToDateTime(xmlDoc.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);
            string POUND = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='GBP']/BanknoteSelling").InnerXml;
            return (string.Format("POUND: {0}",POUND));
        }

        public string POUND_Date()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(today);
            DateTime exchangeDate = Convert.ToDateTime(xmlDoc.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);
            string POUND = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='GBP']/BanknoteSelling").InnerXml;
            return (string.Format("Tarih {0} || POUND: {1}", exchangeDate.ToShortDateString(), POUND));
        }

        
    }
}




//// Bugün (en son iş gününe) e ait döviz kurları için
//string today = "http://www.tcmb.gov.tr/kurlar/today.xml";

//// 14 Şubat 2013 e ait döviz kurları için
////string anyDays = "http://www.tcmb.gov.tr/kurlar/201302/14022013.xml";

//var xmlDoc = new XmlDocument();
//xmlDoc.Load(today);

//            // Xml içinden tarihi alma - gerekli olabilir
//            DateTime exchangeDate = Convert.ToDateTime(xmlDoc.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);

//string USD = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='USD']/BanknoteSelling").InnerXml;
//string EURO = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='EUR']/BanknoteSelling").InnerXml;
//string POUND = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='GBP']/BanknoteSelling").InnerXml;

//Console.WriteLine(string.Format("Tarih {0} USD   : {1}", exchangeDate.ToShortDateString(), USD));
//            Console.WriteLine(string.Format("Tarih {0} EURO  : {1}", exchangeDate.ToShortDateString(), EURO));
//            Console.WriteLine(string.Format("Tarih {0} POUND : {1}", exchangeDate.ToShortDateString(), POUND));