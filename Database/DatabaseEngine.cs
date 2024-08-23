using Dictobot.Configuration;
using Npgsql;
namespace Dictobot.Database;
public class DatabaseEngine
{
    private string? _tableNameAbsolute;

    private string? _connectionString;
    public DatabaseEngine() 
    {
        DatabaseSettingsStructure.InitializeAsync().GetAwaiter().GetResult();

        _connectionString = $"Host={JSONReader<DatabaseSettingsStructure>.Data?.Host};Port={JSONReader<DatabaseSettingsStructure>.Data?.Port};Username={JSONReader<DatabaseSettingsStructure>.Data?.Username};Password={JSONReader<DatabaseSettingsStructure>.Data?.Password};Database={JSONReader<DatabaseSettingsStructure>.Data?.DatabaseName}";

        _tableNameAbsolute = $"{JSONReader<DatabaseSettingsStructure>.Data?.DatabaseName}.{JSONReader<DatabaseSettingsStructure>.Data?.SchemaName}.{JSONReader<DatabaseSettingsStructure>.Data?.TableName}";
    }
    private async Task<long> GetTotalGuildAsync()
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
    private async Task<bool> GuildExistsAsync(string serverID)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            string? query = $"SELECT 1 FROM {_tableNameAbsolute} WHERE guild_id='{serverID}';";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                var str = await cmd.ExecuteScalarAsync();
                return str != null;
            }
        }
    }
    private async Task<bool> StoreGuildAsync(DGuild guild)
    {
        long guild_no = await GetTotalGuildAsync() + 1;

        if (guild_no == -1)
            return false;

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            string? query = $"INSERT INTO {_tableNameAbsolute} VALUES({guild_no}, '{guild.GuildID}', '{guild.ServerName}', NULL);";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
        }
    }
    public async Task<bool> ChannelExistsAsync(string guildID, string channelID)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            string? query = $"SELECT 1 FROM {_tableNameAbsolute} WHERE guild_id=\'{guildID}\' AND \'{channelID}\'=ANY(channels);";

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

            if (!await GuildExistsAsync(guild.GuildID!))
            {
                await StoreGuildAsync(guild);
                Console.WriteLine($"Created a guild with ID: {guild.GuildID!}");
            }

            string query = $"UPDATE {_tableNameAbsolute} SET channels=ARRAY_APPEND(channels, \'{channelID}\') WHERE guild_id=\'{guild.GuildID!}\'";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
				if (await cmd.ExecuteNonQueryAsync() <= 0)
					return false;
			}
        }
		return true;
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
        }
		return true;
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
                    if (reader.HasRows)
					{
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
    }
    public async IAsyncEnumerable<string?> GetGuildChannelsAsync(string guildID)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = $"SELECT channels FROM {_tableNameAbsolute} WHERE guild_id=\'{guildID}\'";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
						if (reader.IsDBNull(0))
							yield return null;

                        var channelIDs = reader.GetFieldValue<List<string>>(0);
                        foreach (var channelID in channelIDs)
                            if (!string.IsNullOrEmpty(channelID))
                                yield return channelID;
                    }
                }
            }
        }
    }
}
