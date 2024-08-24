using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Dictobot.Additions;
using Dictobot.Configuration;
using Dictobot.Services;

namespace Dictobot
{
    public class Bot
    {
        private static DiscordClient? _client;

        private static SlashCommandsService _commandService = new();

        private static ScheduleService _scheduleService = new();
        private static async Task BotInit()
        {
            APIStructure.InitializeAsync().GetAwaiter().GetResult();

            var configuration = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = JSONReader<APIStructure>.Data?.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            _client = new DiscordClient(configuration);

            _client.Ready += OnClientReady;
            _client.GuildAvailable += OnGuildAvailable;

            CommandServiceInit();

            await _client!.ConnectAsync();

            await Globals.GuildDatabaseInitAsync(_client!);
            await ScheduleServiceInit();
        }
        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs args) => Task.CompletedTask;
        private static Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs args) => Task.CompletedTask;
        private static void CommandServiceInit()
        {
            var commandConfig = _client.UseSlashCommands();
            commandConfig.RegisterCommands<SlashCommandsService>();
        }
        private static async Task ScheduleServiceInit()
        {
            await _scheduleService.SendEmbedMessageAsync(_client!);
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
