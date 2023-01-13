using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using FacePost.DataModels;
using FFImageLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacePost.Fragments
{
    public class PostImageClickFragment : AndroidX.Fragment.App.DialogFragment
    {
        Post thisimagepost;
        ImageView postclickImageView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public PostImageClickFragment( Post post)
        {
            thisimagepost = post;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.postimageclick, container, false);
            postclickImageView = (ImageView)view.FindViewById(Resource.Id.postclickImageView);

            GetImage(thisimagepost.DownloadUrl, postclickImageView);

            return view;
        }

        void GetImage(string url, ImageView imageView)
        {
            ImageService.Instance.LoadUrl(url)
                .Retry(3, 200)
                .DownSample(400, 400)
                .Into(imageView);
        }
    }
}