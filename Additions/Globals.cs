using Dictobot.Database;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Dictobot.Additions;
public static class Globals
{
    private static readonly DatabaseEngine _databaseEngineService = new();

    public static readonly Dictionary<string, List<DiscordChannel>?>? GuildDatabase = new();
    public static async Task GuildDatabaseInit(DiscordClient client)
    {
        await foreach (var guildIDString in _databaseEngineService.GetGuildIDs())
        {
            List<DiscordChannel> channels = new();
            await foreach (var channelIDString in _databaseEngineService.GetGuildChannelsAsync(guildIDString))
            {
                if (!string.IsNullOrEmpty(channelIDString))
                {
                    if (!ulong.TryParse(guildIDString, out ulong guildID) ||
                        !ulong.TryParse(channelIDString, out ulong channelID))
                        return;

                    var guild = await client.GetGuildAsync(guildID);
                    var channel = await client.GetChannelAsync(channelID);

                    if (channel == null)
                        continue;

                    channels.Add(channel);
                }
            }
            GuildDatabase!.Add(guildIDString, channels.Any() ? channels : null);
        }
    }
}
