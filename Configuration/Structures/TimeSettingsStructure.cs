using Dictobot.Configuration.Structures;
using DSharpPlus;
using Newtonsoft.Json;
public sealed class TimeSettingsStructure : JSONReader<TimeSettingsStructure>
{
	private static readonly Lazy<TimeSettingsStructure> _sharedInstance = new(() => new());
	protected override string FileName => "schedule-time.json";
	public static TimeSettingsStructure Shared => _sharedInstance.Value;

	[JsonProperty("time")]
	public string? Time { get; set; } = "03:33:00";
	private TimeSettingsStructure() { }
	public async Task UpdateTimeSettingsAsync(TimeOnly time)
	{
		string parsedTime = time.ToString("HH:mm:ss");
		var json = JsonConvert.SerializeObject(new TimeSettingsStructure() { Time = parsedTime }, Formatting.Indented);
		Console.WriteLine($"Serialized JSON:\n\n{json}");

		if (json == "null")
			return;

		Time = parsedTime;
		await File.WriteAllTextAsync(FileName!, json);

		string jsonString = await File.ReadAllTextAsync(FileName);
		Console.WriteLine($"Reading the file ...\n\n{jsonString}");
	}
}

