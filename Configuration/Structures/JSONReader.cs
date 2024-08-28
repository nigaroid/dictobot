using Newtonsoft.Json;

namespace Dictobot.Configuration.Structures
{
	public abstract class JSONReader<T> where T : class
	{
		public static T? Data { get; protected set; }

		protected abstract string FileName { get; }

		private async Task<T?> Initialize()
		{
			using (StreamReader sr = new StreamReader(FileName))
			{
				var json = await sr.ReadToEndAsync();
				return JsonConvert.DeserializeObject<T>(json);
			}
		}

		public virtual async Task Load()
		{
			var data = await Initialize();
			Data = data;
		}
	}
}
