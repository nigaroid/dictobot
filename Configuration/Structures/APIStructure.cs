using Newtonsoft.Json;

namespace Dictobot.Configuration.Structures
{
	public sealed class APIStructure : JSONReader<APIStructure>
	{
		private static readonly Lazy<APIStructure> _sharedInstance = new(() => new());

		protected override string FileName => "api-config.json";

		public static APIStructure Shared => _sharedInstance.Value;

		[JsonProperty("token")]
		public string? Token { get; set; }
	}
}
