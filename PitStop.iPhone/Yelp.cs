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
		static string consumerKey = "N7u4ooSx5FbT-uycO5Q2xA";
		static string consumerSecret = "4_4p3WWNr466liJjDEG4dUsCMN0";
		static string token = "A0tsWTzDwQw_u9k8fd-HnGlMi9QD0v7u";
		static string tokenSecret = "oE8i6f7PkYVCa3Y6o9iiQzl0UE4";

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

			//string formattedUri = String.Format(System.Globalization.CultureInfo.InvariantCulture, urlRequest + , "");
			Uri urlUri = new Uri(urlRequest);
			string signature = oAuth.GenerateSignature (urlUri, consumerKey, consumerSecret, token, tokenSecret, "GET", timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1, out outNormalisedUrl, out outNormalisedRequestParameters);
			//string oauth_signature = CalculateSha1Hash (tokenSecret);

//			urlRequest += "&location=" + WebUtility.UrlEncode("Portland, OR");
//			urlRequest += "&oauth_consumer_key=" + WebUtility.UrlEncode(consumerKey);
//			urlRequest += "&oauth_token=" + WebUtility.UrlEncode(token);
//			urlRequest += "&oauth_signature_method=" + WebUtility.UrlEncode("HMAC-SHA1");
//			urlRequest += "&oauth_signature=" + WebUtility.UrlEncode(signature);
//			urlRequest += "&oauth_timestamp=" + WebUtility.UrlEncode(timeStamp);
//			urlRequest += "&oauth_nonce=" + WebUtility.UrlEncode(nonce);
//			urlRequest += "&radius_filter=" + WebUtility.UrlEncode("1000");
//			urlRequest += "&term=" + WebUtility.UrlEncode("food");

			Uri uri = new Uri(outNormalisedUrl + "?" + outNormalisedRequestParameters + 
				"&oauth_signature=" + signature);

			var httpClient = new HttpClient(new NativeMessageHandler());
			Task<HttpResponseMessage> response = null;
			response = httpClient.GetAsync(uri.AbsoluteUri);
			Task<string> returnedText = response.Result.Content.ReadAsStringAsync ();
			var json = (JObject)JsonConvert.DeserializeObject(returnedText.Result);
		
			System.IO.File.WriteAllText (@"/Users/kevinquiring/Desktop/json.txt", json.ToString());
		} 

//		private string GetOAuthSignature()
//		{
//			
//			
//			HMACSHA1 myhmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(HttpUtility.UrlEncode(consumeSecret)),true);
//			byte[] hashValue = myhmacsha1.ComputeHash(Encoding.UTF8.GetBytes(baseString));
//			oauthSig = Convert.ToBase64String(hashValue);
//		}
//
//		private static string CalculateSha1Hash(string httpMethod, string url, string callback, string noonce, string signatureMethod, string timeStamp, string version)
//		{
//			string oauthSig = "";
//			string baseString = System.Net.WebUtility.UrlEncode(httpMethod.ToUpper()) + "&" +
//				System.Net.WebUtility.UrlEncode(url) + "&" +
//				System.Net.WebUtility.UrlEncode("oauth_callback="+callback+"&"+
//					"oauth_consumer_key="+consumerKey+"&"+
//					"oauth_nonce="+nounce+"&"+
//					"oauth_signature_method="+sigMethod+"&"+
//					"oauth_timestamp=" + timestamp + "&" +
//					"oauth_version=" + version
//				);
//			
//			// step 1, calculate MD5 hash from input
//			var hasher = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha1);
//			byte[] inputBytes = Encoding.UTF8.GetBytes(input);
//			byte[] hash = hasher.HashData(inputBytes);
//
//			StringBuilder sb = new StringBuilder();
//			for (int i = 0; i < hash.Length; i++)
//			{
//				sb.Append(hash[i].ToString("X2"));
//			}
//			return sb.ToString();
//
//
//		}
//
//		public static int GetEpochTime() 
//		{ 
//			TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
//			int secondsSinceEpoch = (int)t.TotalSeconds; 
//			return secondsSinceEpoch;
//		}
//
//		private static string ConvertToUnixTimestamp()
//		{
//			Int32 unixTimestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
//			return unixTimestamp.ToString();
//		}
//
//		private static string GenerateNonce()
//		{
//			var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
//			var random = new Random();
//			var result = new string(
//				Enumerable.Repeat(chars, 32)
//				.Select(s => s[random.Next(s.Length)])
//				.ToArray());
//			return result;
//		}
	}
}

