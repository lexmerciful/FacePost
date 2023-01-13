using Android;
using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.App;
using FacePost.EventListeners;
using FacePost.Fragments;
using FacePost.Helpers;
using Firebase.Firestore;
using Firebase.Storage;
using Java.Util;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskStackBuilder = AndroidX.Core.App.TaskStackBuilder;

namespace FacePost.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class CreatePostActivity : AppCompatActivity, IOnSuccessListener, IOnFailureListener, IOnProgressListener
    {
        AndroidX.AppCompat.Widget.Toolbar toolbar;
        ImageView postImage;
        Button submitButton;
        TextView postEditText;

        StorageReference storageReference = null;
        HashMap postMap;
        DocumentReference newPostRef;

        //Notification
        static readonly int NOTIFICATION_ID = 1000;
        static readonly string CHANNEL_ID = "location_notification";
        internal static readonly string COUNT_KEY = "count";

        //Task Completion Listener
        TaskCompletionListeners taskCompletionListeners = new TaskCompletionListeners();
        TaskCompletionListeners downloadUrlListener = new TaskCompletionListeners();

        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        byte[] fileBytes;
        progressDialogueFragment progressDialogue;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.create_post);
            toolbar = (AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            postImage = (ImageView)FindViewById(Resource.Id.newPostImage);
            submitButton = (Button)FindViewById(Resource.Id.submitButton);
            postEditText = (EditText)FindViewById(Resource.Id.postEditText);
            // Create your application here

            //Toolbar
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Create Post";

            //Notification
            CreateNotificationChannel();

            //ActionBar Back
            AndroidX.AppCompat.App.ActionBar actionBar = SupportActionBar;
            actionBar.SetDisplayHomeAsUpEnabled(true);
            actionBar.SetHomeAsUpIndicator(Resource.Drawable.outline_arrowback);
            
            submitButton.Click += SubmitButton_Click;
            postImage.Click += PostImage_Click;
            RequestPermissions(permissionGroup, 0);
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            postMap = new HashMap();
            postMap.Put("author", AppDataHelper.GetFullName());
            postMap.Put("owner_id", AppDataHelper.GetFirebaseAuth().CurrentUser.Uid);
            postMap.Put("post_date", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            postMap.Put("post_body", postEditText.Text);

            newPostRef = AppDataHelper.GetFirestore().Collection("posts").Document();
            string postKey = newPostRef.Id;

            postMap.Put("image_id", postKey);

            ShowProgressDialogue("Saving Information...");

            // Save Post Image to Firestore Storage
            if (fileBytes == null)
            {
                CloseProgressDialogue();
                Toast.MakeText(this, "Post image empty! Upload post image to upload", ToastLength.Short).Show();
                return;
            }
            else if (fileBytes != null)
            {
                storageReference = FirebaseStorage.Instance.GetReference("postImages/" + postKey);
                storageReference.PutBytes(fileBytes)
                    .AddOnProgressListener(this)
                    .AddOnSuccessListener(taskCompletionListeners)
                    .AddOnFailureListener(taskCompletionListeners);
            }

            // Image Upload Success Callback
            taskCompletionListeners.Sucess += (obj, args) =>
            {
                if (storageReference != null)
                {
                    storageReference.DownloadUrl.AddOnSuccessListener(downloadUrlListener);
                }
            };

            // Image Download URL Callback
            downloadUrlListener.Sucess += (obj, args) =>
            {
                string downloadUrl = args.Result.ToString();
                postMap.Put("download_url", downloadUrl);

                // Save post to Firebase Firestore
                newPostRef.Set(postMap);
                CloseProgressDialogue();
                Finish();

                // When the user clicks the notification, SecondActivity will start up.
                var resultIntent = new Intent(this, typeof(MainActivity));

                // Construct a back stack for cross-task navigation:
                var stackBuilder = TaskStackBuilder.Create(this);
                //stackBuilder.AddParentStack(Class.(typeof(MainActivity)));
                stackBuilder.AddNextIntent(resultIntent);

                // Create the PendingIntent with the back stack:
                var resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

                // Build the notification:
                var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                              .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                              .SetContentTitle("Post Succesful") // Set the title
                              .SetContentIntent(resultPendingIntent)
                              .SetSmallIcon(Resource.Drawable.FacePost) // This is the icon to display
                              .SetContentText($"Post Uploaded successfully"); // the message to display.

                // Finally, publish the notification:
                var notificationManager = NotificationManagerCompat.From(this);
                notificationManager.Notify(NOTIFICATION_ID, builder.Build());
            };


            // Image Upload Failure Callback
            taskCompletionListeners.Failure += (obj, args) =>
            {
                Toast.MakeText(this, "Upload was not completed", ToastLength.Short).Show();
            };
        }

        private void PostImage_Click(object sender, EventArgs e)
        {
            AndroidX.AppCompat.App.AlertDialog.Builder photoAlert = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            photoAlert.SetMessage("Change photo");

            photoAlert.SetNegativeButton("Take Photo", (thisalert, args) =>
            {
                //Capture image
                TakePhoto();
            });

            photoAlert.SetPositiveButton("Upload photo", (thisalert, args) =>
            {
                //Upload Photo
                SelectPhoto();
            });
            photoAlert.Show();
        }

        async void TakePhoto()
        {
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 20,
                Directory = "Sample",
                Name = GenerateRandomString(6) + "lexFacepost.jpg"
            });

            if(file == null)
            {
                return;
            }

            //Convert file.path to byte array and set the resulting bitmap to imageview
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            //fileBytes = imageArray.Select(x => (byte)x).ToArray();
            fileBytes = imageArray;

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            postImage.SetImageBitmap(bitmap);
        }

        async void SelectPhoto()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, "Upload not supported!", ToastLength.Short).Show();
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 40,
            });

            if(file == null)
            {
                return;
            }

            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            fileBytes = imageArray;

            Bitmap bitmap = BitmapFactory.DecodeByteArray((byte[])imageArray, 0, imageArray.Length);
            postImage.SetImageBitmap(bitmap);
        }

        void ShowProgressDialogue(string status)
        {
            progressDialogue = new progressDialogueFragment(status);
            var trans = SupportFragmentManager.BeginTransaction();
            progressDialogue.Cancelable = false;
            progressDialogue.Show(trans, "progress");
        }

        void CloseProgressDialogue()
        {
            if (progressDialogue != null)
            {
                progressDialogue.Dismiss();
                progressDialogue = null;
            }
        }

        string GenerateRandomString(int lenght)
        {
            System.Random rand = new System.Random();
            char[] allowchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            string sResult = "";
            for(int i = 0; i <= lenght; i++)
            {
                sResult += allowchars[rand.Next(0,allowchars.Length)];
            }
            return sResult;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            if (storageReference != null)
            {
                storageReference.DownloadUrl.AddOnSuccessListener(this);
            }

            //Image upload URL Callback
            string downloadUrl = result.ToString();
            postMap.Put("download_url", downloadUrl);

            //Save Post to firebase firestore
            newPostRef.Set(postMap);
            CloseProgressDialogue();
            Finish();

            // Build the notification:
            var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                          .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                          .SetContentTitle("Post Succesful") // Set the title
                          .SetSmallIcon(Resource.Drawable.FacePost) // This is the icon to display
                          .SetContentText($"Post Uploaded successfully"); // the message to display.

            // Finally, publish the notification:
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(NOTIFICATION_ID, builder.Build());
        }


        public void OnFailure(Java.Lang.Exception e)
        {
            Toast.MakeText(this, "Upload failed! : " + e.Message, ToastLength.Short).Show();
        }

        public void OnProgress(Java.Lang.Object snapshot)
        {
            //Toast.MakeText(this, "Please wait while image is uploading...", ToastLength.Short).Show();
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var name = Resources.GetString(Resource.String.channel_name);
            var description = GetString(Resource.String.channel_description);
            var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Default)
            {
                Description = description
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}