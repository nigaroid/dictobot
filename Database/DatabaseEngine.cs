using Dictobot.Configuration.Structures;
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
	private async Task<long> GetTotalGuild()
	{
		using (var conn = new NpgsqlConnection(_connectionString))
		{
			await conn.OpenAsync();

			string? query = $"SELECT COUNT(*) FROM {_tableNameAbsolute};";

			using (var cmd = new NpgsqlCommand(query, conn))
			{
				var userCount = await cmd.ExecuteScalarAsync();
				return Convert.ToInt64(userCount);
			}
		}
	}
	private async Task<bool> GuildExists(DGuild guild)
	{
		using (var conn = new NpgsqlConnection(_connectionString))
		{
			await conn.OpenAsync();

			string? query = $"SELECT 1 FROM {_tableNameAbsolute} WHERE guild_id='{guild.GuildID}';";

			using (var cmd = new NpgsqlCommand(query, conn))
			{
				var str = await cmd.ExecuteScalarAsync();
				return str != null;
			}
		}
	}
	private async Task<bool> StoreGuild(DGuild guild)
	{
		long guildRegistryNo = await GetTotalGuild() + 1;

		if (guildRegistryNo == -1)
			return false;

		using (var conn = new NpgsqlConnection(_connectionString))
		{
			await conn.OpenAsync();

			string? query = $"INSERT INTO {_tableNameAbsolute} VALUES({guildRegistryNo}, '{guild.GuildID}', '{guild.ServerName}', NULL);";

			using (var cmd = new NpgsqlCommand(query, conn))
			{
				await cmd.ExecuteNonQueryAsync();
				return true;
			}
		}
	}
	public async Task<bool> ChannelExistsAsync(DGuild guild, string channelID)
	{
		using (var conn = new NpgsqlConnection(_connectionString))
		{
			await conn.OpenAsync();

			string? query = $"SELECT 1 FROM {_tableNameAbsolute} WHERE guild_id=\'{guild.GuildID}\' AND \'{channelID}\'=ANY(channels);";

			using (var cmd = new NpgsqlCommand(query, conn))
			{
				var str = await cmd.ExecuteScalarAsync();
				return str != null;
			}
		}
	}
	public async Task<bool> RegisterGuildChannelsAsync(DGuild guild, string channelID)
	{
		using (var conn = new NpgsqlConnection(_connectionString))
		{
			await conn.OpenAsync();

			if (!await GuildExists(guild))
				await StoreGuild(guild);

			string query = $"UPDATE {_tableNameAbsolute} SET channels=ARRAY_APPEND(channels, \'{channelID}\') WHERE guild_id=\'{guild.GuildID!}\'";

			using (var cmd = new NpgsqlCommand(query, conn))
			{
				if (await cmd.ExecuteNonQueryAsync() <= 0)
					return false;
			}
			return true;
		}
	}
	public async Task<bool> DeregisterGuildChannelsAsync(DGuild guild, string channelID)
	{
		using (var conn = new NpgsqlConnection(_connectionString))
		{
			await conn.OpenAsync();

			string query = $"UPDATE {_tableNameAbsolute} SET channels=ARRAY_REMOVE(channels, \'{channelID}\') WHERE guild_id=\'{guild.GuildID}\' AND channels IS NOT NULL";

			using (var cmd = new NpgsqlCommand(query, conn))
			{
				if (await cmd.ExecuteNonQueryAsync() <= 0)
					return false;
			}
			return true;
		}
	}
	public async IAsyncEnumerable<string> GetGuildIDsAsync()
	{
		using (var conn = new NpgsqlConnection(_connectionString))
		{
			await conn.OpenAsync();

			string query = $"SELECT guild_id FROM {_tableNameAbsolute};";

			using (var cmd = new NpgsqlCommand(query, conn))
			{
				using (var reader = await cmd.ExecuteReaderAsync())
				{
					if (!reader.HasRows)
						yield return default!;

					while (await reader.ReadAsync())
					{
						var guildID = reader.GetString(0);
						if (!string.IsNullOrEmpty(guildID))
							yield return guildID;
					}
				}
			}
		}
	}
	public async Task<List<string>?> GetGuildChannelIDsAsync(string guildID)
	{
		using (var conn = new NpgsqlConnection(_connectionString))
		{
			await conn.OpenAsync();

			string query = $"SELECT channels FROM {_tableNameAbsolute} WHERE guild_id=\'{guildID}\'";

			using (var cmd = new NpgsqlCommand(query, conn))
			{
				using (var reader = await cmd.ExecuteReaderAsync())
				{
					if (!reader.HasRows)
						return default!;

					while (await reader.ReadAsync())
					{
						var channelIDs = reader.GetFieldValue<List<string>>(0);
						return channelIDs;
					}
				}
				return default;
			}
		}
	}
}
