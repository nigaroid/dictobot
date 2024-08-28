using Dictobot.Services;
using DSharpPlus.Entities;

namespace Dictobot.Webhooks;
public static class WebhookBuilder
{
	private static DictionaryService? _dictionaryService;

	private static DiscordColor[] Colors => [DiscordColor.Green, DiscordColor.Yellow, DiscordColor.Red];

	private static class Status
	{
		public const int Success = 0;
		public const int Warning = 1;
		public const int Failure = 2;
	}

	public static DiscordWebhookBuilder RegisterCategoryFailure() => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[Status.Failure])
					.WithDescription("Categories cannot be registered. Please try again."));

	public static DiscordWebhookBuilder RegisterChannelExistsFailure(DiscordChannel channel) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[Status.Warning])
					.WithDescription($"Channel {channel.Mention} is already registered."));

	public static DiscordWebhookBuilder DatabaseFailure() => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[Status.Failure])
					.WithDescription("No channels were updated. This may indicate an issue with the database."));

	public static DiscordWebhookBuilder RegisterSuccess(DiscordChannel channel) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[Status.Success])
					.WithDescription($"Channel {channel.Mention} added successfully."));

	public static DiscordWebhookBuilder DeregisterChannelNotExistsFailure(DiscordChannel channel) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[Status.Warning])
					.WithDescription($"Channel {channel.Mention} is not registered."));

	public static DiscordWebhookBuilder DeregisterSuccess(DiscordChannel channel) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[Status.Success])
					.WithDescription($"Channel {channel.Mention} deleted successfully."));

	public static DiscordWebhookBuilder InvalidDateOrFormat() => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[Status.Failure])
					.WithDescription("Invalid date or format, please try again.\n\n" + $"Date should be within {DateTime.UtcNow.Date.Year}, and in the format YYYY-MM-DD"));

	public static DiscordWebhookBuilder TimeOnlyParseFailure() => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[Status.Failure])
					.WithDescription("Please provide the time in hh:mm:ss format."));

	public static DiscordWebhookBuilder ParseSuccess(string time) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[Status.Success])
					.WithDescription($"The new time is set to {time}."));

	public static DiscordWebhookBuilder WOTDMessage(List<string> objects) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Blurple)
					.WithTitle($"{objects[0]}\t[{objects[2]}]\t({objects[1]})\n\nWhat It Means?\n\n")
					.WithDescription($"{objects[3]}\n\n")
					.WithFooter($"{objects[4]}"));

	public static async Task<DiscordWebhookBuilder> GetDictionaryWebhookBuilderAsync()
	{
		_dictionaryService = new();
		return await _dictionaryService.GetWebhookBuilderAsync();
	}

	public static async Task<DiscordWebhookBuilder> GetDictionaryWebhookBuilderAsync(string date)
	{
		_dictionaryService = new(date);
		return await _dictionaryService.GetWebhookBuilderAsync();
	}
}
