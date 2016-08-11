using System;
using System.Net;
using System.Json;
using System.Collections.Generic;

/*
 * http://www.openremote.org/display/knowledge/Controlling+DirecTV+DVR+via+ethernet
 * 
 * Background: 
 * DirecTV recently published preliminary protocols for IP control of their high definition DVR set-top boxes. Supported models include the HR-20, HR-21, HR-22, HR-23 and HR-24. Several of the HD, non-DVR models initially were controllable by this protocol, but recently have been reported to no longer work. One way control is relatively simple. Two way control has been described using CommandFusion, a commercial iphone app. Two way control is beyond the ability of the author and is not discussed.
 * 
 * Control Protocol:
 * You can try this out in a browser first. Commands take the following format: http://192.168.50.13:8080/remote/processKey?key=guide&hold=keyPress
 * 
 * You will need to change the IP address in the above command to the one that your DVR uses.
 * 
 * The following keys can be substituted for "guide": power, poweron, poweroff, format, pause, rew, replay, stop, advance, ffwd, record, play, guide, active, list, exit, back, menu, info, up, down, left, right, select, red, green, yellow, blue, chanup, chandown, prev,0, 1, 2, 3, 4, 5, 6, 7, 8, 9, dash, enter.
 * 
 * The HR-20 and HR-21 have been reported to respond to a shortened version of the above command. http://192.168.50.13:8080/remote/processKey?key=guide
 * 
 * If you wanted to tune CNN on channel 202 using the above method you would send: http://192.168.50.13:8080/remote/processKey?key=2&hold=keyPress http://192.168.50.13:8080/remote/processKey?key=0&hold=keyPress http://192.168.50.13:8080/remote/processKey?key=2&hold=keyPress http://192.168.50.13:8080/remote/processKey?key=enter&hold=keyPress
 * 
 * A shorter way to do this is with the following command: http://192.168.50.13:8080/tv/tune?major=202&minor=65535
 * 
 */

// http://www.sbcatest.com/TechUpdates/DTV-MD-0359-DIRECTV%20SHEF%20Public%20Beta%20Command%20Set-V1.0.pdf
// http://whitlockjc.github.io/directv-remote-api/
// http://whitlockjc.github.io/directv-remote-api/js/docs/dtv.remote.api.html

namespace MoSoft.Devices
{
	public class DirecTV : HttpDevice
	{
		public DirecTV (string ip) : base(ip)
		{
			if (string.IsNullOrWhiteSpace (ip))
				throw new ArgumentNullException (nameof (ip));
		}

		public string Name { get; set; }
		public string Port { get; } = "8080";
		public string BaseUrl => $"http://{Ip}:{Port}/";

		public override string ToString ()
		{
			return string.IsNullOrWhiteSpace(Name) ? "Unnamed" : Name;
		}

		#region Commands
		public enum Command
		{
			Standby,
			Active,
			GetPrimaryStatus,
			GetCommandVersion,
			GetCurrentChannel,
			GetSignalQuality,
			GetCurrentTime,
			GetUserCommand,
			EnableUserEntry,
			DisableUserEntry,
			GetReturnValue,
			Reboot,
			SendUserCommand,
			OpenUserChannel,
			GetTuner,
			GetPrimaryStatusMT,
			GetCurrentChannelMT,
			GetSignalQualityMT,
			OpenUserChannelMT
		}

		private static readonly string [] CommandStrings = new string []
		{
			"FA81", // Standby
			"FA82", // Active
			"FA83", // GetPrimaryStatus
			"FA84", // GetCommandVersion
			"FA87", // GetCurrentChannel
			"FA90", // GetSignalQuality
			"FA91", // GetCurrentTime
			"FA92", // GetUserCommand
			"FA93", // EnableUserEntry
			"FA94", // DisableUserEntry
			"FA95", // GetReturnValue
			"FA96", // Reboot
			"FAA5", // SendUserCommand
			"FAA6", // OpenUserChannel
			"FA9A", // GetTuner
			"FA8A", // GetPrimaryStatusMT
			"FA8B", // GetCurrentChannelMT
			"FA9D", // GetSignalQualityMT
			"FA9F", // OpenUserChannelMT
		};

		public string CommandString (Command command) => CommandStrings [(int)command];
		#endregion Commands

		#region Hold events
		public enum Hold
		{
			KeyUp,
			KeyDown,
			KeyPress
		}
		private static readonly string [] HoldStrings = new string []
		{
			"keyUp",
			"keyDown",
			"keyPress"
		};
		public string HoldString (Hold hold) => $"hold={HoldStrings[(int)hold]}";
		#endregion Hold events

		#region Remote control keys
		private static readonly string [] RemoteControlKeyStrings = new string []
		{
			"power",
			"poweron",
			"poweroff",
			"format",
			"pause",
			"rew",
			"replay",
			"stop",
			"advance",
			"ffwd",
			"record",
			"play",
			"guide",
			"active",
			"list",
			"exit",
			"back",
			"menu",
			"info",
			"up",
			"down",
			"left",
			"right",
			"select",
			"red",
			"green",
			"yellow",
			"blue",
			"chanup",
			"chandown",
			"prev",
			"0",
			"1",
			"2",
			"3",
			"4",
			"5",
			"6",
			"7",
			"8",
			"9",
			"dash",
			"enter"
		};
		public enum RemoteControlKey
		{
			Power,
			PowerOn,
			PowerOff,
			Format,
			Pause,
			Rewind,
			Replay,
			Stop,
			Advance,
			FastForward,
			Record,
			Play,
			Guide,
			Active,
			List,
			Exit,
			Back,
			Menu,
			Info,
			Up,
			Down,
			Left,
			Right,
			Select,
			Red,
			Green,
			Yellow,
			Blue,
			ChannelUp,
			ChannelDown,
			Previous,
			Number0,
			Number1,
			Number2,
			Number3,
			Number4,
			Number5,
			Number6,
			Number7,
			Number8,
			Number9,
			Numberdash,
			Enter
		};
		public string RemoteControlKeyString(RemoteControlKey key) => $"key={RemoteControlKeyStrings[(int)key]}";
		#endregion Remote control keys

		/// <summary>
		/// Process a key request from the remote control
		/// </summary>
		/// <returns>
		/// </returns>
		/// <param name="key">
		/// Remote control key
		/// </param>
		public HttpStatusCode SendRemoteControlKey(Hold hold, RemoteControlKey key)
		{
			var url = $"{BaseUrl}remote/processKey?{RemoteControlKeyString(key)}&{HoldString(hold)}";
			return SendGetRequest(url);
		}

		/// <summary>
		/// Tune to a channel
		/// </summary>
		/// <returns>The chanel request.</returns>
		/// <param name="channel">Channel to tune to.</param>
		public HttpStatusCode SendTuneRequest(int channel)
		{
			var url = $"{BaseUrl}tv/tune?major={channel}&minor=65535";
			return SendGetRequest(url);
		}

		/// <summary>
		/// Process a command request from remote control
		/// </summary>
		/// <returns>The command request.</returns>
		/// <param name="command">Command.</param>
		public HttpStatusCode SendCommandRequest(Command command)
		{
			var url = $"{BaseUrl}/serial/processCommand?{CommandString(command)}";
			return SendGetRequest(url);
		}

		/// <summary>
		/// Retuns the list of available commands and command descriptions
		/// </summary>
		/// <returns>
		/// Retuns the list of available commands and command descriptions
		/// </returns>
		public JsonValue GetOptions()
		{
			var value = RequestJsonValue ($"{BaseUrl}info/getOptions");
			return value;
		}

		/// <summary>
		/// List of available client locations.
		/// </summary>
		/// <returns>The list of available client locations.</returns>
		public JsonValue GetLocations()
		{
			var value = RequestJsonValue ($"{BaseUrl}info/getLocations");
			return value;
		}

		/// <summary>
		/// Gets the current station as a JsonValue.  The JsonValue is basicly
		/// a dictionary of <string, JsonValue>.
		/// </summary>
		/// <returns>The tuned.</returns>
		public JsonValue GetTuned ()
		{
			var value = RequestJsonValue($"{BaseUrl}tv/getTuned");
			return value;
		}

		public JsonValue GetProgramInfo(uint channel)
		{
			//  http://STBIP:port/tv/getProgInfo?major=num[&minor=num][&time=num] 
			var value = RequestJsonValue ($"{BaseUrl}tv/getProgInfo?major={channel}");
			return value;
		}

		public void Test()
		{
			var receivers = GetReceivers(10000);
			foreach (var item in receivers)
				System.Diagnostics.Debug.WriteLine ($"IP: {item.Ip}  Name: {item.Name}");
			// {0e261de4-12f0-46e6-91ba-428607ccef64}
			var value = GetLocations();
			System.Diagnostics.Debug.WriteLine ($"{value}");
		}

		public static DirecTV[] GetReceivers(int timeOutInMiliSeconds = 10)
		{
			var receivers = new List<DirecTV>();
			try
			{
				SSDP.Start();//Start a service as this will take a long time
				System.Threading.Thread.Sleep(timeOutInMiliSeconds);//Wait for each TV/Device to reply to the broadcast
				SSDP.Stop ();//Stop the service if it has not stopped already
				var servers = SSDP.Servers;
				if (string.IsNullOrWhiteSpace (servers))
					return receivers.ToArray();
  			var devices = servers.Split (new char [] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				string ip, name;
				foreach (var device in devices)
					if (TryGetReceiverFromUrl (device, out ip, out name))
						receivers.Add(new DirecTV (ip) { Name = name });
			}
			catch (Exception e)
			{
				WriteException(e);
			}
			return receivers.ToArray();
		}

		private static bool TryGetReceiverFromUrl (string url, out string ip, out string name)
		{
			ip = null;
			name = null;
			if (string.IsNullOrWhiteSpace (url))
				return false;
			string xml;
			SendGetRequest (url, out xml);
			if (string.IsNullOrWhiteSpace (xml))
				return false;
			var manufacturer = GetXmlValue ("manufacturer", xml);
			if (string.IsNullOrWhiteSpace (manufacturer) || !manufacturer.StartsWith ("DIRECTV", StringComparison.OrdinalIgnoreCase))
				return false;
			var device_type = GetXmlValue ("deviceType", xml);
			if (string.IsNullOrWhiteSpace (device_type) || !device_type.Contains ("MediaServer"))
				return false;
			var start = url.IndexOf ("//", StringComparison.Ordinal);
			var end = url.LastIndexOf (":", StringComparison.Ordinal);
			if (start < 0 || end < start)
				return false;
			start += "//".Length;
			ip = url.Substring (start, end - start);
			name = GetXmlValue ("directv-hmc", xml);
			return true;
		}

		private static string GetXmlValue (string key, string xml)
		{
			var start = xml.IndexOf ($"<{key}", StringComparison.Ordinal);
			if (start >= 0)
				start = xml.IndexOf ('>', start);
			if (start < 0)
				return null;
			start++;
			var end = xml.IndexOf ($"</{key}>", start, StringComparison.Ordinal);
			if (end < start)
				return null;
			return end < start ? null : xml.Substring (start, end - start).Trim ();
		}

		public void TestGetPlayList()
		{
			// Turn tuner on
			SendRemoteControlKey (Hold.KeyPress, RemoteControlKey.PowerOn);

			// Get play list, I don't think this is supported in the public SDK any longer
			// http://STBIP:port/dvr/getPlayList?[start=num][&max=num][&type=string] 
			var url = $"{BaseUrl}/dvr/getPlayList?start=0&max=100&type=string";
			string result;
			SendGetRequest(url, out result);
			System.Diagnostics.Debug.WriteLine(result ?? "<empty string>");
		}

		public void TestGetCurrentStation()
		{
			// Turn tuner on
			SendRemoteControlKey (Hold.KeyPress, RemoteControlKey.PowerOn);

			// Get the current station and program
			var value = GetTuned ();
			if (value == null)
				return;
			var channel = TryGetJsonValue ("major", value);
			var station = TryGetJsonValue ("callsign", value);
			var title = TryGetJsonValue ("title", value);
			var episode = TryGetJsonValue ("episodeTitle", value);
			System.Diagnostics.Debug.WriteLine ($"Channle: {channel}  Station: {station}  Tilte: {title}  Episode: {episode}");
			var info = GetProgramInfo (channel);
			System.Diagnostics.Debug.WriteLine ($"Info: {info}");
		}
	}
}
