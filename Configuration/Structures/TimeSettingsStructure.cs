using Dictobot.Configuration.Structures;
using Newtonsoft.Json;
public sealed class TimeSettingsStructure : JsonReader<TimeSettingsStructure>
{
	private static readonly Lazy<TimeSettingsStructure> _sharedInstance = new(() => new());

	protected override string FileName => "schedule-time.json";

	public static TimeSettingsStructure Shared => _sharedInstance.Value;

	[JsonProperty("time")]
	public string? Time { get; private set; }

	public async Task SetTimeSettingsAsync(TimeOnly time)
	{
		Time = time.ToString("HH:mm:ss");
		var json = JsonConvert.SerializeObject(new TimeSettingsStructure() { Time = Time }, Formatting.Indented);
		await File.WriteAllTextAsync(FileName!, json);
	}
}

