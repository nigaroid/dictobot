using Dictobot.Additions;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Dictobot.Services
{
    public sealed class ScheduleService
    {
        private TimeSpan _scheduledTime = new(9, 0, 0);

        private static readonly DictionaryEmbedBuilderService _embedBuilderService = new();
        private TimeSpan GetDelay()
        {
            TimeSpan messageTime = _scheduledTime;
            TimeSpan currentTime = DateTime.UtcNow.TimeOfDay;

            return messageTime > currentTime ? messageTime - currentTime
                                             : new TimeSpan(1, 0, 0, 0) - currentTime + messageTime;
        }
        public async Task SendEmbedMessage(DiscordClient client)
        {
            var embed = await _embedBuilderService.GetEmbedBuilder();

            while (true)
            {
                TimeSpan delay = GetDelay();
                await Task.Delay(delay);

                if (Globals.GuildDatabase == null || Globals.GuildDatabase.Count == 0)
                    return;

                foreach (var (guildIDString, channels) in Globals.GuildDatabase)
                {
                    if (channels == null)
                    {
                        if (!ulong.TryParse(guildIDString, out ulong guildID))
                            return;

                        DiscordGuild guild = await client.GetGuildAsync(guildID);
                        var totalChannels = await guild.GetChannelsAsync();


                        if (totalChannels != null)
                        {
                            try
                            {
                                foreach (var channel in totalChannels)
                                {
                                    if (!channel.IsCategory)
                                    {
                                        await channel.SendMessageAsync(embed);
                                        Console.WriteLine($"Message sent to {channel.Name}.");
                                        return;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to send message: {ex.Message}");
                            }
                        }
                    }

                    foreach (var channel in channels!)
                    {
                        if (!channel.IsCategory)
                        {
                            try
                            {
                                await channel.SendMessageAsync(embed);
                                Console.WriteLine($"Message sent to {channel.Name}.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to send message to {channel.Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
    }
}
