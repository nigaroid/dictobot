using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Dictobot.Database;
using Dictobot.Services;

namespace Dictobot.Commands
{
	public sealed class SlashCommands : ApplicationCommandModule
	{
		private static readonly DictionaryService _dictionaryService = new();

		private static readonly DatabaseEngine _databaseEngine = new();

		private static readonly ScheduleService _scheduleService = new();

		[SlashCommand("wotd", "Send the word of the day.")]
		public async Task SendWotd(InteractionContext ctx)
		{
			await ctx.DeferAsync();
			var embedBuilder = await Webhooks.WebhookBuilder.GetDictionaryWebhookBuilderAsync();
			await ctx.EditResponseAsync(embedBuilder);
		}

		[SlashCommand("wotdof", "Send the word of the day.")]
		public async Task SendWotdByDate(InteractionContext ctx, [Option("date", "date within the current year as yyyy-mm-dd")] string date)
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

		[SlashCommand("register", "Register a channel within the server to recieve a daily message.")]
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
				GuildId = ctx.Guild.Id.ToString(),
				ServerName = ctx.Guild.Name
			};

			string channelId = channel.Id.ToString();

			if (await _databaseEngine.ChannelExistsAsync(guild, channelId))
			{
				await ctx.EditResponseAsync(Webhooks.WebhookBuilder.RegisterChannelExistsFailure(channel));
				return;
			}

			if (!await _databaseEngine.RegisterChannelsAsync(guild, channelId))
			{
				await ctx.EditResponseAsync(Webhooks.WebhookBuilder.DatabaseFailure());
				return;
			}

			await ctx.EditResponseAsync(Webhooks.WebhookBuilder.RegisterSuccess(channel));
		}

		[SlashCommand("deregister", "Input a channel ID you want to deregister the bot into.")]
		public async Task DeregisterChannel(InteractionContext ctx, [Option("channel", "channel tag within the server")] DiscordChannel channel)
		{
			await ctx.DeferAsync();

			var guild = new DGuild
			{
				GuildId = ctx.Guild.Id.ToString(),
				ServerName = ctx.Guild.Name,
			};

			string channelId = channel.Id.ToString();
			if (!await _databaseEngine.ChannelExistsAsync(guild, channelId))
			{
				await ctx.EditResponseAsync(Webhooks.WebhookBuilder.DeregisterChannelNotExistsFailure(channel));
				return;
			}

			if (!await _databaseEngine.DeregisterChannelsAsync(guild, channelId))
			{
				await ctx.EditResponseAsync(Webhooks.WebhookBuilder.DatabaseFailure());
				return;
			}

			await ctx.EditResponseAsync(Webhooks.WebhookBuilder.DeregisterSuccess(channel));
		}

		[SlashCommand("setschedule", "Set a new schedule time.")]
		public async Task UpdateScheduledTime(InteractionContext ctx, [Option("time", "scheduled time as hh:mm:ss in 24-hour format")] string time)
		{
			await ctx.DeferAsync();

			if (!TimeOnly.TryParseExact(time, "HH:mm:ss", out TimeOnly parsedTime))
			{
				await ctx.EditResponseAsync(Webhooks.WebhookBuilder.TimeOnlyParseFailure());
				return;
			}

			await TimeSettingsStructure.Shared.SetTimeSettingsAsync(parsedTime);
			await TimeSettingsStructure.Shared.Load();
			
			await ctx.EditResponseAsync(Webhooks.WebhookBuilder.ParseSuccess(TimeSettingsStructure.Data?.Time!));
		}
	}
}
