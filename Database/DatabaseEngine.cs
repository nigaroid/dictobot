using Dictobot.Configuration.Structures;
using Microsoft.Win32;
using Npgsql;

namespace Dictobot.Database;
public class DatabaseEngine
{
	private string? _tableNameAbsolute;

	private string? _connectionString;

	public DatabaseEngine()
	{
		_connectionString = $"Host={DatabaseSettingsStructure.Data?.Host};Port={DatabaseSettingsStructure.Data?.Port};Username={DatabaseSettingsStructure.Data?.Username};Password={DatabaseSettingsStructure.Data?.Password};Database={DatabaseSettingsStructure.Data?.DatabaseName}";

		_tableNameAbsolute = $"{DatabaseSettingsStructure.Data?.DatabaseName}.{DatabaseSettingsStructure.Data?.SchemaName}.{DatabaseSettingsStructure.Data?.TableName}";
	}

	private async Task<long> TotalGuild()
	{
		using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync();

		string? query = $"SELECT COUNT(*) FROM {_tableNameAbsolute};";

		using var cmd = new NpgsqlCommand(query, conn);
		var userCount = await cmd.ExecuteScalarAsync();
		return Convert.ToInt64(userCount);
	}

	private async Task<bool> GuildExists(DGuild guild)
	{
		using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync();

		string? query = $"SELECT 1 FROM {_tableNameAbsolute} WHERE guild_id='{guild.GuildId}';";

		using var cmd = new NpgsqlCommand(query, conn);
		var str = await cmd.ExecuteScalarAsync();
		return str != null;
	}

	public async Task<bool> StoreGuildAsync(DGuild guild, string serverId)
	{
		long registryKey = await TotalGuild() + 1;

		if (registryKey == -1)
			return false;

		if (await GuildExists(guild) || guild.GuildId != serverId)
			return false;

		using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync();

		string? query = $"INSERT INTO {_tableNameAbsolute} VALUES({registryKey}, '{guild.GuildId}', '{guild.ServerName}', NULL);";

		using var cmd = new NpgsqlCommand(query, conn);
		await cmd.ExecuteNonQueryAsync();
		return true;
	}

	public async Task<bool> RemoveGuildAsync(DGuild guild, string serverId)
	{
		using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync();

		if (!await GuildExists(guild) || guild.GuildId != serverId)
			return false;

		string? query = $"DELETE FROM {_tableNameAbsolute} WHERE guild_id='{guild.GuildId}'";

		using var cmd = new NpgsqlCommand(query, conn);
		await cmd.ExecuteNonQueryAsync();
		return true;
	}

	public async Task<bool> ChannelExistsAsync(DGuild guild, string channelId)
	{
		using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync();

		string? query = $"SELECT 1 FROM {_tableNameAbsolute} WHERE guild_id=\'{guild.GuildId}\' AND \'{channelId}\'=ANY(channels);";

		using var cmd = new NpgsqlCommand(query, conn);
		var str = await cmd.ExecuteScalarAsync();
		return str != null;
	}

	public async Task<bool> RegisterChannelsByGuildAsync(DGuild guild, string channelId)
	{
		using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync();

		if (!await GuildExists(guild))
			await StoreGuildAsync(guild, guild.GuildId!);

		string query = $"UPDATE {_tableNameAbsolute} SET channels=ARRAY_APPEND(channels, \'{channelId}\') WHERE guild_id=\'{guild.GuildId!}\'";

		using var cmd = new NpgsqlCommand(query, conn);
		return await cmd.ExecuteNonQueryAsync() <= 0 ? false : true;
	}

	public async Task<bool> DeregisterChannelsByGuildAsync(DGuild guild, string channelId)
	{
		using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync();

		string query = $"UPDATE {_tableNameAbsolute} SET channels=ARRAY_REMOVE(channels, \'{channelId}\') WHERE guild_id=\'{guild.GuildId}\' AND channels IS NOT NULL";

		using var cmd = new NpgsqlCommand(query, conn);
		return await cmd.ExecuteNonQueryAsync() <= 0 ? false : true;
	}

	public async IAsyncEnumerable<string?> GetGuildsIdAsync()
	{
		using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync();

		string query = $"SELECT guild_id FROM {_tableNameAbsolute};";

		using var cmd = new NpgsqlCommand(query, conn);
		using var reader = await cmd.ExecuteReaderAsync();
		if (!reader.HasRows)
			yield return null;

		while (await reader.ReadAsync())
		{
			var guildId = reader.GetString(0);
			if (!string.IsNullOrEmpty(guildId))
				yield return guildId;
		}
	}

	public async Task<List<string>?> GetChannelsIdAsync(DGuild guild)
	{
		using var conn = new NpgsqlConnection(_connectionString);
		await conn.OpenAsync();

		string query = $"SELECT channels FROM {_tableNameAbsolute} WHERE guild_id=\'{guild.GuildId}\'";

		using var cmd = new NpgsqlCommand(query, conn);
		using var reader = await cmd.ExecuteReaderAsync();
		if (!reader.HasRows)
			return null;

		while (await reader.ReadAsync())
		{
			var channelsId = reader.GetFieldValue<List<string>>(0);
			return channelsId;
		}
		return null;
	}
}
