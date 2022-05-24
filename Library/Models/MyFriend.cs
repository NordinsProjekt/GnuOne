using Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    public class MyFriend
    {
        [Key]
        public int ID { get; set; }
        public string userName { get; set; } = String.Empty;
        public string Email { get; set; }
        public bool? isFriend { get; set; } = false;
        public string pubKey { get; set; }
        public string userInfo { get; set; }
        public int pictureID { get; set; }
        
        public int? tagOne { get; set; }
        public int? tagTwo { get; set; }
        public int? tagThree { get; set; }
        public bool hideMe { get; set; } = false;
        public bool hideFriend { get; set; } = false;

        public MyFriend()
        {

        }
        public MyFriend(string[] bodymessage)
        {
            this.userName = bodymessage[0];
            this.Email = bodymessage[1];
        }

        public MyFriend(myProfile Profile)
        {
            this.Email = Profile.Email;
            this.userInfo = Profile.myUserInfo;
            this.pictureID = Profile.pictureID;
            this.tagOne = Profile.tagOne;
            this.tagTwo = Profile.tagTwo;
            this.tagThree = Profile.tagThree;
            this.pubKey = Profile.MyPubKey;

        }
    }

}
