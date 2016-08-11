//Dr Gadgit from the Code project http://www.codeproject.com/Articles/893791/DLNA-made-easy-and-Play-To-for-any-device
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public static class Extentions
{

	public static string ChopOffBefore (this string s, string Before)
	{//Usefull function for chopping up strings
		int End = s.ToUpper ().IndexOf (Before.ToUpper (), StringComparison.Ordinal);
		if (End > -1) {
			return s.Substring (End + Before.Length);
		}
		return s;
	}



	public static string ChopOffAfter (this string s, string After)
	{//Usefull function for chopping up strings
		int End = s.ToUpper ().IndexOf (After.ToUpper (), StringComparison.Ordinal);
		if (End > -1) {
			return s.Substring (0, End);
		}
		return s;
	}

	//public static string ReplaceIgnoreCase (this string Source, string Pattern, string Replacement)
	//{// using \\$ in the pattern will screw this regex up
	// //return Regex.Replace(Source, Pattern, Replacement, RegexOptions.IgnoreCase);

	//	if (Regex.IsMatch (Source, Pattern, RegexOptions.IgnoreCase))
	//		Source = Regex.Replace (Source, Pattern, Replacement, RegexOptions.IgnoreCase);
	//	return Source;
	//}

}

namespace MoSoft.Devices
{
	//This class is used to broadcast a SSDP message using UDP on port 1900 and to then wait for any replies send back on the LAN
	public static class SSDP
	{
		private static Socket UdpSocket = null;
		public static string Servers = "";
		private static string NewServer = "";
		private static Thread THSend = null;
		private static bool Running = false;
		public static void Start ()
		{//Stop should be called in about 12 seconds which will kill the thread
			if (Running) return;
			Running = true;
			NewServer = "";
			var send = new Thread (SendRequest);
			send.Start ();
			var stop = new Thread (Stop);
			stop.Start ();
		}
		public static void Stop ()
		{//OK time is up so lets return our DLNA server list
			Thread.Sleep (9000);
			Running = false;
			try {
				Thread.Sleep (1000);
				if (UdpSocket != null)
					UdpSocket.Close ();
				if (THSend != null)
					THSend.Abort ();
			} catch {; }
			if (NewServer.Length > 0)
				Servers = NewServer.Trim ();//Bank in our new servers
		}

		private static void SendRequest ()
		{
			try { SendRequestNow (); } catch {; }
		}

		private static void SendRequestNow ()
		{//Uses UDP Multicast on 239.255.255.250 with port 1900 to send out invitations that are slow to be answered
			var local_end_point = new IPEndPoint (IPAddress.Any, 6000);
			var multicast_end_point = new IPEndPoint (IPAddress.Parse ("239.255.255.250"), 1900);//SSDP port
			var udpdate_socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			udpdate_socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			udpdate_socket.Bind (local_end_point);
			udpdate_socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption (multicast_end_point.Address, IPAddress.Any));
			udpdate_socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
			udpdate_socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
			const string search_string = "M-SEARCH * HTTP/1.1\r\nHOST:239.255.255.250:1900\r\nMAN:\"ssdp:discover\"\r\nST:ssdp:all\r\nMX:3\r\n\r\n";
			udpdate_socket.SendTo (Encoding.UTF8.GetBytes (search_string), SocketFlags.None, multicast_end_point);
			var receive_buffer = new byte [4000];
			var received_bytes = 0;
			var Count = 0;
			while (Running && Count < 100) {//Keep loopping until we timeout or stop is called but do wait for at least ten seconds 
				Count++;
				if (udpdate_socket.Available > 0) {
					received_bytes = udpdate_socket.Receive (receive_buffer, SocketFlags.None);
					if (received_bytes > 0) {
						string Data = Encoding.UTF8.GetString (receive_buffer, 0, received_bytes);
						if (Data.ToUpper ().IndexOf ("LOCATION: ", StringComparison.Ordinal) > -1) {//ChopOffAfter is an extended string method added in Helper.cs
							Data = Data.ChopOffBefore ("LOCATION: ").ChopOffAfter (Environment.NewLine);
							if (NewServer.ToLower ().IndexOf (Data.ToLower (), StringComparison.Ordinal) == -1)
								NewServer += " " + Data;
						}
					}
				} else
					Thread.Sleep (100);
			}
			if (NewServer.Length > 0)
				Servers = NewServer.Trim ();//Bank in our new servers nice and quick with minute risk of thread error due to not locking
			udpdate_socket.Close ();
			THSend = null;
			udpdate_socket = null;
		}
	}
}
