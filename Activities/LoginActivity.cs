using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using FacePost.Activities;
using FacePost.Fragments;
using FacePost.Helpers;
using Firebase.Auth;
using Google.Android.Material.TextField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacePost
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class LoginActivity : AppCompatActivity, IOnSuccessListener, IOnFailureListener
    {

        TextInputLayout emailLogintext;
        TextInputLayout passwordLogintext;
        Button loginButton;
        TextView clickToRegister;

        FirebaseAuth mAuth;
        progressDialogueFragment progressDialogue;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.login);
            emailLogintext = FindViewById<TextInputLayout>(Resource.Id.emailLogintext);
            passwordLogintext = FindViewById<TextInputLayout>(Resource.Id.passwordLoginText);
            loginButton = FindViewById<Button>(Resource.Id.loginButton);
            clickToRegister = FindViewById<TextView>(Resource.Id.clickToRegister);

            clickToRegister.Click += ClickToRegister_Click;
            loginButton.Click += LoginButton_Click;
            mAuth = AppDataHelper.GetFirebaseAuth();

            // Create your application here
        }

        private void ClickToRegister_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(RegistrationActivity));
            Finish();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string email, password;
            email = emailLogintext.EditText.Text;
            password = passwordLogintext.EditText.Text;

            if (!email.Contains("@"))
            {
                Toast.MakeText(this, "Please enter a valid email address", ToastLength.Short).Show();
                return;
            }
            else if(password.Length < 8)
            {
                Toast.MakeText(this, "Please enter a valid password", ToastLength.Short).Show();
                return;
            }

            ShowProgressDialogue("Verifying you...");

            mAuth.SignInWithEmailAndPassword(email, password).AddOnSuccessListener(this)
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
            if(progressDialogue != null)
            {
                progressDialogue.Dismiss();
                progressDialogue = null;
            }
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            CloseProgressDialogue();
            Toast.MakeText(this, "Login Failed : " + e.Message, ToastLength.Short).Show();
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            CloseProgressDialogue();
            StartActivity(typeof(MainActivity));
            Toast.MakeText(this, "Login was successful", ToastLength.Short).Show();
        }
    }
}