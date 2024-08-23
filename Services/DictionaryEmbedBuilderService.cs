using DSharpPlus.Entities;

namespace Dictobot.Services;
public sealed class DictionaryEmbedBuilderService
{
    private static DictionaryService? _dictionaryService;
    public async Task<DiscordEmbedBuilder> GetEmbedBuilder()
    {
		_dictionaryService = new();
        return await _dictionaryService.GetEmbedBuilderAsync();
    }
    public async Task<DiscordEmbedBuilder> GetEmbedBuilder(string date)
    {
        _dictionaryService = new(date);
        return await _dictionaryService.GetEmbedBuilderAsync();
    }
}
