﻿using Library;
using Library.HelpClasses;
using Library.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Search;
using MailKit.Security;
using Newtonsoft.Json;
using System.Text.Json;

namespace GnuOne.Data
{
    public static class MailReader
    {

        public static async void ReadUnOpenEmails(MariaContext _newContext, string ConnectionString)
        {
            var myInfo = _newContext.MySettings.First();

            var pass = AesCryption.Decrypt(myInfo.Password, myInfo.Secret);

            using (var client = new ImapClient()) //**** new ProtocolLogger("imap.log")) om vi vill logga
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect); //****
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(myInfo.Email, pass);                                                 //vad händer om man har fel lösen?
                client.Inbox.Open(FolderAccess.ReadWrite);


                var emails = client.Inbox.Search(SearchQuery.All);

                foreach (var mail in emails)
                {
                    var message = client.Inbox.GetMessage(mail);
                    var emailFrom = message.From.ToString();
                    string cleanEmailFrom = SkrubbaMailAdress(emailFrom);


                    var subject = message.Subject;
                    string body = message.GetTextBody(MimeKit.Text.TextFormat.Plain);
                    string[] splittedBody = body.Split("XYXY/(/(XYXY7");
                    string[] Sub;

                    if (subject.Contains("/()/"))
                    {
                        Sub = subject.Split("/()/");
                        string decryptedMessage = AesCryption.Decrypt(splittedBody[0], myInfo.Secret);
                        string[] Data = decryptedMessage.Split("\"");

                        switch (Sub[1])
                        {
                            case "PostedDiscussion":

                                var done = RecieveAndSaveDiscussion(decryptedMessage, _newContext);

                                if (done == 1)
                                { break; }
                                else
                                {
                                    ///try again?
                                    break;
                                }

                            case "PostedPost":

                                var doing = RecieveAndSavePost(decryptedMessage, _newContext, cleanEmailFrom, myInfo);
                                if (doing == 1)
                                {
                                    var doinga = ForwardPostToFriends(decryptedMessage, _newContext, cleanEmailFrom, "ForwardPost");
                                    break;
                                }
                                
                                else
                                {
                                    ///try again?
                                    break;
                                }

                            case "ForwardPost":
                                var doingb = RecieveAndSavePost(decryptedMessage, _newContext, cleanEmailFrom, myInfo);
                                if (doingb == 1)
                                { break; }
                                else
                                {
                                    ///try again?
                                    break;
                                }

                            case "DeleteDiscussion":

                                var deed = RecieveAndDeleteDiscussion(decryptedMessage, _newContext);
                                if (deed == 1)
                                { break; }
                                else
                                {
                                    ///try again?
                                    break;
                                }

                            //case "DeletePost":
                            //    var deeding = ReceiveAndDeletePost(decryptedMessage, _newContext);
                            //    if (deeding == 1)
                            //    { 
                            //        var deeding1 = ForwardPostToFriends(decryptedMessage, _newContext, cleanEmailFrom, "ForwardDeletePost");
                            //        break; }
                            //    else
                            //    {
                            //        ///try again?
                            //        break;
                            //    }

                            case "ForwardDeletePost":
                                var deeding2 = ReceiveAndDeletePost(decryptedMessage, _newContext);
                                if (deeding2 == 1)
                                {
                                    break;
                                }
                                else
                                {
                                    ///try again?
                                    break;
                                }


                            case "PutPost":
                                var deedb = ReceiveAndPutPost(decryptedMessage, _newContext);
                                if (deedb == 1)
                                {
                                    var doinga = ForwardPostToFriends(decryptedMessage, _newContext, cleanEmailFrom, "ForwardPutPost");
                                    break;
                                }
                                else
                                {
                                    ///try again?
                                    break;
                                }

                            case "ForwardPutPost":

                                var deed1 = ReceiveAndPutPost(decryptedMessage, _newContext);
                                if (deed1 == 1)
                                {
                                    break;
                                }
                                else
                                {
                                    ///try again?
                                    break;
                                }

                            case "PutDiscussion":
                                var deedc = RecieveAndPutDiscussion(decryptedMessage, _newContext);
                                break;


                            case "FriendRequest":
                                var deedd = RecieveFriendRequest(decryptedMessage, _newContext);
                                if (deedd == 1)
                                { break; }
                                else
                                {
                                    ///try again?
                                    break;
                                }
                            case "PutFriendsProfile":
                                var deed3 = RecieveAndUpdateFriend(decryptedMessage, _newContext);
                                if (deed3 == 1)
                                { break; }
                                else
                                {
                                    ///try again?
                                    break;
                                }

                            case "DeniedFriendRequest":

                                var deede = RecieveDeniedFriendRequest(decryptedMessage, _newContext);
                                if (deede == 1)
                                { break; }
                                else
                                {
                                    ///try again?
                                    break;
                                }

                            case "AcceptedFriendRequest":
                                //var deedf = await Task.Run(() => ReceieveInfoAndAcceptFriend(decryptedMessage, _newContext, false, myInfo.Email));
                                var deedf = ReceieveInfoAndAcceptFriend(decryptedMessage, _newContext, false, myInfo.Email);
                                if (deedf == 1)
                                {
                                    //await Task.Run(() => GiveBackMyInformation(_newContext, emailFrom));
                                    var returnInfo = GiveBackMyInformation(_newContext, cleanEmailFrom);
                                    break;
                                }
                                else { break; }

                            case "GiveBackInformation":
                                var deedg = ReceieveInfoAndAcceptFriend(decryptedMessage, _newContext, true, myInfo.Email);
                                if (deedg == 1) { break; }
                                else { break; }

                            case "ItsNotMeItsYou":
                                var deedh = RemoveFriendAndTheirInfoFromDb(decryptedMessage, _newContext);
                                if (deedh == 1) { break; }
                                else { break; }

                            case "FriendGotAFriend":
                                var deedi = UpdateFriendFriends(decryptedMessage, _newContext);
                                if (deedi == 1) { break; }
                                else { break; }


                            case "FriendsFriendGotRemoved":
                                var deedj = RemoveFriendsFriend(decryptedMessage, _newContext, cleanEmailFrom);
                                if (deedj == 1) { break; }
                                else { break; }



                            case "FriendHiding":
                                var deed4 = UpdateFriendHiding(decryptedMessage, _newContext, cleanEmailFrom);
                                if (deed4 == 1) { break; }
                                else { break; }

                            case "FriendShowing":
                                var deed5 = UpdateFriendShowing(decryptedMessage, _newContext, cleanEmailFrom);
                                if (deed5 == 1) { break; }
                                else { break; }

                            default:
                                break;
                        }

                        // Flaggar meddelandet att det skall tas bort.
                        client.Inbox.AddFlags(mail, MessageFlags.Deleted, true);  //false? testa
                        /*Backup.BackupDatabase();*/ // Backar upp dB
                    }

                    //Spammail kmr hit och tas bort
                    else
                    {
                        client.Inbox.AddFlags(mail, MessageFlags.Deleted, true);
                        continue;
                    }
                }
                client.Disconnect(true);

                //backupdatabase();
            }
        }

        private static int UpdateFriendShowing(string decryptedMessage, MariaContext context, string cleanEmailFrom)
        {
            var friendShowing = decryptedMessage;
            var friend = context.MyFriends.Where(x => x.Email == cleanEmailFrom).FirstOrDefault();

            if (friend != null)
            {
                friend.hideFriend = false;
                context.MyFriends.Update(friend);
                context.SaveChangesAsync().Wait();

                var mySettings = context.MySettings.FirstOrDefault();

                var friendfriend = new MyFriendsFriends(friend, mySettings.Email);
                var jsonFriendFriend = JsonConvert.SerializeObject(friendfriend);
                foreach (var user in context.MyFriends)
                {
                    MailSender.SendObject(jsonFriendFriend, user.Email, mySettings, "FriendGotAFriend");

                }

                return 1;
            }

            return -1;
        }


        private static int UpdateFriendHiding(string decryptedMessage, MariaContext context, string cleanEmailFrom)
        {
            var friendHiding = decryptedMessage;

            var friend = context.MyFriends.Where(x => x.Email == cleanEmailFrom).FirstOrDefault();
            if (friend != null)
            {
                friend.hideFriend = true;
                context.MyFriends.Update(friend);
                context.SaveChangesAsync().Wait();

                var mySettings = context.MySettings.FirstOrDefault();

                var jsonFriend = JsonConvert.SerializeObject(friend);
                foreach (var user in context.MyFriends)
                {
                    MailSender.SendObject(jsonFriend, user.Email, mySettings, "FriendsFriendGotRemoved");

                }

                return 1;
            }

            return -1;
        }

        private static int RecieveAndUpdateFriend(string decryptedMessage, MariaContext context)
        {
            var friendInfo = JsonConvert.DeserializeObject<MyFriend>(decryptedMessage);
            if (friendInfo != null)
            {
                var friend = context.MyFriends.Where(x => x.Email == friendInfo.Email).FirstOrDefault();
                friend.userInfo = friendInfo.userInfo;
                friend.pictureID = friendInfo.pictureID;
                friend.tagOne = friendInfo.tagOne;
                friend.tagTwo = friendInfo.tagTwo;
                friend.tagThree = friendInfo.tagThree;

                context.MyFriends.Update(friend);
                context.SaveChangesAsync().Wait();
                return 1;
            }
            return -1;
        }


        private static int ForwardPostToFriends(string decryptedMessage, MariaContext context, string fromEmail, string subject)
        {
            var mysettings = context.MySettings.FirstOrDefault();

            foreach (var friend in context.MyFriends)
            {
                if (friend.Email != fromEmail)
                {
                    if (friend.isFriend == false) { continue; }
                    MailSender.SendObject(decryptedMessage, friend.Email, mysettings, subject);
                }
            }
            return 1;

        }

        private static int UpdateFriendFriends(string decryptedMessage, MariaContext context)
        {
            var newFriendFriends = JsonConvert.DeserializeObject<MyFriendsFriends>(decryptedMessage);


            if (newFriendFriends is not null)
            {
                var isFriendAlready = context.MyFriendsFriends.Where(x => x.Email == newFriendFriends.Email && x.myFriendEmail == newFriendFriends.myFriendEmail).Any();
                if (isFriendAlready == false)
                {
                    context.MyFriendsFriends.Add(newFriendFriends);
                    context.SaveChangesAsync().Wait();
                    return 1;
                }
            }
            return -1;
        }

        private static int RemoveFriendsFriend(string decryptedMessage, MariaContext context, string fromEmail)
        {
            var friend = context.MyFriends.Where(x => x.Email == fromEmail).FirstOrDefault();

            var friendNotfriend = JsonConvert.DeserializeObject<MyFriend>(decryptedMessage);

            var removeablefriendfirend = context.MyFriendsFriends.Where(x => x.myFriendEmail == fromEmail && x.Email == friendNotfriend.Email).FirstOrDefault();
            if (removeablefriendfirend is not null)
            {
                context.MyFriendsFriends.Remove(removeablefriendfirend);
                context.SaveChangesAsync().Wait();
                return 1;
            }
            return -1;
        }

        private static int RemoveFriendAndTheirInfoFromDb(string decryptedMessage, MariaContext context)
        {
            var theirSettings = JsonConvert.DeserializeObject<MySettings>(decryptedMessage);

            if (theirSettings is not null)
            {
                var stupidFriend = context.MyFriends.Where(x => x.Email == theirSettings.Email).FirstOrDefault();
                var theirDiscussion = context.Discussions.Where(x => x.Email == theirSettings.Email).ToList();
                context.Discussions.RemoveRange(theirDiscussion);
                context.MyFriends.Remove(stupidFriend);
                context.SaveChangesAsync().Wait();
                ///skicka ut mail till mina vänner att dom är borta
                var jsonStupidFriend = JsonConvert.SerializeObject(stupidFriend);

                var mySettings = context.MySettings.FirstOrDefault();

                foreach (var user in context.MyFriends)
                {
                    if (user.isFriend == false) { continue; }
                    MailSender.SendObject(jsonStupidFriend, user.Email, mySettings, "FriendsFriendGotRemoved");
                }
                return 1;
            }
            return -1;
        }

        private static int GiveBackMyInformation(MariaContext context, string toEmail)
        {
            var mysettingsEmail = context.MySettings.Select(x => x.Email).Single();

            var myprofile = context.MyProfile.FirstOrDefault();


            var bigListWithMyInfo = BigList.FillingBigListWithMyInfo(context, mysettingsEmail, false, myprofile);
            var jsonBigList = JsonConvert.SerializeObject(bigListWithMyInfo);
            var mySettings = context.MySettings.FirstOrDefault();
            MailSender.SendObject(jsonBigList, toEmail, mySettings, "GiveBackInformation");

            ///vilken vän
            var friend = context.MyFriends.Where(x => x.Email == toEmail).FirstOrDefault();
            //gör om till friendfriend och skicka till mina vänner
            var friendFriendNew = new MyFriendsFriends(friend, mysettingsEmail);
            var jsonFriendFriendNew = JsonConvert.SerializeObject(friendFriendNew);

            foreach (var user in context.MyFriends)
            {
                if (user.isFriend == false) { continue; }
                MailSender.SendObject(jsonFriendFriendNew, user.Email, mySettings, "FriendGotAFriend");
            }

            return 1;

        }

        private static int ReceieveInfoAndAcceptFriend(string decryptedMessage, MariaContext context, bool isSendBack, string myEmail)
        {
            var theirLists = JsonConvert.DeserializeObject<BigList>(decryptedMessage);


            var email = theirLists.FromEmail;
            var username = theirLists.username;
            var friend = context.MyFriends.Where(x => x.Email == email).FirstOrDefault();


            if (theirLists is not null)
            {
                friend.isFriend = true;
                friend.userName = username.ToString();

                if (theirLists.myInfo != null)
                {
                    friend.userInfo = theirLists.myInfo.userInfo;
                    friend.pictureID = theirLists.myInfo.pictureID;
                    friend.tagOne = theirLists.myInfo.tagOne;
                    friend.tagTwo = theirLists.myInfo.tagTwo;
                    friend.tagThree = theirLists.myInfo.tagThree;
                }

                context.MyFriends.Update(friend);
                var theirFriends = theirLists.MyFriends;
                if (theirFriends is not null)
                {

                    var myFriendFriendsList = new List<MyFriendsFriends>();
                    foreach (var theirfriend in theirFriends)
                    {
                        if (theirfriend.Email != myEmail)
                        {
                            var bff = new MyFriendsFriends(theirfriend, friend.Email);

                            myFriendFriendsList.Add(bff);
                        }

                    }
                    context.MyFriendsFriends.AddRangeAsync(myFriendFriendsList);
                    //context.MyFriends.AddRange(theirFriends);
                }
                var theirDiscussion = theirLists.Discussions;
                if (theirDiscussion is not null)
                {
                    context.Discussions.AddRangeAsync(theirDiscussion);
                    //context.SaveChanges();
                }
                var theirPosts = theirLists.Posts;
                if (theirPosts is not null)
                {
                    context.Posts.AddRangeAsync(theirPosts);
                    //context.SaveChanges();
                }



                //if (!isSendBack)
                //{

                //}
                context.SaveChangesAsync().Wait();
                //context.SaveChangesAsync();
                return 1;
            }
            return -1;
        }

        private static int RecieveDeniedFriendRequest(string decryptedMessage, MariaContext context)
        {
            var notNewFriend = JsonConvert.DeserializeObject<MyFriend>(decryptedMessage);

            if (notNewFriend is not null)
            {
                var myfriend = context.MyFriends.Where(x => x.Email == notNewFriend.Email).FirstOrDefault();
                context.MyFriends.Remove(myfriend);
                context.SaveChangesAsync().Wait();
                return 1;
            }
            return -1;
        }

        private static int RecieveFriendRequest(string decryptedMessage, MariaContext context)
        {
            var potentialfriend = JsonConvert.DeserializeObject<MyFriend>(decryptedMessage);

            if (potentialfriend is not null)
            {
                context.MyFriends.Add(potentialfriend);
                context.SaveChangesAsync().Wait();
                return 1;
            }
            return -1;

        }

        private static int RecieveAndPutDiscussion(string decryptedMessage, MariaContext context)
        {
            var discussion = JsonConvert.DeserializeObject<Discussion>(decryptedMessage);
            if (discussion is not null)
            {
                context.Update(discussion);
                context.SaveChangesAsync().Wait();
                return 1;
            }
            return -1;
        }

        private static int ReceiveAndPutPost(string decryptedMessage, MariaContext context)
        {
            var post = JsonConvert.DeserializeObject<Post>(decryptedMessage);
            if (post is not null)
            {
                if (CheckIfIHaveDiscussion(post, context))
                {
                    context.Update(post);
                    context.SaveChangesAsync().Wait();
                    return 1;
                }
            }
            return -1;
        }

        private static int ReceiveAndDeletePost(string decryptedMessage, MariaContext context)
        {
            var post = JsonConvert.DeserializeObject<Post>(decryptedMessage);
            if (post is not null)
            {
                if (CheckIfIHaveDiscussion(post, context))
                {
                    context.Remove(post);
                    context.SaveChangesAsync().Wait();
                    return 1;
                }
            }
            return -1;
        }

        private static bool CheckIfIHaveDiscussion(Post post, MariaContext context)
        {
            var anyDisc = context.Discussions.Where(x => x.ID == post.discussionID).Any();

            return anyDisc;
        }

        private static int RecieveAndDeleteDiscussion(string decryptedMessage, MariaContext context)
        {
            var discussion = JsonConvert.DeserializeObject<Discussion>(decryptedMessage);
            if (discussion is not null)
            {
                context.Remove(discussion);
                context.SaveChangesAsync().Wait();
                return 1;
            }
            return -1;
        }
        private static int RecieveAndSavePost(string decryptedbody, MariaContext context, string Email, MySettings myInfo)
        {
            var post = JsonConvert.DeserializeObject<Post>(decryptedbody);
            if (post is not null)
            {
                if (CheckIfIHaveDiscussion(post, context))
                {
                    if (Email != myInfo.Email)
                    {
                        context.Posts.Add(post);
                        context.SaveChangesAsync().Wait();
                    }
                    return 1;
                }
            }
            return -1;
        }

        private static int RecieveAndSaveDiscussion(string decryptedbody, MariaContext context)
        {


            var discussion = JsonConvert.DeserializeObject<Discussion>(decryptedbody);
            if (discussion is not null)
            {
                context.Discussions.Add(discussion);
                context.SaveChangesAsync().Wait();

                return 1;
            }
            return -1;
        }

        public static string SkrubbaMailAdress(string fromMail)
        {
            int startindex = fromMail.IndexOf('<') + 1;
            int endindex = fromMail.IndexOf('>');
            string cleanMail = fromMail.Substring(startindex, (endindex - startindex));
            return cleanMail;
        }
    }
}