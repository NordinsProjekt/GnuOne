using GnuOne.Data.Models;
using Library;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace GnuOne.Data
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
        }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<MyFriend> MyFriends { get; set; }
        public DbSet<MyFriendsFriends> MyFriendsFriends { get; set; }
        public DbSet<MySettings> MySettings { get; set; }
        public DbSet<Tag> tags { get; set; }
        public DbSet<myProfile> MyProfile { get; set; }
        public DbSet<standardpictures> Standardpictures { get; set; }

        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Message> Messages { get; set; }
    }
}

