using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using FacePost.Fragments;
using FacePost.Helpers;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Google.Android.Material.TextField;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacePost.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class RegistrationActivity : AppCompatActivity, IOnSuccessListener, IOnFailureListener
    {
        Button registerButton;
        TextInputLayout fullNameRegtext, emailRegText, passwordRegText, confirmPasswordRegText;
        TextView clickToLogin;
        string fullname, email, password, confirm;

        FirebaseFirestore database;
        FirebaseAuth mAuth;
        progressDialogueFragment progressDialogue;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register);
            fullNameRegtext = FindViewById<TextInputLayout>(Resource.Id.fullNameRegtext);
            emailRegText = FindViewById<TextInputLayout>(Resource.Id.emailRegText);
            passwordRegText = FindViewById<TextInputLayout>(Resource.Id.passwordRegText);
            confirmPasswordRegText = FindViewById<TextInputLayout>(Resource.Id.confirmPasswordRegText);
            registerButton = FindViewById<Button>(Resource.Id.registerButton);
            clickToLogin = FindViewById<TextView>(Resource.Id.clickToLogin);

            clickToLogin.Click += ClickToLogin_Click;
            registerButton.Click += RegisterButton_Click;
            database = AppDataHelper.GetFirestore();
            mAuth = AppDataHelper.GetFirebaseAuth();
            // Create your application here
        }

        private void ClickToLogin_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {

            fullname = fullNameRegtext.EditText.Text;
            email = emailRegText.EditText.Text;
            password = passwordRegText.EditText.Text;
            confirm = confirmPasswordRegText.EditText.Text;

            if(fullname.Length < 4)
            {
                Toast.MakeText(this, "Please enter a valid full name", ToastLength.Short).Show();
                return;
            }
            else if (!email.Contains("@"))
            {
                Toast.MakeText(this, "Please enter a valid email address", ToastLength.Short).Show();
                return;
            }
            else if(password.Length < 8)
            {
                Toast.MakeText(this, "Password is too weak, Please enter a strong password!", ToastLength.Short).Show();
                return;
            }
            else if(confirm != password)
            {
                Toast.MakeText(this, "Password does not match, please make correction!", ToastLength.Short).Show();
                return;
            }

            //Perform Registration
            ShowProgressDialogue("Registering you...");
            mAuth.CreateUserWithEmailAndPassword(email, password).AddOnSuccessListener(this)
                .AddOnFailureListener(this);

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

        public void OnSuccess(Java.Lang.Object result)
        {
            HashMap userMap = new HashMap();
            userMap.Put("email", email);
            userMap.Put("fullname", fullname);

            DocumentReference userReference = database.Collection("users").Document(mAuth.CurrentUser.Uid);
            userReference.Set(userMap);

            CloseProgressDialogue();
            StartActivity(typeof(MainActivity));
            Finish();
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            CloseProgressDialogue();
            Toast.MakeText(this, "Registration Failed : " + e.Message, ToastLength.Short).Show();
        }
    }
}