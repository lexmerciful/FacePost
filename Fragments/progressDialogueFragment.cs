using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacePost.Fragments
{
    public class progressDialogueFragment : AndroidX.Fragment.App.DialogFragment
    {

        ProgressBar loader;
        TextView progressStatus;
        string status;

        public progressDialogueFragment (string _status)
        {
            status = _status;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.progress, container, false);

            loader = view.FindViewById<ProgressBar>(Resource.Id.loader);
            progressStatus = view.FindViewById<TextView>(Resource.Id.progressStatus);
            progressStatus.Text = status;
            return view;
        }
    }
}