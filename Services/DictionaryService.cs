using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using HtmlAgilityPack;

namespace Dictobot.Services
{
	public sealed class DictionaryService
	{
        private readonly HttpClient _httpClient = new();

		private readonly string _url = "https://www.merriam-webster.com/word-of-the-day";

		public string? UrlAbsolute { get; private set; }

		private bool IsDictionaryObject(string input) => !input.Contains("See the entry");

		public DictionaryService()
		{
			UrlAbsolute = $"{_url}/{$"{DateTime.UtcNow.Year.ToString("00")}-" +
									$"{DateTime.UtcNow.Month.ToString("00")}-" +
									$"{DateTime.UtcNow.Day.ToString("00")}"}";
		}

		public DictionaryService(string date)
        {
			UrlAbsolute = $"{_url}/{date}";
        }

		private async Task<HtmlDocument> GetResponseHtml()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(UrlAbsolute);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            HtmlDocument document = new();
            document.LoadHtml(responseBody);
            return document;
        }

		public async Task<DiscordWebhookBuilder> GetWebhookBuilderAsync()
        {
            var objects = new List<string>();
            await foreach (var @object in GetObjectsAsync())
                objects.Add(@object);

            return Webhooks.WebhookBuilder.WotdMessage(objects);
        }

		public async IAsyncEnumerable<string?> GetObjectsAsync()
        {
            HtmlDocument document = await GetResponseHtml();

            var wotd		= document.DocumentNode.SelectSingleNode("//div[contains(@class, 'word-and-pronunciation')]/h2");
            var description = document.DocumentNode.SelectNodes("//div[contains(@class, 'wod-definition-container')]/p");
            var attributes	= document.DocumentNode.SelectNodes("//div[contains(@class, 'word-attributes')]");
            var dateInfo	= document.DocumentNode.SelectNodes("//div[contains(@class, 'w-a-title')]")[0];

			if (wotd == null || description == null || attributes == null || dateInfo == null)
				yield return null;

			foreach (var attribute in attributes!)
			{
				yield return wotd?.InnerText.Trim().ToUpper()[0] + wotd!.InnerText.Trim().Substring(1);
				yield return Regex.Replace(attribute.InnerText, @"\s+", " ").Trim().Split().ToList()[0].ToString();
				yield return Regex.Replace(attribute.InnerText, @"\s+", " ").Trim().Split().ToList()[1].ToString();
				yield return string.Join("\n\n", description!.Select(x => x.InnerText.Trim()).Where(x => IsDictionaryObject(x))).ToString();
				yield return Regex.Replace(dateInfo!.InnerText.Split('\n')[2].ToString(), @"\s+", " ").Trim().Split(':')[1].ToString();
			}
        }
    }
}
