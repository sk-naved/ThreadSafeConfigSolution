using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ThreadSafeConfigService
{
	// 1. The Model
	public class AppConfig
	{
		public string ConnectionString { get; set; }
		public DateTime LastUpdated { get; set; }
	}

	// 2. The Thread-Safe Service
	public class ConfigService
	{
		private AppConfig _cachedConfig;
		private DateTime _nextExpiry = DateTime.MinValue;
		private readonly System.Threading.SemaphoreSlim _semaphore = new System.Threading.SemaphoreSlim(1, 1);
		private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(5); // Short for testing

		public async Task<AppConfig> GetConfigurationAsync(int userId)
		{
			// Fast Path: Valid cache
			if (_cachedConfig != null && DateTime.UtcNow < _nextExpiry)
			{
				Console.WriteLine($"[User {userId}] Retrieved from Cache.");
				return _cachedConfig;
			}

			// Slow Path: Need Lock
			Console.WriteLine($"[User {userId}] Cache expired/null. Waiting for lock...");
			await _semaphore.WaitAsync();

			try
			{
				// Double Check
				if (_cachedConfig != null && DateTime.UtcNow < _nextExpiry)
				{
					Console.WriteLine($"[User {userId}] Lock acquired, but cache was already updated by another thread.");
					return _cachedConfig;
				}

				// Simulate DB Call
				Console.WriteLine($"[User {userId}] *** CALLING DATABASE ***");
				await Task.Delay(1000); // Simulate network latency

				_cachedConfig = new AppConfig
				{
					ConnectionString = "Server=MS_PROD_SQL;Database=testDb;",
					LastUpdated = DateTime.UtcNow
				};
				_nextExpiry = DateTime.UtcNow.Add(_cacheDuration);

				return _cachedConfig;
			}
			finally
			{
				_semaphore.Release();
			}
		}
	}

	// 3. Execution Logic
	class Program
	{
		static async Task Main(string[] args)
		{
			var service = new ConfigService();
			Console.WriteLine("--- Starting High Concurrency Test ---");

			// Simulate 10 users hitting the service at the EXACT same time
			var tasks = new List<Task>();
			for (int i = 1; i <= 10; i++)
			{
				int userId = i;
				tasks.Add(Task.Run(async () => {
					var config = await service.GetConfigurationAsync(userId);
				}));
			}

			await Task.WhenAll(tasks);

			Console.WriteLine("\n--- Test Complete. Observe that 'CALLING DATABASE' only appeared ONCE. ---");
			Console.ReadLine();
		}
	}
}