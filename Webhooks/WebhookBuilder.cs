using Dictobot.Services;
using DSharpPlus.Entities;

namespace Dictobot.Webhooks;
public static class WebhookBuilder
{
	private static DictionaryService? _dictionaryService;

	private static DiscordColor[] Colors = [DiscordColor.Green, DiscordColor.Yellow, DiscordColor.Red];
	private enum Status
	{
		Success,
		Warning,
		Error
	}
	public static DiscordWebhookBuilder RegisterCategoryFailure() => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[(int)Status.Error])
					.WithDescription("Categories cannot be registered. Please try again."));
	public static DiscordWebhookBuilder RegisterChannelExistsFailure(DiscordChannel channel) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[(int)Status.Warning])
					.WithDescription($"Channel {channel.Mention} is already registered."));
	public static DiscordWebhookBuilder DatabaseFailure() => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[(int)Status.Error])
					.WithDescription("No channels were updated. This may indicate an issue with the database."));
	public static DiscordWebhookBuilder RegisterSuccess(DiscordChannel channel) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[(int)Status.Success])
					.WithDescription($"Channel {channel.Mention} added successfully."));
	public static DiscordWebhookBuilder DeregisterChannelNotExistsFailure(DiscordChannel channel) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[(int)Status.Warning])
					.WithDescription($"Channel {channel.Mention} is not registered."));
	public static DiscordWebhookBuilder DeregisterSuccess(DiscordChannel channel) => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[(int)Status.Success])
					.WithDescription($"Channel {channel.Mention} deleted successfully."));

	public static DiscordWebhookBuilder InvalidDateOrFormat() => new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(Colors[(int)Status.Error])
					.WithDescription("Invalid date or format, please try again.\n\n" + $"Date should be within {DateTime.UtcNow.Date.Year}, and in the format YYYY-MM-DD"));
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
