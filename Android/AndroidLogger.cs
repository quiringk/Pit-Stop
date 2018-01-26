using System;
using Android.Util;

namespace PitStop.Droid
{
	public class AndroidLogger : ILogger
	{
		void ILogger.Write(string tag, string message)
		{
			Log.Info(tag, message);
		}
	}
}

