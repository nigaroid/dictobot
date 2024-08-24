using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Dictobot.Database;
using Dictobot.Configuration;

namespace Dictobot.Services
{
    public sealed class SlashCommandsService : ApplicationCommandModule
    {
        private static readonly DictionaryService _dictionaryService = new();

		private static readonly DatabaseEngine _databaseEngine = new();

        [SlashCommand("wotd", "Send the word of the day.")]
        public async Task SendWOTD(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var embedBuilder = await Webhooks.WebhookBuilder.GetDictionaryWebhookBuilderAsync();
            await ctx.EditResponseAsync(embedBuilder);
        }

        [SlashCommand("wotdof", "Send the word of the day.")]
        public async Task SendWOTDByDate(InteractionContext ctx, [Option("date", "Date within the current year, in the format YYYY-MM-DD")] string date)
        {
            await ctx.DeferAsync();

            if (DateValidationService.ValidateDate(date))
            {
                var embedBuilder = await Webhooks.WebhookBuilder.GetDictionaryWebhookBuilderAsync(date);
                await ctx.EditResponseAsync(embedBuilder);
            }
            else
            {
                await ctx.EditResponseAsync(Webhooks.WebhookBuilder.InvalidDateOrFormat());
            }
        }

        [SlashCommand("register", "Register a channel within the server to recieve a daily message")]
        public async Task RegisterChannel(InteractionContext ctx, [Option("channel", "channel tag (can't be a category)")] DiscordChannel channel)
        {
            await ctx.DeferAsync();

            if (channel.IsCategory)
            {
                await ctx.EditResponseAsync(Webhooks.WebhookBuilder.RegisterCategoryFailure());
                return;
            }

            var guild = new DGuild
            {
                GuildID = ctx.Guild.Id.ToString(),
                ServerName = ctx.Guild.Name
            };

            string channelID = channel.Id.ToString();
            if (await _databaseEngine.ChannelExistsAsync(guild, channelID))
            {
                await ctx.EditResponseAsync(Webhooks.WebhookBuilder.RegisterChannelExistsFailure(channel));
                return;
            }

            if (!await _databaseEngine.RegisterGuildChannelsAsync(guild, channelID))
            {
				await ctx.EditResponseAsync(Webhooks.WebhookBuilder.DatabaseFailure());
				return;
			}
			await ctx.EditResponseAsync(Webhooks.WebhookBuilder.RegisterSuccess(channel));
        }

        [SlashCommand("deregister", "Input a channel ID you want to deregister the bot into.")]
        public async Task DeregisterChannel(InteractionContext ctx, [Option("channel", "channel ID within the server")] DiscordChannel channel)
        {
            await ctx.DeferAsync();

            var guild = new DGuild
            {
                GuildID = ctx.Guild.Id.ToString(),
                ServerName = ctx.Guild.Name,
            };

            string channelID = channel.Id.ToString();

            if (!await _databaseEngine.ChannelExistsAsync(guild, channelID))
            {
                await ctx.EditResponseAsync(Webhooks.WebhookBuilder.DeregisterChannelNotExistsFailure(channel));
                return;
            }

			if (!await _databaseEngine.DeregisterGuildChannelsAsync(guild, channelID))
			{
				await ctx.EditResponseAsync(Webhooks.WebhookBuilder.DatabaseFailure());
				return;
			}
			await ctx.EditResponseAsync(Webhooks.WebhookBuilder.DeregisterSuccess(channel));
        }

        /*[SlashCommand("setschedule", "Set a new schedule time.")]
        public async Task SetScheduleCommand(InteractionContext ctx, [Option("time", "New scheduled time in HH:MM format")] string time)
        {
            // undone
        }*/
    }
}
