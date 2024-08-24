using Dictobot.Additions;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Dictobot.Services
{
    public sealed class ScheduleService
    {
        private static TimeSpan _scheduledTime = new(9, 0, 0);

        private static readonly DictionaryEmbedBuilderService _embedBuilderService = new();
        private TimeSpan GetDelay()
        {
            TimeSpan messageTime = _scheduledTime;
            TimeSpan currentTime = DateTime.UtcNow.TimeOfDay;

            return messageTime > currentTime ? messageTime - currentTime
                                             : new TimeSpan(1, 0, 0, 0) - currentTime + messageTime;
        }
		private async Task<DiscordChannel?> GetDefaultChannelAsync(string guildIDString, DiscordClient client)
		{
			if (!ulong.TryParse(guildIDString, out ulong guildID))
				return null;

			DiscordGuild guild = await client.GetGuildAsync(guildID);
			var totalChannels = await guild.GetChannelsAsync();

			if (totalChannels == null)
				return null;

			return totalChannels.FirstOrDefault(x => x != null && !x.IsCategory);
		}
		private async Task SendMessageToChannels(DiscordClient client, DiscordEmbed embed)
		{
			foreach (var (guildIDString, channels) in Globals.GuildDatabase!)
			{
				if (channels == null)
				{
					var defaultChannel = await GetDefaultChannelAsync(guildIDString, client);

					if (defaultChannel != null)
						await defaultChannel.SendMessageAsync(embed);
				}
				else
				{
					foreach (var channel in channels!)
						await channel.SendMessageAsync(embed);
				}
			}
		}
		public async Task SendEmbedMessage(DiscordClient client)
        {
            var embed = await _embedBuilderService.GetEmbedBuilder();

            while (true)
            {
                TimeSpan delay = GetDelay();
                await Task.Delay(delay);

                if (!Globals.GuildDatabase!.Any())
                    return;

				await SendMessageToChannels(client, embed);
			}
        }
    }
}
