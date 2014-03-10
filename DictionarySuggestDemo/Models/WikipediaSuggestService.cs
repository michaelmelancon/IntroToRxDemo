using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DictionarySuggestDemo.Models
{
    public class WikipediaSuggestService : ISuggestService
    {
        public async Task<string[]> GetSuggestions(string text)
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync(
                string.Format(
                    "http://en.wikipedia.org/w/api.php?action=opensearch&format=json&search={0}",
                    Uri.EscapeUriString(text)
                )
            );

            var parsedResponse = (JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(response);

            return parsedResponse[1].ToArray().Select(value => value.ToString()).ToArray();
        }
    }
}
