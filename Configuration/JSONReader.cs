using Newtonsoft.Json;

namespace Dictobot.Configuration
{
    public abstract class JSONReader<T> where T : new()
    {
        public static T? Data { get; private set; }
        protected static async Task Read(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                string json = await sr.ReadToEndAsync();
                Data = JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}
