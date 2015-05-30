using Android.App;
using Android.Graphics;
using Android.OS;

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

            var profileimage = FindViewById<Xamarin.CircleImageView>(Resource.Id.profile_image);
            var largeprofileimage = FindViewById<Xamarin.CircleImageView>(Resource.Id.large_profile_image);

            largeprofileimage.SetImageResource(Resource.Drawable.homer);
            largeprofileimage.SetBorderColor(Color.Blue);
            largeprofileimage.SetBorderWidth(20);
            largeprofileimage.SetBorderOverlay(true);

            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(false);
        }
    }
}