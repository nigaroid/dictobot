using Dictobot.Additions;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Dictobot.Services
{
    public sealed class ScheduleService
    {
        private static TimeSpan _scheduledTime = new(9, 0, 0);
        private TimeSpan GetDelay()
        {
            TimeSpan messageTime = _scheduledTime;
            TimeSpan currentTime = DateTime.UtcNow.TimeOfDay;

            return messageTime > currentTime ? messageTime - currentTime
                                             : new TimeSpan(1, 0, 0, 0) - currentTime + messageTime;
        }
		private async Task<DiscordChannel?> GetDefaultChannel(string guildIDString, DiscordClient client)
		{
			if (!ulong.TryParse(guildIDString, out ulong guildID))
				return null;

			DiscordGuild guild = await client.GetGuildAsync(guildID);
			var totalChannels = await guild.GetChannelsAsync();

			if (totalChannels == null)
				return null;

			return totalChannels.FirstOrDefault(x => x != null && !x.IsCategory);
		}
		private async Task SendMessageToChannels(DiscordClient client, DiscordWebhookBuilder webhookBuilder)
		{
			foreach (var (guildIDString, channels) in Globals.GuildDatabase!)
			{
				if (channels == null)
				{
					var defaultChannel = await GetDefaultChannel(guildIDString, client);

					if (defaultChannel != null)
						await defaultChannel.SendMessageAsync(webhookBuilder.Embeds[0]);
				}
				else
				{
					foreach (var channel in channels!)
						await channel.SendMessageAsync(webhookBuilder.Embeds[0]);
				}
			}
		}
		public async Task SendEmbedMessageAsync(DiscordClient client)
        {
            var webhookBuilder = await Webhooks.WebhookBuilder.GetDictionaryWebhookBuilderAsync();

            while (true)
            {
                TimeSpan delay = GetDelay();
                await Task.Delay(delay);

                if (!Globals.GuildDatabase!.Any())
                    return;

				await SendMessageToChannels(client, webhookBuilder);
			}
        }
    }
}
