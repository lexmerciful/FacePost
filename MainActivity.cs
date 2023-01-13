using Android.App;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using FacePost.Activities;
using FacePost.Adapter;
using FacePost.DataModels;
using FacePost.EventListeners;
using FacePost.Fragments;
using FacePost.Helpers;
using Firebase.Storage;
using Java.Lang;
using System.Collections.Generic;
using System.Linq;

namespace FacePost
{
    
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, IOnSuccessListener, IOnFailureListener
    {

        AndroidX.AppCompat.Widget.Toolbar toolbar;
        RelativeLayout layStatus;
        ImageView cameraImage;
        RecyclerView postRecyclerView;
        PostAdapter postAdapter;
        List<Post> ListOfPost;
        PostEventListener postEventListener;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            
            toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            layStatus = FindViewById<RelativeLayout>(Resource.Id.layStatus);
            cameraImage = FindViewById<ImageView>(Resource.Id.camera);
            
            SetSupportActionBar(toolbar);

            postRecyclerView = (RecyclerView)FindViewById(Resource.Id.postRecycleView);

            layStatus.Click += LayStatus_Click;
            cameraImage.Click += CameraImage_Click;

            //Retrieve Fullname on Login
            FullnameListener fullnameListener = new FullnameListener();
            fullnameListener.FetchUser();

            FetchPost();
            //CreateData();
            
        }

        private void CameraImage_Click(object sender, System.EventArgs e)
        {
            StartActivity(typeof(CreatePostActivity));
        }

        private void LayStatus_Click(object sender, System.EventArgs e)
        {
            StartActivity(typeof(CreatePostActivity));
        }

        //Referencing PostEventListener and fetching post
        void FetchPost()
        {
            postEventListener = new PostEventListener();
            postEventListener.FetchPost();
            postEventListener.OnPostRetieved += PostEventListener_OnPostRetieved;
        }

        private void PostEventListener_OnPostRetieved(object sender, PostEventListener.PostEventArgs e)
        {
            ListOfPost = new List<Post>();
            ListOfPost = e.Posts;

            if(ListOfPost != null)
            {
                ListOfPost = ListOfPost.OrderByDescending(x => x.PostDate).ToList();
            }

            SetupRecyclerView();
        }

        void CreateData()
        {
            ListOfPost = new List<Post>();
            ListOfPost.Add(new Post { PostBody = "But I must explain to you how all this mistaken idea of denouncing pleasure and praising pain was born and I will give you a complete account of the system, and expound the actual teachings of the great explorer of the truth", Author= "Lexx Merciful", LikeCount = 762 });
            ListOfPost.Add(new Post { PostBody = "Happy new week, Jesus loves you!", Author = "Afolabi Blessing", LikeCount = 2 });
            ListOfPost.Add(new Post { PostBody = "we denounce with righteous indignation and dislike men who are so beguiled and demoralized by the charms of pleasure of the moment, so blinded by desire, that they cannot foresee", Author = "Afolabi Marvellous", LikeCount = 12 });
            ListOfPost.Add(new Post { PostBody = "What In The F**kery is this, so diabolical and inhumane to reason. I rather just doze off mother earth and into the oblivious space", Author = "Afolabi Blessing", LikeCount = 5 });
            ListOfPost.Add(new Post { PostBody = "Machala no.1 #Bigwiz #Morelovelessego", Author = "Dawil Fred", LikeCount = 251 });
            ListOfPost.Add(new Post { PostBody = "My piece of art", Author = "Adeshewa Milores", LikeCount = 97 });
        }

        void SetupRecyclerView()
        {
            postRecyclerView.SetLayoutManager(new LinearLayoutManager(postRecyclerView.Context));
            postAdapter = new PostAdapter(ListOfPost);
            postRecyclerView.SetAdapter(postAdapter);

            postAdapter.ItemLongClick += PostAdapter_ItemLongClick;
            postAdapter.LikeClick += PostAdapter_LikeClick;
            postAdapter.PostImageClick += PostAdapter_PostImageClick;
        }

        private void PostAdapter_PostImageClick(object sender, PostAdapterClickEventArgs e)
        {
            PostImageClickFragment postImageClickFragment = new PostImageClickFragment(ListOfPost[e.Position]);
            var trans = SupportFragmentManager.BeginTransaction();
            postImageClickFragment.Show(trans, "image");
        }

        private void PostAdapter_LikeClick(object sender, PostAdapterClickEventArgs e)
        {
            Post post = ListOfPost[e.Position];
            LikeEventListener likeEventListener = new LikeEventListener(post.ID);

            if (!post.Liked)
            {
                likeEventListener.LikePost();
            }
            else
            {
                likeEventListener.UnlikePost();
            }
        }

        private void PostAdapter_ItemLongClick(object sender, PostAdapterClickEventArgs e)
        {
            string postID = ListOfPost[e.Position].ID;
            string ownerID = ListOfPost[e.Position].OwnerId;

            if(AppDataHelper.GetFirebaseAuth().CurrentUser.Uid == ownerID)
            {
                AndroidX.AppCompat.App.AlertDialog.Builder alert = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
                alert.SetTitle("Do you wish to Edit or Delete Post");
                alert.SetMessage("Are you sure?");

                //Edit post on firestore
                alert.SetPositiveButton("Edit Post", (o, args) =>
                {
                    EditPostFragment editPostFragment = new EditPostFragment(ListOfPost[e.Position]);
                    var trans = SupportFragmentManager.BeginTransaction();
                    editPostFragment.Show(trans, "edit");
                });

                alert.SetNegativeButton("Delete Post", (o, args) =>
                {
                    AppDataHelper.GetFirestore().Collection("posts").Document(postID).Delete();
                    StorageReference storageReference = FirebaseStorage.Instance.GetReference("postImages/" + postID);
                    storageReference.Delete().AddOnSuccessListener(this)
                    .AddOnFailureListener(this);
                });

                alert.Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.feed_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if(id == Resource.Id.action_logout)
            {
                postEventListener.RemoveListener();

                AppDataHelper.GetFirebaseAuth().SignOut();
                StartActivity(typeof(LoginActivity));
                Finish();
            }

            else if(id == Resource.Id.action_refresh)
            {
                Toast.MakeText(this, "Refresh was clicked", ToastLength.Short).Show();
            }
            return base.OnOptionsItemSelected(item);
        }

        public void OnSuccess(Object result)
        {
            Toast.MakeText(this, "Post deleted successfully", ToastLength.Short).Show();
        }

        public void OnFailure(Exception e)
        {
            Toast.MakeText(this, "Post delete failed : " + e.Message, ToastLength.Short).Show();
        }
    }
}