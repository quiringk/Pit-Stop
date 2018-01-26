using System;

namespace PitStop
{
	public interface ILogger
	{
		void Write(String tag, String message);
	}
}

