using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;



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
                    return "No Errors are found";
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

                // Root JSON object: { "Hotels": { "Hotel": [ ... ] } }
                JObject rootObj = new JObject();
                JObject hotelsObj = new JObject();
                JArray hotelArray = new JArray();

                // Get all <Hotel> nodes
                XmlNodeList hotelNodes = doc.SelectNodes("//Hotel");
                foreach (XmlNode hotelNode in hotelNodes)
                {
                    // Each hotel becomes a JObject
                    JObject hotelObj = new JObject();

                    // 1) Name
                    XmlNode nameNode = hotelNode.SelectSingleNode("Name");
                    if (nameNode != null)
                    {
                        hotelObj["Name"] = nameNode.InnerText;
                    }

                    // 2) Phone (one or more)
                    JArray phoneList = new JArray();
                    XmlNodeList phoneNodes = hotelNode.SelectNodes("Phone");
                    foreach (XmlNode p in phoneNodes)
                    {
                        phoneList.Add(p.InnerText);
                    }
                    hotelObj["Phone"] = phoneList;

                    // 3) Address
                    XmlNode addressNode = hotelNode.SelectSingleNode("Address");
                    JObject addressObj = new JObject();
                    if (addressNode != null)
                    {
                        // Sub-elements: Number, Street, City, State, Zip
                        if (addressNode["Number"] != null)
                            addressObj["Number"] = addressNode["Number"].InnerText;
                        if (addressNode["Street"] != null)
                            addressObj["Street"] = addressNode["Street"].InnerText;
                        if (addressNode["City"] != null)
                            addressObj["City"] = addressNode["City"].InnerText;
                        if (addressNode["State"] != null)
                            addressObj["State"] = addressNode["State"].InnerText;
                        if (addressNode["Zip"] != null)
                            addressObj["Zip"] = addressNode["Zip"].InnerText;

                        // Optional attribute => _NearestAirport
                        XmlAttribute nearestAirportAttr = addressNode.Attributes?["NearestAirport"];
                        if (nearestAirportAttr != null)
                        {
                            addressObj["_NearestAirport"] = nearestAirportAttr.Value;
                        }
                    }
                    hotelObj["Address"] = addressObj;

                    // 4) Optional Rating attribute => _Rating
                    XmlAttribute ratingAttr = hotelNode.Attributes?["Rating"];
                    if (ratingAttr != null)
                    {
                        hotelObj["_Rating"] = ratingAttr.Value;
                    }

                    // Add this hotel to the array
                    hotelArray.Add(hotelObj);
                }

                // Build the final JSON structure
                hotelsObj["Hotel"] = hotelArray;
                rootObj["Hotels"] = hotelsObj;

                // Return a nicely formatted JSON string
                return rootObj.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception ex)
            {
                return "Exception: " + ex.Message;
            }
        }
    }

}
