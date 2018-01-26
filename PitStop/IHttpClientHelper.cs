using System;
using System.Net.Http;

namespace PitStop
{
	public interface IHttpClientHelper
	{
		HttpMessageHandler MessageHandler { get; }
	}
}

