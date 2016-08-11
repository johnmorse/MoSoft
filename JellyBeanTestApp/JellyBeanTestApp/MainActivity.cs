using System;
using Android.App;
using Android.Widget;
using Android.OS;
using MoSoft.Devices;

namespace JellyBeanTestApp
{
	[Activity (Label = "JellyBeanTestApp", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		public MainActivity ()
		{
			m_directv = new DirecTvButtonHandlers (this);
		}

		protected override void OnStop ()
		{
			base.OnStop ();
			MainActivitySettings.SaveSettings (this);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			MainActivitySettings.SaveSettings (this);
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			MainActivitySettings.RestoreSettings (this);

			// DirecTV buttons
			m_directv.Initialize ();

			SetButtonClick (Resource.Id.buttonWatchTVOn, OnButtonWatchTVClickOn);
			SetButtonClick (Resource.Id.buttonWatchTVOff, OnButtonWatchTVClickOff);
			SetButtonClick (Resource.Id.buttonTvOn, OnButtonTvOnClick);
			SetButtonClick (Resource.Id.buttonTvOff, OnButtonTvOffClick);

			//var webview = FindViewById<Android.Webkit.WebView>(Resource.Id.webView);
			//webview?.LoadUrl ("http://smarttiles.me/?app=91efb4b9-40d4-4a8e-b110-88f759b7d71b&shard=NA02&label=Family Room Launcher&wait=2");
		}

		const string SharpTvIp = "192.168.1.7";
		const string GENIE = "192.168.1.5";

		private void OnButtonWatchTVClick (bool on)
		{
			var tv = new SharpTV (SharpTvIp);
			var dtv = new DirecTV (GENIE);
			if (on)
			{
				tv.Power (true);
				tv.SetInput (SharpTV.Input.Hdmi1);
				dtv.SendRemoteControlKey (DirecTV.Hold.KeyPress, DirecTV.RemoteControlKey.PowerOn);
			}
			else
			{
				tv.Power (false);
				dtv.SendRemoteControlKey (DirecTV.Hold.KeyPress, DirecTV.RemoteControlKey.PowerOff);
			}
		}

		private void OnButtonWatchTVClickOn (object sender, EventArgs e)
		{
			OnButtonWatchTVClick (true);
		}

		private void OnButtonWatchTVClickOff (object sender, EventArgs e)
		{
			OnButtonWatchTVClick (false);
		}

		private void OnButtonTvOnClick (object sender, EventArgs e)
		{
			var tv = new SharpTV (SharpTvIp);
			tv.Power (true);
		}

		private void OnButtonTvOffClick (object sender, EventArgs e)
		{
			var tv = new SharpTV (SharpTvIp);
			tv.Power (false);
		}

		/// <summary>
		/// Contains the DirecTV button click handlers
		/// </summary>
		private readonly DirecTvButtonHandlers m_directv;

		/// <summary>
		/// Get button from the layout resource and attach an event to it
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="handler">Handler.</param>
		public Button SetButtonClick (int id, EventHandler handler)
		{
			var button = FindViewById<Button> (id);
			button.Click += handler;
			return button;
		}
	}
}
