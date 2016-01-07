using Android.App;
using Android.Graphics;
using Android.OS;
using civ.Xamarin;

namespace CircleImageView.Sample
{
    [Activity(Label = "CircleImageView.Sample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : BaseActivity
    {
        protected override int LayoutResource
        {
            get { return Resource.Layout.main; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			SupportActionBar.SetDisplayHomeAsUpEnabled(false);
			SupportActionBar.SetHomeButtonEnabled(false);

			var profileimage = FindViewById<civ.Xamarin.CircleImageView>(Resource.Id.profile_image);
			var largeprofileimage = FindViewById<civ.Xamarin.CircleImageView>(Resource.Id.large_profile_image);
			var fillimage = FindViewById<civ.Xamarin.CircleImageView>(Resource.Id.fill_image);

            largeprofileimage.SetImageResource(Resource.Drawable.homer);
            largeprofileimage.SetBorderColor(Color.Blue);
            largeprofileimage.SetBorderWidth(20);
            largeprofileimage.SetBorderOverlay(true);

			fillimage.SetImageResource(Resource.Drawable.profile);
			fillimage.SetFillColor (Color.OrangeRed);
			fillimage.SetBorderColor (Color.ForestGreen);
			fillimage.SetBorderWidth (10);
			fillimage.SetBorderOverlay (true);


        }
    }
}