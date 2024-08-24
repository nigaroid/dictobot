using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using HtmlAgilityPack;

namespace Dictobot.Services
{
    public sealed class DictionaryService
    {
        private string _date = $"{DateTime.UtcNow.Year.ToString("00")}-{DateTime.UtcNow.Month.ToString("00")}-{DateTime.UtcNow.Day.ToString("00")}";

        private string _url = "https://www.merriam-webster.com/word-of-the-day";

        private static readonly HttpClient _httpClient = new();
		private bool IsDictionaryObject(string input) => !input.Contains("See the entry");
		public DictionaryService() { }
        public DictionaryService(string date)
        {
            _date = date;
            _url = $"{_url}/{_date}";
        }
        private async Task<HtmlDocument> GetResponseHtmlAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            HtmlDocument document = new();
            document.LoadHtml(responseBody);
            return document;
        }
        public async Task<DiscordEmbedBuilder> GetEmbedBuilderAsync()
        {
            var objects = new List<string>();
            await foreach (var @object in GetObjectsAsync())
                objects.Add(@object);

            return new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Blurple)
                .WithTitle($"{objects[0]}\t[{objects[2]}]\t({objects[1]})\n\nWhat It Means?\n\n")
                .WithDescription($"{objects[3]}\n\n")
                .WithFooter($"{objects[4]}");
        }
        public async IAsyncEnumerable<string> GetObjectsAsync()
        {
            HtmlDocument document = await GetResponseHtmlAsync();

            var wotd = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'word-and-pronunciation')]/h2");
            var description = document.DocumentNode.SelectNodes("//div[contains(@class, 'wod-definition-container')]/p");
            var attributes = document.DocumentNode.SelectNodes("//div[contains(@class, 'word-attributes')]");
            var dateInfo = document.DocumentNode.SelectNodes("//div[contains(@class, 'w-a-title')]")[0];

            if (wotd != null && description != null && attributes != null && dateInfo != null)
			{
				foreach (var attribute in attributes)
				{
					yield return wotd?.InnerText.Trim().ToUpper()[0] + wotd!.InnerText.Trim().Substring(1);
					yield return Regex.Replace(attribute.InnerText, @"\s+", " ").Trim().Split().ToList()[0].ToString();
					yield return Regex.Replace(attribute.InnerText, @"\s+", " ").Trim().Split().ToList()[1].ToString();
					yield return string.Join("\n\n", description.Select(x => x.InnerText.Trim()).Where(x => IsDictionaryObject(x))).ToString();
					yield return Regex.Replace(dateInfo.InnerText.Split('\n')[2].ToString(), @"\s+", " ").Trim().Split(':')[1].ToString();
				}
			}
        }
    }
}
