using Android.Views;
using Android.Widget;
using System;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;
using FacePost.DataModels;
using FFImageLoading;

namespace FacePost.Adapter
{
    internal class PostAdapter : RecyclerView.Adapter
    {
        public event EventHandler<PostAdapterClickEventArgs> ItemClick;
        public event EventHandler<PostAdapterClickEventArgs> ItemLongClick;
        public event EventHandler<PostAdapterClickEventArgs> LikeClick;
        public event EventHandler<PostAdapterClickEventArgs> PostImageClick;
        List<Post> items;

        public PostAdapter(List<Post> data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = null;

            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.post, parent, false);

            var vh = new PostAdapterViewHolder(itemView, OnClick, OnLongClick, OnLike, OnPostImage);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as PostAdapterViewHolder;
            //holder.TextView.Text = items[position];

            holder.usernameTextView.Text = item.Author;
            holder.postBodyTextView.Text = item.PostBody;
            holder.likeCountTextView.Text = item.LikeCount.ToString() + " Likes";

            if (item.Liked)
            {
                holder.likeImageView.SetImageResource(Resource.Drawable.redlike);
            }
            else
            {
                holder.likeImageView.SetImageResource(Resource.Drawable.like);
            }

            GetImage(item.DownloadUrl, holder.postImageView);
        }

        void GetImage(string url, ImageView imageView)
        {
            ImageService.Instance.LoadUrl(url)
                .Retry(3, 200)
                .DownSample(400, 400)
                .Into(imageView);
        }

        public override int ItemCount => items.Count;

        void OnClick(PostAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(PostAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        void OnLike(PostAdapterClickEventArgs args) => LikeClick?.Invoke(this, args);
        void OnPostImage(PostAdapterClickEventArgs args) => PostImageClick?.Invoke(this, args);

    }

    public class PostAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public TextView usernameTextView, postBodyTextView, likeCountTextView;
        public ImageView postImageView, likeImageView;


        public PostAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener,
                            Action<PostAdapterClickEventArgs> longClickListener, Action<PostAdapterClickEventArgs> likeClickListener, Action<PostAdapterClickEventArgs> PostImageClickListener) : base(itemView)
        {
            //TextView = v;
            usernameTextView = itemView.FindViewById<TextView>(Resource.Id.usernameTextView);
            postBodyTextView = itemView.FindViewById<TextView>(Resource.Id.postBodyTextView);
            likeCountTextView = itemView.FindViewById<TextView>(Resource.Id.likeText);
            postImageView = itemView.FindViewById<ImageView>(Resource.Id.postImage);
            likeImageView = itemView.FindViewById<ImageView>(Resource.Id.likeButton);

            itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            likeImageView.Click += (sender, e) => likeClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            postImageView.Click += (sender, e) => PostImageClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class PostAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}