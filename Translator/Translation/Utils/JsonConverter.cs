using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Translation.Utils
{
    public static class JsonConverter
    {
        /// <summary>
        /// Method to Convert Object to JsonString
        /// </summary>
        /// <param name="sender">Takes in an Object</param>
        /// <returns>Json string representing the object</returns>
        public static async Task<string> ReturnJsonStringFromObject(object sender)
        {
            return await Task.Run(() => JsonConvert.SerializeObject(sender, settings: new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
        }

        /// <summary>
        /// Method to Convert JsonString to a specified Object
        /// </summary>
        /// <typeparam name="T">Takes in Object Type</typeparam>
        /// <param name="jsonString">Takes in JsonString representing an Object</param>
        /// <returns>Object of the specified Type</returns>
        public static async Task<T> ReturnObjectFromJsonString<T>(string jsonString)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(jsonString));
        }

        /// <summary>
        /// Serializer Settings
        /// </summary>
        static JsonSerializerSettings SerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };
            }
        }

        /// <summary>
        /// Method to create and return HttpClient
        /// String Content
        /// </summary>
        /// <returns>StringContent</returns>
        public static async Task<StringContent> ReturnStringContent(object obj)
        {
            var jsonString = await ReturnJsonStringFromObject(obj);

            return new StringContent(jsonString, Encoding.UTF8, "application/json");
        }

        public static async Task<T> DuplicateObject<T>(object sender)
        {
            var objectOneJson = await ReturnJsonStringFromObject(sender);
            return await ReturnObjectFromJsonString<T>(objectOneJson);
        }
    }
}
