﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using FacePost.DataModels;
using FacePost.Helpers;
using FFImageLoading;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacePost.Fragments
{
    public class EditPostFragment : AndroidX.Fragment.App.DialogFragment
    {
        Post thisPost;
        ImageView postImageView;
        EditText postEditText;
        Button editButton;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public EditPostFragment( Post post)
        {
            thisPost = post;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.editpost, container, false);
            postImageView = (ImageView)view.FindViewById(Resource.Id.postImageView);
            postEditText = (EditText)view.FindViewById(Resource.Id.postEditText);
            editButton = (Button)view.FindViewById(Resource.Id.editButton);
            postEditText.Text = thisPost.PostBody;

            GetImage(thisPost.DownloadUrl, postImageView);

            editButton.Click += EditButton_Click;
            return view;
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            DocumentReference reference = AppDataHelper.GetFirestore().Collection("posts").Document(thisPost.ID);
            reference.Update("post_body", postEditText.Text);
            this.Dismiss();
        }

        void GetImage (string url, ImageView imageView)
        {
            ImageService.Instance.LoadUrl(url)
                .Retry(3, 200)
                .DownSample(400, 400)
                .Into(imageView);
        }
    }
}