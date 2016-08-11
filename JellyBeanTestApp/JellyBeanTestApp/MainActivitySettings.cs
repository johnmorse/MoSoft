using System.Collections.Generic;
using Android.App;
using Android.Content;
using MoSoft.Devices;
using Android.Preferences;

namespace JellyBeanTestApp
{
	static class MainActivitySettings
	{
		class Keys
		{
			public static string Settings => "JellyBeanTestApp";
			public static string IpList => "IpList";
			public static string NameList => "NameList";
			public static string SelectedDirecTvIndex => "SelectedDirecTvIndex";
		}

		public static ISharedPreferences Preferences => Application.Context.GetSharedPreferences (Keys.Settings, FileCreationMode.Private);

		public static void SaveSettings (Activity activity)
		{
			PreferenceManager.GetDefaultSharedPreferences (activity);
			var ips = new string [g_directv.Length];
			var names = new string [g_directv.Length];
			var i = 0;
			foreach (var item in g_directv) {
				ips [i] = item.Ip;
				names [i] = item.Name;
				i++;
			}
			var prefs = PreferenceManager.GetDefaultSharedPreferences (activity).Edit ();
			prefs.Clear ();
			prefs.PutInt (Keys.SelectedDirecTvIndex, SelectedDirecTvIndex);
			prefs.PutStringSet (Keys.IpList, ips);
			prefs.PutStringSet (Keys.NameList, names);
			prefs.Commit ();
		}

		public static void RestoreSettings (Activity activity)
		{
			var prefs = PreferenceManager.GetDefaultSharedPreferences (activity);
			var iplist = prefs.GetStringSet (Keys.IpList, new string [0]);
			var namelist = prefs.GetStringSet (Keys.NameList, new string [0]);
			SelectedDirecTvIndex = prefs.GetInt (Keys.SelectedDirecTvIndex, -1);

			if (iplist.Count != namelist.Count)
				return;

			if (iplist.Count == 0) {
				iplist.Add ("192.168.1.22");
				namelist.Add ("RED ZONE");
				iplist.Add ("192.168.1.5");
				namelist.Add ("GENIE");
			}
			var list = new List<DirecTV> ();
			foreach (var ip in iplist)
				list.Add (new DirecTV (ip));
			var i = 0;
			foreach (var name in namelist)
				list [i++].Name = name;
			g_directv = list.ToArray ();
		}

		public static int SelectedDirecTvIndex { get; set; }

		public static DirecTV [] DirecTVArray
		{
			get { return g_directv; }
			set {	g_directv = value ?? new DirecTV [0]; }
		}
		private static DirecTV [] g_directv = new DirecTV [0];
	}
}

