using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using PCLCrypto;
using ModernHttpClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Validation;
using Newtonsoft.Json.Linq;

namespace PitStop
{
	public class Yelp
	{
		static string consumerKey = "";
		static string consumerSecret = "";
		static string token = "";
		static string tokenSecret = "";

		public static void SearchForPlaces()
		{
			
			OAuthBase oAuth = new OAuthBase ();

			string outNormalisedUrl = "";
			string outNormalisedRequestParameters = "";

			string nonce = oAuth.GenerateNonce ();
			string timeStamp = oAuth.GenerateTimeStamp ();

			string term = "gas";
			string coords = "45.516579,-122.622908";
			string radius = "1000";

			string urlRequest = "http://api.yelp.com/v2/search?";
			urlRequest += "term=" + WebUtility.UrlEncode (term);
			urlRequest += "&ll=" + WebUtility.UrlEncode (coords);
			urlRequest += "&radius_filter=" + WebUtility.UrlEncode (radius);

			Uri urlUri = new Uri(urlRequest);
			string signature = oAuth.GenerateSignature (urlUri, consumerKey, consumerSecret, token, 
				tokenSecret, "GET", timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1, out outNormalisedUrl, 
				out outNormalisedRequestParameters);

			Uri uri = new Uri(outNormalisedUrl + "?" + outNormalisedRequestParameters + 
				"&oauth_signature=" + signature);

			var httpClient = new HttpClient(new NativeMessageHandler());
			Task<HttpResponseMessage> response = null;
			response = httpClient.GetAsync(uri.AbsoluteUri);
			Task<string> returnedText = response.Result.Content.ReadAsStringAsync ();
			var json = (JObject)JsonConvert.DeserializeObject(returnedText.Result);
		
			System.IO.File.WriteAllText (@"/Users/kevinquiring/Desktop/json.txt", json.ToString());
		} 

	}
}

