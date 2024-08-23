using DSharpPlus.Entities;

namespace Dictobot.Services;
public sealed class DictionaryEmbedBuilderService
{
    private static DictionaryService _dictionaryService = new();
    public async Task<DiscordEmbedBuilder> GetEmbedBuilder()
    {
        return await _dictionaryService.GetEmbedBuilder();
    }
    public async Task<DiscordEmbedBuilder> GetEmbedBuilder(string date)
    {
        _dictionaryService = new(date);
        return await _dictionaryService.GetEmbedBuilder();
    }
}
