using DSharpPlus.Entities;

namespace Dictobot.Additions;
public interface IEmbedBuilder
{
    public Task<DiscordEmbedBuilder> GetEmbedBuilder();
}