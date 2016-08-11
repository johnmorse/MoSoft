using System;
using Android.Widget;
using MoSoft.Devices;

namespace JellyBeanTestApp
{
	class DirecTvButtonHandlers
	{
		public DirecTvButtonHandlers (MainActivity activity)
		{
			Activity = activity;
		}

		public void Initialize ()
		{
			if (m_initialized)
				return;
			m_initialized = true;

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonPowerOn, DirecTV.RemoteControlKey.PowerOn);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonOff, DirecTV.RemoteControlKey.PowerOff);
			Activity.SetButtonClick (Resource.Id.buttonLoad, OnButtonLoadClick);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonGuide, DirecTV.RemoteControlKey.Guide);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonMenu, DirecTV.RemoteControlKey.Menu);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonList, DirecTV.RemoteControlKey.List);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonRecord, DirecTV.RemoteControlKey.Record);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonUpArrow, DirecTV.RemoteControlKey.Up);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonExit, DirecTV.RemoteControlKey.Exit);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonLeftArrow, DirecTV.RemoteControlKey.Left);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonSelect, DirecTV.RemoteControlKey.Select);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonRightArrow, DirecTV.RemoteControlKey.Right);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonRed, DirecTV.RemoteControlKey.Red);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonDownArrow, DirecTV.RemoteControlKey.Down);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonInfo, DirecTV.RemoteControlKey.Info);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonMinus30, DirecTV.RemoteControlKey.Replay);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonStop, DirecTV.RemoteControlKey.Stop);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonPlus30, DirecTV.RemoteControlKey.Advance);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonRew, DirecTV.RemoteControlKey.Rewind);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonPlay, DirecTV.RemoteControlKey.Play);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonFF, DirecTV.RemoteControlKey.FastForward);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonPause, DirecTV.RemoteControlKey.Pause);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonChanUp, DirecTV.RemoteControlKey.ChannelUp);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonChanDown, DirecTV.RemoteControlKey.ChannelDown);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonPause, DirecTV.RemoteControlKey.Pause);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.button1, DirecTV.RemoteControlKey.Number1);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.button2, DirecTV.RemoteControlKey.Number2);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.button3, DirecTV.RemoteControlKey.Number3);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.button4, DirecTV.RemoteControlKey.Number4);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.button5, DirecTV.RemoteControlKey.Number5);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.button6, DirecTV.RemoteControlKey.Number6);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.button7, DirecTV.RemoteControlKey.Number7);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.button8, DirecTV.RemoteControlKey.Number8);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.button9, DirecTV.RemoteControlKey.Number9);

			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonDash, DirecTV.RemoteControlKey.Numberdash);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.button0, DirecTV.RemoteControlKey.Number0);
			SetButtonDirecTvRemoteButtonClick (Resource.Id.buttonEnter, DirecTV.RemoteControlKey.Enter);

			var spinner = Activity.FindViewById<Spinner> (Resource.Id.spinnerReceivers);
			if (spinner != null)
				spinner.ItemSelected += SpinnerItemSelected;

			InializeDirecTvSpinner();

			if (spinner != null && MainActivitySettings.SelectedDirecTvIndex >= 0 && MainActivitySettings.SelectedDirecTvIndex < MainActivitySettings.DirecTVArray.Length)
				spinner.SetSelection (MainActivitySettings.SelectedDirecTvIndex);
		}
		bool m_initialized;

		public MainActivity Activity { get; }

		private void SetButtonDirecTvRemoteButtonClick (int id, DirecTV.RemoteControlKey key)
		{
			var button = Activity.SetButtonClick (id, OnButtonClick);
			button.Tag = new Java.Lang.Integer ((int)key);
		}

		private void SetDirecTvArray (DirecTV [] array)
		{
			MainActivitySettings.DirecTVArray = array;
			InializeDirecTvSpinner ();
		}

		/// <summary>
		/// Load the list of receivers into the spinner, the first one gets
		/// selected by default.
		/// </summary>
		/// <returns>The direc tv spinner.</returns>
		private void InializeDirecTvSpinner ()
		{
			var spinner = Activity.FindViewById<Spinner> (Resource.Id.spinnerReceivers);
			if (spinner == null)
				return;
			spinner.ItemSelected += SpinnerItemSelected;
			//var adapter = new ArrayAdapter<DirecTV> (this, Android.Resource.Layout.SimpleSpinnerItem, MainActivitySettings.DirecTVArray);
			var adapter = new DirecTVSpinnerAdapter (Activity, MainActivitySettings.DirecTVArray);
			spinner.Adapter = adapter;
		}

		/// <summary>
		/// Called when the selected reciever spinner chanages
		/// </summary>
		/// <returns>The item selected.</returns>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void SpinnerItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			var spinner = sender as Spinner;
			if (spinner == null)
				return;
			MainActivitySettings.SelectedDirecTvIndex = e.Position;
			var selected = spinner.GetItemAtPosition (e.Position);
			SelectedDirecTv = selected == null ? null : MainActivitySettings.DirecTVArray [(int)selected];
		}

		/// <summary>
		/// The selected (active) receiver
		/// </summary>
		/// <value>The selected direc tv.</value>
		public DirecTV SelectedDirecTv { get; private set; }

		/// <summary>
		/// Simulates hitting a remote control button for the selected receiver.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		public void OnButtonClick (object sender, EventArgs e)
		{
			if (m_getting_receivers)
				return;
			var button = sender as Button;
			var key = (DirecTV.RemoteControlKey)(int)button.Tag;
			SelectedDirecTv?.SendRemoteControlKey (DirecTV.Hold.KeyPress, key);
		}

		/// <summary>
		/// Change to channel 202 button handler
		/// </summary>
		/// <returns>
		/// Returns true if the request was successfully sent.
		/// </returns>
		/// <param name="sender">Button that was clicked</param>
		/// <param name="e">Click event arguments</param>
		public void OnButtonLoadClick (object sender, EventArgs e)
		{
			if (m_getting_receivers)
				return;
			var button = Activity.FindViewById<Button> (Resource.Id.buttonLoad);
			button.Text = "Loading...";
			m_getting_receivers = true;
			SetDirecTvArray(DirecTV.GetReceivers());
			m_getting_receivers = false;
			button.Text = $"Loaded {MainActivitySettings.DirecTVArray.Length}";

			//DirecTV.SendChanelRequest ("206");
			//DirecTV.Test();
		}

		bool m_getting_receivers;
	}
}
