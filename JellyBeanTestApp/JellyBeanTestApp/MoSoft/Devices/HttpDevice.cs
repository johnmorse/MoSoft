using System;
using System.Net;
using System.IO;
using System.Json;

namespace MoSoft.Devices
{
	public class HttpDevice
	{
		public HttpDevice (string ip)
		{
			Ip = ip;
		}

		public string Ip { get; }

		/// <summary>
		/// Send HTTP Get request
		/// </summary>
		/// <returns>
		/// If the request was sent and return HttpStatusCode.OK then return
		/// HttpStatusCode otherwise return HttpStatusCode.Unauthorized
		/// </returns>
		/// <param name="url">URL.</param>
		public HttpStatusCode SendGetRequest (string url)
		{
			string result;
			return SendGetRequest (url, out result);
		}

		/// <summary>
		/// Send HTTP Get request
		/// </summary>
		/// <returns>
		/// If the request was sent and return HttpStatusCode.OK then return
		/// HttpStatusCode otherwise return HttpStatusCode.Unauthorized
		/// </returns>
		/// <param name="url">URL.</param>
		/// <param name="responseString">Response string.</param>
		public static HttpStatusCode SendGetRequest (string url, out string responseString)
		{
			const HttpStatusCode def_result = HttpStatusCode.Unauthorized;
			responseString = null;
			try
			{
				var request = WebRequest.Create(url);
				using (var response = request.GetResponse())
				{
					var http_response = response as HttpWebResponse;
					if (http_response != null)
					{
						// Get the stream containing content returned by the server.
						var stream = response.GetResponseStream();
						// Open the stream using a StreamReader for easy access.
						var reader = new StreamReader(stream);
						// Read the content.
						responseString = reader.ReadToEnd ();
					}
					var result = http_response?.StatusCode ?? def_result;
					response?.Close();
					return result;
				}
			}
			catch (Exception e)
			{
				WriteException(e);
				return def_result;
			}
		}

		protected static void WriteException (Exception exception)
		{
			for (var e = exception; e != null; e = e.InnerException)
			{
				System.Diagnostics.Debug.WriteLine (e.Message);
				System.Diagnostics.Debug.WriteLine (e.StackTrace);
			}
		}

		public JsonValue RequestJsonValue (string url)
		{
			string json_string;
			var success = SendGetRequest(url, out json_string);
			if (success != HttpStatusCode.OK)
				return null;
			var value = TryParseJsonString (json_string);
			return value;
		}

		public JsonValue TryParseJsonString (string jsonString)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(jsonString))
					return null;
				var value = JsonValue.Parse(jsonString);
				return value;
			}
			catch (Exception e)
			{
				WriteException (e);
				return null;
			}
		}

		public JsonValue TryGetJsonValue(string key, JsonValue jsonValue)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));
			if (jsonValue == null || !jsonValue.ContainsKey(key))
				return null;
			return jsonValue[key];
		}
  }
}
