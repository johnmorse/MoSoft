using Android.App;
using Android.Views;
using Android.Widget;
using MoSoft.Devices;

namespace JellyBeanTestApp
{
	class DirecTVSpinnerAdapter : BaseAdapter
	{
		private readonly Activity m_context;
		private DirecTV [] m_items;

		public DirecTVSpinnerAdapter (Activity context, DirecTV [] listOfItems)
		{
			m_context = context;
			m_items = listOfItems;
		}

		public override int Count => m_items.Length;

		public override Java.Lang.Object GetItem (int position) => position;

		public override long GetItemId (int position) => position;

		public DirecTV GetItemAtPosition (int position) => m_items [position];

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var item = m_items [position];
			var view = (convertView ?? m_context.LayoutInflater.Inflate (Android.Resource.Layout.SimpleSpinnerDropDownItem,
					parent,
					false));
			var name = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			name.Text = item.Name;
			return view;
		}
	}
}

