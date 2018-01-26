using System;

namespace PitStop
{
	public class Logger
	{
		static ILogger log;
		public static void init(ILogger logger)
		{
			log = logger;
		}

		public static void info(string tag, string message)
		{
			log.Write ("kq-" + tag, message);
		}
	}
}

