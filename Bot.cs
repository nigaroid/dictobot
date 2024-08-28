using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Dictobot.Configuration.Structures;
using Newtonsoft.Json;
using Dictobot.Services;
using Dictobot.Commands;

namespace Dictobot
{
	public class Bot
    {
        private static DiscordClient? _client;

        private static SlashCommands _slashCommands = new();

        private static ScheduleService _scheduleService = new();
        private static async Task BotInit()
		{
			await APIStructure.Shared.Load();

			var configuration = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = APIStructure.Shared.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            _client = new(configuration);

            _client.Ready += OnClientReady;
            _client.GuildAvailable += OnGuildAvailable;

            SlashCommandsInit();

			await _client!.ConnectAsync();

			await DatabaseInit();
			await ServicesInit();
        }
        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs args) => Task.CompletedTask;
        private static Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs args) => Task.CompletedTask;
        private static void SlashCommandsInit()
        {
            var commandConfig = _client.UseSlashCommands();
            commandConfig.RegisterCommands<SlashCommands>();
        }
        private static async Task ScheduleServiceInit()
        {
			await _scheduleService.SendEmbedMessageAsync(_client!);
        }
		private static async Task DatabaseInit()
		{
			await DatabaseSettingsStructure.Shared.Load();
		}
		private static async Task ServicesInit()
		{
            await ScheduleServiceInit();
		}
		private static async Task BotRun()
        {
            await Task.Delay(Timeout.Infinite);
        }
        private static async Task Main()
        {
            await BotInit();
            await BotRun();
        }
    }
}
