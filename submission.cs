using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;



/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/


namespace ConsoleApp1
{


    public class Program
    {
        public static string xmlURL = "https://lewine.github.io/Project3/Hotels.xml";
        public static string xmlErrorURL = "https://lewine.github.io/Project3/HotelsErrors.xml";
        public static string xsdURL = "https://lewine.github.io/Project3/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);


            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);


            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", xsdUrl);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas = schemas;
                settings.ValidationType = ValidationType.Schema;

                System.Text.StringBuilder errors = new System.Text.StringBuilder();
                settings.ValidationEventHandler += (sender, args) =>
                {
                    errors.AppendLine(args.Message);
                };

                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) { }
                }

                if (errors.Length == 0)
                {
                    return "No Error";
                }
                else
                {
                    return errors.ToString();
                }
            }
            catch (Exception ex)
            {
                return "Exception: " + ex.Message;
            }
        }

        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlUrl);

                var hotelList = new List<dynamic>();

                XmlNodeList hotelNodes = doc.SelectNodes("//Hotel");
                if (hotelNodes == null) return "{}"; 

                foreach (XmlNode hotelNode in hotelNodes)
                {
                    dynamic hotelObj = new ExpandoObject();
                    var hotelDict = (IDictionary<string, object>)hotelObj;

                    var nameNode = hotelNode.SelectSingleNode("Name");
                    if (nameNode != null)
                        hotelDict["Name"] = nameNode.InnerText;

                    var phoneNodes = hotelNode.SelectNodes("Phone");
                    var phoneList = new List<string>();
                    foreach (XmlNode p in phoneNodes)
                    {
                        phoneList.Add(p.InnerText);
                    }
                    hotelDict["Phone"] = phoneList;

                    var addressNode = hotelNode.SelectSingleNode("Address");
                    dynamic addressObj = new ExpandoObject();
                    var addressDict = (IDictionary<string, object>)addressObj;

                    if (addressNode != null)
                    {
                        addressDict["Number"] = addressNode["Number"]?.InnerText;
                        addressDict["Street"] = addressNode["Street"]?.InnerText;
                        addressDict["City"]   = addressNode["City"]?.InnerText;
                        addressDict["State"]  = addressNode["State"]?.InnerText;
                        addressDict["Zip"]    = addressNode["Zip"]?.InnerText;

                        var nearestAirportAttr = addressNode.Attributes?["NearestAirport"];
                        if (nearestAirportAttr != null)
                        {
                            addressDict["_NearestAirport"] = nearestAirportAttr.Value;
                        }
                    }
                    hotelDict["Address"] = addressObj;

                    var ratingAttr = hotelNode.Attributes?["Rating"];
                    if (ratingAttr != null)
                    {
                        hotelDict["_Rating"] = ratingAttr.Value;
                    }

                    hotelList.Add(hotelObj);
                }

                dynamic rootObj = new ExpandoObject();
                rootObj.Hotels = new ExpandoObject();
                ((IDictionary<string, object>)rootObj.Hotels)["Hotel"] = hotelList;

                string jsonText = JsonConvert.SerializeObject(rootObj, Formatting.Indented);
                return jsonText;
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
    }

}
