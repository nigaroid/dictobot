using Dictobot.Database;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Dictobot.Additions;
public static class Globals
{
	private static readonly DatabaseEngine _databaseEngineService = new();
	public static async Task GuildDatabaseInitAsync(DiscordClient client)
	{
		await foreach (var guildID in _databaseEngineService.GetGuildIDsAsync())
		{
			List<DiscordChannel> channels = new();

			var channelIDs = await _databaseEngineService.GetGuildChannelIDsAsync(guildID);

			foreach (var channelID in channelIDs!)
				if (!string.IsNullOrEmpty(channelID))
				{
					if (!ulong.TryParse(guildID, out ulong parsedGuildID) ||
						!ulong.TryParse(channelID, out ulong parsedChannelID))
						return;

					var guild = await client.GetGuildAsync(parsedGuildID);
					var channel = await client.GetChannelAsync(parsedChannelID);

					if (channel == null)
						continue;

					channels.Add(channel);
				}
		}
	}
}
