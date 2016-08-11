using System;
using System.Net.Sockets;

// http://www.manualslib.com/manual/318372/Sharp-Aquos-Lc-70le632u.html?page=65#manual
// http://snpi.dell.com/sna/manuals/A1534250.pdf

namespace MoSoft.Devices
{
	class SharpTV : HttpDevice
	{
		public SharpTV (string ip) : base (ip)
		{
		}

		public int Port { get; set; } = 10002;

		public enum Success
		{
			OK,
			Error,
			Unknown,
		}

		/// <summary>
		/// Convert return string to Success code
		/// </summary>
		/// <returns>The success code</returns>
		/// <param name="s">Send command result string</param>
		public Success GetSuccess (string s)
		{
			if (string.IsNullOrWhiteSpace (s))
				return Success.Unknown;
			if (s.StartsWith ("OK", StringComparison.OrdinalIgnoreCase))
				return Success.OK;
			if (s.StartsWith ("Err", StringComparison.OrdinalIgnoreCase))
				return Success.Error;
			return Success.Unknown;
		}

		public string SetVolumne (int level)
		{
			var value = Math.Max (0, Math.Min (60, level));
			return SendRequestFormat ($"VOLM{value}");
		}

		public string Power (bool on)
		{
			StandBy (true);
			var option = on ? 1 : 0;
			return SendRequestFormat ($"POWR{option}");
		}

		public string StandBy (bool on)
		{
			var option = on ? 2 : 0;
			var result = SendRequestFormat ($"RSPW{option}");
			// Wait on second
			System.Threading.Thread.Sleep (1000);
			return result;
		}

		public enum Input
		{
			Toggle = -1,
			TV = 0,
			Hdmi1 = 1,
			Hdmi2 = 2,
			Hdmi3 = 3,
			Hdmi4 = 4,
			Component = 5,
			Video1 = 6,
			Video2 = 7,
		}

		public string SetInput (Input input)
		{
			if (input == Input.TV)
				return SendRequestFormat ("ITVD0");
			if (input == Input.Toggle)
				return SendRequestFormat ("ITGD0");
			return SendRequestFormat ($"IAVD{(int)input}");
		}

		public enum AvMode
		{
			Toggle = 0,
			Standard = 1,
			Movie = 2,
			Game = 3,
			User = 4,
			DynamicFixed = 5,
			Dynamic = 6,
			PC = 7,
			XVColor = 8,
			Standard3d = 14,
			Movie3d = 15,
			Game3d = 16,
			Auto = 100,
		}

		public string SetAvMode (AvMode mode) => SendRequestFormat ($"AVMD{(int)mode}");

		public enum ViewMode
		{
			Toggle = 0,
			SideBar = 1,
			AVStretch = 2,
			Zoom = 3,
			AVStretch2 = 4,
			Normal = 5,
			PCZoom = 6,
			PCStretch = 7,
			PCDot = 8,
			AVFullScreen = 9,
			AVAutoUsbVideo = 10,
			Original = 11
		}

		public string SetViewMode (AvMode mode) => SendRequestFormat ($"WIDE{(int)mode}");

		public enum Mute
		{
			Toggle = 0,
			On = 1,
			Off = 2
		}

		public string SetMute(Mute mode) => SendRequestFormat ($"MUTE{(int)mode}");

		public enum Surround
		{
			Toggle = 0,
			Normal = 1,
			Off = 2,
			Hall3D = 4,
			Movie3D = 5,
			Standard3D = 6,
		}

		public string SetSurround (Surround mode) => SendRequestFormat ($"ACSU{(int)mode}");

		public string ToggleAudioSelection() => SendRequestFormat ($"ACHA0");

		public enum SleepTimer
		{
			Toggle = 0,
			Normal = 1,
			Off = 2,
			Hall3D = 4,
			Movie3D = 5,
			Standard3D = 6
		}

		public string SetSleepTimer (SleepTimer mode) => SendRequestFormat ($"OFTM{(int)mode}");

		public string ChannelUp() => SendRequestFormat ($"CHUP0");
		public string ChannelDown () => SendRequestFormat ($"CHDW0");

		public enum Mode3D
		{
			Off = 0,
			x2d3d = 1,
			SBS = 2,
			TAB = 3,
			x3d2dSBS = 4,
			x3d2dTAB = 5,
			x3dAuto = 6,
			x2dAuto = 7
		}

		public string SetMode3D(Mode3D mode) => SendRequestFormat ($"TDCH{(int)mode}");

		public string ToggleClosedCaption() => SendRequestFormat ("CLCP0");

		public string DeviceName => SendRequestFormat ("TVNM1");
		public string ModelName => SendRequestFormat ("MNRD1");
		public string SoftwareVersion => SendRequestFormat ("SWVN1");
		public string IpProtocolVersion => SendRequestFormat ("IPPV1");

		public string SendCommandString (string command, string parameters)
		{
			if (string.IsNullOrWhiteSpace (command) || string.IsNullOrWhiteSpace (parameters))
				return "Err";
			command = PaddString (command.ToUpper (), 4);
			parameters = PaddString (parameters, 4);
			return SendRequest (FormatRequestString (command + parameters));
		}

		private string FormatRequestString (string s)
		{
			return PaddString (s, 8) + "\r";
		}

		private string PaddString (string s, int length)
		{
			if (s == null)
				s = "";
			if (s.Length > length)
				return s.Substring (0, length);
			while (s.Length < length)
				s += " ";
			return s;
		}

		private string SendRequestFormat (string message)
		{
			return SendRequest( FormatRequestString (message));
		}

		private string SendRequest (string message)
		{
			var result = string.Empty;
			TcpClient client = null;
			try
			{
				// Create a TcpClient.
				// Note, for this client to work you need to have a TcpServer 
				// connected to the same address as specified by the server, port
				// combination.
				client = new TcpClient (Ip, Port);

				// Translate the passed message into ASCII and store it as a Byte array.
				var data = System.Text.Encoding.ASCII.GetBytes (message);

				// Get a client stream for reading and writing.
				//  Stream stream = client.GetStream();

				var stream = client.GetStream ();

				// Send the message to the connected TcpServer. 
				stream.Write (data, 0, data.Length);

				WriteLine ($"Sent: {message}");

				// Receive the TcpServer.response.

				// Buffer to store the response bytes.
				data = new Byte [256];

				// Read the first batch of the TcpServer response bytes.
				var bytes = stream.Read (data, 0, data.Length);
				result = System.Text.Encoding.ASCII.GetString (data, 0, bytes);
				WriteLine ($"Received: {result}");

				// Close everything.
				stream.Close ();
			}
			catch (ArgumentNullException e)
			{
				WriteLine ($"ArgumentNullException: {e}");
			}
			catch (SocketException e)
			{
				WriteLine ($"SocketException: {e}");
			}
			catch (Exception e)
			{
				WriteLine (e);
			}
			finally
			{
				client?.Close ();
			}
			return result;
		}

		void WriteLine (Exception e) => WriteException (e);

		void WriteLine (string s) => System.Diagnostics.Debug.WriteLine (s?.Replace ("\r", "$r"));
	}
}
