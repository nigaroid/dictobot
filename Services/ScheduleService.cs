using DSharpPlus.Entities;
using DSharpPlus;
using Dictobot.Database;
public sealed class ScheduleService
{
	private static readonly DatabaseEngine _databaseEngine = new();

	private async Task<TimeSpan> GetDelay()
	{
		await TimeSettingsStructure.Shared.Load();

		TimeOnly messageTime = TimeOnly.Parse(TimeSettingsStructure.Data?.Time!);
		TimeOnly currentTime = TimeOnly.FromTimeSpan(DateTime.UtcNow.TimeOfDay);

		return messageTime > currentTime ? messageTime - currentTime
										 : new TimeSpan(1, 0, 0, 0) - (currentTime.ToTimeSpan() - messageTime.ToTimeSpan());
	}

	private async Task<DiscordChannel?> GetDefaultChannel(string guildID, DiscordClient client)
	{
		if (!ulong.TryParse(guildID, out ulong parsedGuildID))
			return null;

		var guild	 = await client.GetGuildAsync(parsedGuildID);
		var channels = await guild.GetChannelsAsync();

		return channels == null ? null : channels.FirstOrDefault(x => !x.IsCategory);
	}

	private async Task SendMessageToChannels(DiscordClient client, DiscordWebhookBuilder webhookBuilder)
	{
		var databaseEngine = new DatabaseEngine();

		await foreach (var guildID in databaseEngine.GetGuildsIdAsync())
		{
			var channelIDs = await databaseEngine.GetChannelsIdAsync(new() { GuildId = guildID });

			if (channelIDs == null)
			{
				var defaultChannel = await GetDefaultChannel(guildID, client);
				if (defaultChannel == null)
					return;

				await defaultChannel.SendMessageAsync(webhookBuilder.Embeds[0]);
			}
			else
			{
				foreach (var channelID in channelIDs)
					if (ulong.TryParse(channelID, out ulong parsedChannelID))
					{
						var channel = await client.GetChannelAsync(parsedChannelID);
						await channel.SendMessageAsync(webhookBuilder.Embeds[0]);
					}
			}
		}
	}

	public async Task SendEmbedMessageAsync(DiscordClient client)
	{
		var webhookBuilder = await Dictobot.Webhooks.WebhookBuilder.GetDictionaryWebhookBuilderAsync();

		while (true)
		{
			var delay = await GetDelay();
			await Task.Delay(delay);
			await SendMessageToChannels(client, webhookBuilder);
		}
	}
}
