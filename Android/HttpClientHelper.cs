using System;
using System.Net.Http;
using ModernHttpClient;

namespace PitStop.Droid
{
	public class HttpClientHelper : IHttpClientHelper
	{
		private HttpMessageHandler handler;
		public HttpMessageHandler MessageHandler
		{
			get { return handler ?? (handler = new NativeMessageHandler()); }
		}
	}
}

