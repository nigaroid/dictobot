using DSharpPlus.Entities;
using DSharpPlus;
using Dictobot.Database;
public sealed class ScheduleService
{
	private static TimeSpan _scheduledTime = new(11, 57, 0);

	private readonly DatabaseEngine _databaseEngine = new();
	private TimeSpan GetDelay()
	{
		TimeSpan messageTime = _scheduledTime;
		TimeSpan currentTime = DateTime.UtcNow.TimeOfDay;

		return messageTime > currentTime ? messageTime - currentTime
										 : new TimeSpan(1, 0, 0, 0) - currentTime + messageTime;
	}
	private async Task<DiscordChannel?> GetDefaultChannel(string guildID, DiscordClient client)
	{
		if (!ulong.TryParse(guildID, out ulong ulonGuildID))
			return null;

		var guild = await client.GetGuildAsync(ulonGuildID);
		var totalChannels = await guild.GetChannelsAsync();

		if (totalChannels == null)
			return null;

		return totalChannels.FirstOrDefault(x => !x.IsCategory);
	}
	private async Task SendMessageToChannels(DiscordClient client, DiscordWebhookBuilder webhookBuilder)
	{
		await foreach (var guildID in _databaseEngine.GetGuildIDsAsync())
		{
			var channels = await _databaseEngine.GetGuildChannelIDsAsync(guildID);

			if (channels == null)
			{
				var defaultChannel = await GetDefaultChannel(guildID, client);

				if (defaultChannel != null)
					await defaultChannel.SendMessageAsync(webhookBuilder.Embeds[0]);
			}
			else
			{
				foreach (var channelID in channels)
					if (ulong.TryParse(channelID, out ulong ulongChannelID))
					{
						var channel = await client.GetChannelAsync(ulongChannelID);
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
			await Task.Delay(GetDelay());
			await SendMessageToChannels(client, webhookBuilder);
		}
	}
}
