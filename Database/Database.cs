using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MiiBot
{
    public class Database
    {

        private static string queueDB = "Database/Queues.json";
        
        public static async Task<bool> WriteDB(Dictionary<ulong, Dictionary<string, List<string>>> queueDictionary)
        {
            try
            {
                // Create a file if it doesn't exist, or overwrite if it does
                FileStream fileStream = new FileStream(queueDB, FileMode.Create);
                using (StreamWriter writer = new StreamWriter (fileStream))
                {
                    // Convert the 'queueDictionary' object to json text
                    var json = JsonConvert.SerializeObject(queueDictionary, Formatting.Indented);

                    // Update the file with the new text
                    writer.Write(json);
                }
                fileStream.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<Dictionary<ulong, Dictionary<string, List<string>>>> ReadDB()
        {
            var queueDictionary = new Dictionary<ulong, Dictionary<string, List<string>>>();
            
            try
            {
                FileStream fileStream = new FileStream(queueDB, FileMode.Open);
                using (StreamReader reader = new StreamReader (fileStream))
                {
                    // Get the text as a string from the file
                    string text = reader.ReadToEnd();

                    // Convert json string from file to the queueDictionary type and set it to it
                    queueDictionary = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<string, List<string>>>>(text);
                }
                fileStream.Close();
                return queueDictionary;
            }
            catch (Exception)
            {   
                return queueDictionary;
            }    
        }
    }
}