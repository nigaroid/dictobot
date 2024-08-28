using Newtonsoft.Json;

namespace Dictobot.Configuration.Structures
{
	public sealed class DatabaseSettingsStructure : JSONReader<DatabaseSettingsStructure>
	{
		private static readonly Lazy<DatabaseSettingsStructure> _sharedInstance = new(() => new());

		protected override string FileName => "database-config.json";

		public static DatabaseSettingsStructure Shared => _sharedInstance.Value;

		[JsonProperty("username")]
		public string? Username { get; set; }

		[JsonProperty("password")]
		public string? Password { get; set; }

		[JsonProperty("host")]
		public string? Host { get; set; }

		[JsonProperty("port")]
		public string? Port { get; set; }

		[JsonProperty("database_name")]
		public string? DatabaseName { get; set; }

		[JsonProperty("schema_name")]
		public string? SchemaName { get; set; }

		[JsonProperty("table_name")]
		public string? TableName { get; set; }
	}
}
