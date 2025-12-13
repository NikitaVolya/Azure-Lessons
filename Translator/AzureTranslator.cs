using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Translator.Types;


namespace Translator
{
    public class AzureTranslator
    {
        private string _key;
        private string _endpoint;
        private readonly string _region;
        private readonly string _route;

        public AzureTranslator(string key, string endpoint, string region, string route)
        {
            _key = key;
            _endpoint = endpoint;
            _region = region;
            _route = route;
        }

        public async Task<TranslationResult[]> TranslateTextAsync(string inputText)
        {
            TranslationResult[] translationResults;

            object body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new System.Uri(_endpoint + _route);

                request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

                request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", _region);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                string result = await response.Content.ReadAsStringAsync();

                translationResults = JsonConvert.DeserializeObject<TranslationResult[]>(result);
            }

            return translationResults;
        }
    }
}
