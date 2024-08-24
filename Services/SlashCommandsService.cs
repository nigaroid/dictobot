using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Dictobot.Database;
using Dictobot.Configuration;

namespace Dictobot.Services
{
    public sealed class SlashCommandsService : ApplicationCommandModule
    {
        private static readonly DictionaryEmbedBuilderService _embedBuilderService = new();

        private static readonly DictionaryService _dictionaryService = new();

		private static readonly DatabaseEngine _databaseEngine = new();

        [SlashCommand("wotd", "Send the word of the day.")]
        public async Task SendWOTD(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var embedBuilder = await _embedBuilderService!.GetEmbedBuilder();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
        }

        [SlashCommand("wotdof", "Send the word of the day.")]
        public async Task SendWOTDByDate(InteractionContext ctx, [Option("date", "Date within the current year, in the format YYYY-MM-DD")] string date)
        {
            await ctx.DeferAsync();

            if (DateValidationService.ValidateDate(date))
            {
                var embedBuilder = _embedBuilderService!.GetEmbedBuilder(date);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder.Result));
            }
            else
            {
                var errorMessage = "Invalid date or format, please try again.\n\n" + $"Date should be within {DateTime.UtcNow.Date.Year}, and in the format YYYY-MM-DD";
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(errorMessage)));
            }
        }

        [SlashCommand("register", "Register a channel within the server to recieve a daily message")]
        public async Task RegisterChannel(InteractionContext ctx, [Option("channel", "channel tag (can't be a category)")] DiscordChannel channel)
        {
            await ctx.DeferAsync();

            if (channel.IsCategory)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithTitle("Error")
                    .WithDescription("Categories cannot be registered. Please try again.")));
                return;
            }

            var guild = new DGuild
            {
                GuildID = ctx.Guild.Id.ToString(),
                ServerName = ctx.Guild.Name
            };

            string channelID = channel.Id.ToString();
            if (await _databaseEngine.ChannelExistsAsync(guild.GuildID, channelID))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Yellow)
                    .WithDescription($"Channel {channel.Mention} is already registered.")));
                return;
            }

            if (!await _databaseEngine.RegisterGuildChannelsAsync(guild, channelID))
            {
				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Red)
					.WithTitle("Error")
					.WithDescription("No channels were updated. This may indicate an issue with the database.")));
				return;
			}
			await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Green)
					.WithDescription($"Channel {channel.Mention} added successfully.")));
        }

        [SlashCommand("deregister", "Input a channel ID you want to deregister the bot into.")]
        public async Task DeregisterChannel(InteractionContext ctx, [Option("channel", "channel ID within the server")] DiscordChannel channel)
        {
            await ctx.DeferAsync();

            var guild = new DGuild
            {
                GuildID = ctx.Guild.Id.ToString(),
                ServerName = ctx.Guild.Name
            };

            string channelID = channel.Id.ToString();

            if (!await _databaseEngine.ChannelExistsAsync(guild.GuildID, channelID))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Yellow)
                    .WithDescription($"Channel {channel.Mention} is not registered.")));
                return;
            }

			if (!await _databaseEngine.DeregisterGuildChannelsAsync(guild, channelID))
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Red)
					.WithTitle("Error")
					.WithDescription("No channels were updated. This may indicate an issue with the database.")));
				return;
			}
			await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Green)
					.WithDescription($"Channel {channel.Mention} deleted successfully.")));
        }

        /*[SlashCommand("setschedule", "Set a new schedule time.")]
        public async Task SetScheduleCommand(InteractionContext ctx, [Option("time", "New scheduled time in HH:MM format")] string time)
        {
            // undone
        }*/
    }
}
