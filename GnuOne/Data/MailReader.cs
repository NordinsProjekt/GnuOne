using GnuOne.Data.Models;
using Library;
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
    //Read all incoming emails with Imap.
    //Every email sent from GNU, will have a specified, encrypted subject.
    //After decryption the program will match the subjet to a switch.
    //The switch will assign the appropiate method to read, save and re-send 
    //the content of the e-mail. When read, Imap flags the message as "has-been-read",
    //and deletes the e-mail from the G-mail inbox. 

    //The program is set to loop the Mailreader-class every 10 seconds,
    //for as long as GNU is running.
    public static class MailReader
    {
        public static async void ReadUnOpenEmails(MariaContext _newContext, string ConnectionString)
        {
            var myInfo = _newContext.MySettings.First();

            var pass = AesCryption.Decrypt(myInfo.Password, myInfo.Secret);

            using (var client = new ImapClient()) //**** new ProtocolLogger("imap.log")) if ypu want to log.
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect); 
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(myInfo.Email, pass);                                               
                client.Inbox.Open(FolderAccess.ReadWrite);


                var emails = client.Inbox.Search(SearchQuery.All);

                foreach (var mail in emails)
                {
                    var message = client.Inbox.GetMessage(mail);
                    var emailFrom = message.From.ToString();
                    string cleanEmailFrom = SkrubbaMailAdress(emailFrom);
                    string decryptedMessage = "";
                    var subject = message.Subject;
                    //Första säkerhetslagret, krypterat med aes (standard lösenord)
                    string body = message.GetTextBody(MimeKit.Text.TextFormat.Plain);
                    //splittar upp meddelandet, textmeddelandet, signaturen, aes lösenordet.
                    string[] splittedBody = body.Split("XYXY/(/(XYXY7");
                    string[] Sub;

                    if (subject.Contains("/()/"))
                    {
                        Sub = subject.Split("/()/");
                        if (splittedBody.Length == 3)
                        {
                            MegaCrypt tempCrypt = new MegaCrypt(splittedBody);
                            //Hämtar publickey för den som skickade.
                            var user = _newContext.MyFriends.FirstOrDefault(u => u.Email == emailFrom);
                            //Avkryptera
                            if (tempCrypt.RSADecryptIt(user.pubKey, myInfo.MyPrivKey))
                            {
                                //om sant så är allt ok.
                                decryptedMessage = tempCrypt.body;
                            }
                            else
                            {
                                //Något är fel med meddelandet. Ignorera.
                                continue;
                            }
                            
                        }
                        else
                        {
                            decryptedMessage = AesCryption.Decrypt(splittedBody[0], myInfo.Secret);
                        }


                        switch (Sub[1])
                        {
                            case "PostedDiscussion":

                                var done = RecieveAndSaveDiscussion(decryptedMessage, _newContext);

                                if (done == 1)
                                { break; }
                                else
                                {
                                    break;
                                }

                            case "PostedPost":

                                var doing = RecieveAndSavePost(decryptedMessage, _newContext, cleanEmailFrom, myInfo);
                                if (doing == 1)
                                {
                                    var doinga = ForwardToFriends(decryptedMessage, _newContext, cleanEmailFrom, "ForwardPost");
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            case "ForwardPost":
                                var doingb = RecieveAndSavePost(decryptedMessage, _newContext, cleanEmailFrom, myInfo);
                                if (doingb == 1)
                                { break; }
                                else
                                {
                                    break;
                                }

                            case "DeleteDiscussion":

                                var deed = RecieveAndDeleteDiscussion(decryptedMessage, _newContext);
                                if (deed == 1)
                                { break; }
                                else
                                {
                                    break;
                                }


                            case "ForwardDeletePost":
                                var deeding2 = ReceiveAndDeletePost(decryptedMessage, _newContext);
                                if (deeding2 == 1)
                                {
                                    break;
                                }
                                else
                                {
                                    break;
                                }


                            case "PutPost":
                                var deedb = ReceiveAndPutPost(decryptedMessage, _newContext);
                                if (deedb == 1)
                                {
                                    var doinga = ForwardToFriends(decryptedMessage, _newContext, cleanEmailFrom, "ForwardPutPost");
                                    break;
                                }
                                else
                                {
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
                                    break;
                                }

                            case "PutDiscussion":
                                var deedc = RecieveAndPutDiscussion(decryptedMessage, _newContext);
                                break;

                            case "PostComment":
                                var deed9 = RecieveAndSaveComment(decryptedMessage, _newContext, myInfo);
                                if (deed9 == 1)
                                {
                                    var doing91 = ForwardToFriends(decryptedMessage, _newContext, cleanEmailFrom, "ForwardComment");
                                }
                                break;

                            case "ForwardComment":

                                var deeed = RecieveAndSaveComment(decryptedMessage, _newContext, myInfo);
                                if (deeed == 1)
                                { break; }
                                break;

                            case "PutComment":
                                var deeed1 = RecieveAndPutComment(decryptedMessage, _newContext, myInfo);
                                if (deeed1 == 1)
                                {
                                    var doinga = ForwardToFriends(decryptedMessage, _newContext, cleanEmailFrom, "ForwardPutComment");
                                    break;
                                }
                                else
                                { break; }

                            case "ForwardPutComment":

                                var deeed2 = RecieveAndPutComment(decryptedMessage, _newContext, myInfo);
                                if (deeed2 == 1)
                                { break; }
                                else
                                { break; }

                            case "FriendRequest":
                                var deedd = RecieveFriendRequest(decryptedMessage, _newContext);
                                if (deedd == 1)
                                { break; }
                                else
                                { break; }

                            case "PutFriendsProfile":
                                var deed3 = RecieveAndUpdateFriend(decryptedMessage, _newContext);
                                if (deed3 == 1)
                                {
                                    var deeding12 = ForwardToFriends(decryptedMessage, _newContext, cleanEmailFrom, "FriendFriendsPicID");
                                    break;
                                }
                                break;

                            case "DeniedFriendRequest":

                                var deede = RecieveDeniedFriendRequest(decryptedMessage, _newContext);
                                if (deede == 1)
                                { break; }
                                else
                                {
                                    break;
                                }

                            case "AcceptedFriendRequest":
                                var deedf = ReceieveInfoAndAcceptFriend(decryptedMessage, _newContext, false, myInfo.Email, Sub[1]);
                                if (deedf == 1)
                                {
                                    var returnInfo = GiveBackMyInformation(_newContext, cleanEmailFrom);
                                    break;
                                }
                                else { break; }

                            case "GiveBackInformation":
                                var deedg = ReceieveInfoAndAcceptFriend(decryptedMessage, _newContext, true, myInfo.Email, Sub[1]);
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

                            case "UpdatedUsername":
                                var deed6 = UpdateFriendsUsername(decryptedMessage, _newContext, cleanEmailFrom, myInfo);
                                if (deed6 == 1) { break; }
                                else { break; }

                            case "UpdatedFriendsFriendsUsername":
                                var deed7 = UpdateFriendsFriendsUsername(decryptedMessage, _newContext, cleanEmailFrom);
                                if (deed7 == 1) { break; }
                                else { break; }

                            case "FriendFriendsPicID":
                                var edde1 = UpdateFriendsFriendsPicID(decryptedMessage, _newContext, cleanEmailFrom);
                                if (edde1 == 1) { break; }
                                else { break; }


                            case "DirectMessage":
                                var edde = RecieveAndSaveMessage(decryptedMessage, _newContext);
                                if (edde == 1) { break; }
                                else { break; }


                            default:
                                break;
                        }

                        // Flag message to "been-read" and deletes.
                        client.Inbox.AddFlags(mail, MessageFlags.Deleted, true);  
                        /*Backup.BackupDatabase();*/ // Code for backup goes here.
                    }
                    //Spamm-Email will be deleted.
                    else
                    {
                        client.Inbox.AddFlags(mail, MessageFlags.Deleted, true);
                        continue;
                    }
                }
                client.Disconnect(true);
            }
        }

        private static int UpdateFriendsFriendsPicID(string decryptedMessage, MariaContext context, string cleanEmailFrom)
        {
            var friendsfriendsPicIDAndInfo = JsonConvert.DeserializeObject<MyFriend>(decryptedMessage);
            if (friendsfriendsPicIDAndInfo is not null)
            {
                var friendFriendToChange = context.MyFriendsFriends.Where(x => x.Email == friendsfriendsPicIDAndInfo.Email && x.myFriendEmail == cleanEmailFrom).FirstOrDefault();
                friendFriendToChange.pictureID = friendsfriendsPicIDAndInfo.pictureID;
                context.MyFriendsFriends.Update(friendFriendToChange);
                context.SaveChangesAsync().Wait();
                return 1;
            }
            return -1;
        }


        private static int RecieveAndSaveMessage(string decryptedMessage, MariaContext context)
        {
            var message = JsonConvert.DeserializeObject<Message>(decryptedMessage);
            if (message is not null)
            {
                context.Messages.Add(message);
                context.SaveChangesAsync().Wait();

                return 1;
            }
            return -1;
        }

        private static int RecieveAndPutComment(string decryptedMessage, MariaContext context, MySettings myinfo)
        {
            var comment = JsonConvert.DeserializeObject<Comment>(decryptedMessage);
            if (comment is not null)
            {
                if (CheckifIHavePost(comment, context))
                {
                    if (comment.Email != myinfo.Email)
                    {
                        context.Update(comment);
                        context.SaveChangesAsync().Wait();
                    }
                    return 1;
                }
            }
            return -1;
        }

        private static int RecieveAndSaveComment(string decryptedMessage, MariaContext context, MySettings myinfo)
        {
            {
                var comment = JsonConvert.DeserializeObject<Comment>(decryptedMessage);
                if (comment is not null)
                {
                    if (CheckifIHavePost(comment, context))
                    {
                        if (comment.Email != myinfo.Email)
                        {
                            context.Comments.Add(comment);
                            if (myinfo.Email == comment.postEmail)
                            {

                                if (context.Notifications.Where(x => x.infoID == comment.postID && x.hasBeenRead == false).Any())
                                {
                                    var a = context.Notifications.Where(x => x.infoID == comment.postID && x.hasBeenRead == false).FirstOrDefault();
                                    a.info = comment.userName;
                                    context.Notifications.Update(a);
                                    a.counter++;
                                }
                                else
                                {
                                    Notification not = new Notification("Comment", comment.Email, comment.userName, comment.postID);
                                    context.Notifications.Add(not);
                                }
                            }

                            var bookmarklist = context.Bookmarks.ToList();
                            foreach (var bookmark in bookmarklist)
                            {
                                if (bookmark.ID == comment.postID && myinfo.Email != comment.postEmail)
                                {
                                    if (context.Notifications.Where(x => x.infoID == comment.postID && x.hasBeenRead == false).Any())
                                    {
                                        var a = context.Notifications.Where(x => x.infoID == comment.postID && x.hasBeenRead == false).FirstOrDefault();
                                        a.info = comment.userName;
                                        context.Notifications.Update(a);
                                        a.counter++;
                                    }
                                    else
                                    {
                                        Notification bookmarknot = new Notification("Comment", comment.Email, comment.userName, comment.postID);
                                        context.Notifications.Add(bookmarknot);
                                    }


                                }
                            }

                            context.SaveChangesAsync().Wait();
                        }
                        return 1;
                    }
                }
                return -1;
            }
        }

        private static int UpdateFriendsFriendsUsername(string decryptedMessage, MariaContext context, string cleanEmailFrom)
        {
            var friendsFriendsPosts = context.Posts.Where(x => x.Email == cleanEmailFrom).ToList();
            var friendsFriendsComments = context.Comments.Where(x => x.Email == cleanEmailFrom).ToList();
            var newUsername = JsonConvert.DeserializeObject<string>(decryptedMessage);
            var friendFriendUsername = context.MyFriendsFriends.Where(x => x.Email == cleanEmailFrom).ToList();

            if (friendsFriendsPosts.Count() > 0)
            {
                foreach (var post in friendsFriendsPosts)
                {
                    post.userName = newUsername;
                }

                context.Posts.UpdateRange(friendsFriendsPosts);
                context.SaveChangesAsync().Wait();
            }

            if (friendsFriendsComments.Count() > 0)
            {
                foreach (var comment in friendsFriendsComments)
                {
                    comment.userName = newUsername;
                }

                context.Comments.UpdateRange(friendsFriendsComments);
                context.SaveChangesAsync().Wait();
            }

            foreach (var friendFriend in friendFriendUsername)
            {
                friendFriend.userName = newUsername;
            }

            context.MyFriendsFriends.UpdateRange(friendFriendUsername);
            context.SaveChangesAsync().Wait();

            return 1;
        }

        private static int UpdateFriendsUsername(string decryptedMessage, MariaContext context, string cleanEmailFrom, MySettings mySettings)
        {
            var myDiscussions = context.Discussions.Where(x => x.Email == cleanEmailFrom).ToList();
            var myPosts = context.Posts.Where(x => x.Email == cleanEmailFrom).ToList();
            var myComments = context.Comments.Where(x => x.Email == cleanEmailFrom).ToList();
            var newUsername = JsonConvert.DeserializeObject<string>(decryptedMessage);
            var friendFriendUsername = context.MyFriendsFriends.Where(x => x.Email == cleanEmailFrom).ToList();

            var friend = context.MyFriends.Where(x => x.Email == cleanEmailFrom).FirstOrDefault();
            friend.userName = newUsername;
            context.MyFriends.Update(friend);
            context.SaveChangesAsync().Wait();

            foreach (var discussion in myDiscussions)
            {
                discussion.userName = newUsername;
            }

            foreach (var post in myPosts)
            {
                post.userName = newUsername;
            }

            foreach (var comment in myComments)
            {
                comment.userName = newUsername;
            }

            foreach (var friendFriend in friendFriendUsername)
            {
                friendFriend.userName = newUsername;
            }
            context.MyFriendsFriends.UpdateRange(friendFriendUsername);
            context.SaveChangesAsync().Wait();

            context.Discussions.UpdateRange(myDiscussions);
            context.SaveChangesAsync().Wait();

            context.Posts.UpdateRange(myPosts);
            context.SaveChangesAsync().Wait();

            context.Comments.UpdateRange(myComments);
            context.SaveChangesAsync().Wait();

            return 1;
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
                friend.pubKey = friendInfo.pubKey;

                context.MyFriends.Update(friend);
                context.SaveChangesAsync().Wait();

                var myFriendInFriendFriends = context.MyFriendsFriends.Where(x => x.myFriendEmail == friend.Email).FirstOrDefault();
                myFriendInFriendFriends.pictureID = friendInfo.pictureID;

                return 1;
            }
            return -1;
        }


        private static int ForwardToFriends(string decryptedMessage, MariaContext context, string fromEmail, string subject)
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
            string temp = JsonConvert.DeserializeObject<string>(decryptedMessage);
            var newFriendFriends = JsonConvert.DeserializeObject<MyFriendsFriends>(temp);

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
                var allOurMessage = context.Messages.Where(x => x.To == theirSettings.Email || x.From == theirSettings.Email).ToList();
                context.Messages.RemoveRange(allOurMessage);
                var stupidFriend = context.MyFriends.Where(x => x.Email == theirSettings.Email).FirstOrDefault();
                var theirDiscussion = context.Discussions.Where(x => x.Email == theirSettings.Email).ToList();
                context.Discussions.RemoveRange(theirDiscussion);
                context.MyFriends.Remove(stupidFriend);
                context.SaveChangesAsync().Wait();
                ///Send information that my friend is removed.
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

            var friend = context.MyFriends.Where(x => x.Email == toEmail).FirstOrDefault();
            //Send my friendFriend to my friends.
            var friendFriendNew = new MyFriendsFriends(friend, mysettingsEmail);
            var jsonFriendFriendNew = JsonConvert.SerializeObject(friendFriendNew);

            foreach (var user in context.MyFriends)
            {
                if (user.isFriend == false) { continue; }
                MailSender.SendObject(jsonFriendFriendNew, user.Email, mySettings, "FriendGotAFriend");
            }

            return 1;

        }

        private static int ReceieveInfoAndAcceptFriend(string decryptedMessage, MariaContext context, bool isSendBack, string myEmail, string subject)
        {
            string temp = JsonConvert.DeserializeObject<string>(decryptedMessage);
            var theirLists = JsonConvert.DeserializeObject<BigList>(temp);

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
                    friend.pubKey = theirLists.myInfo.pubKey;
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
                }
                var theirDiscussion = theirLists.Discussions;
                if (theirDiscussion is not null)
                {
                    context.Discussions.AddRangeAsync(theirDiscussion);
                }
                var theirPosts = theirLists.Posts;
                if (theirPosts is not null)
                {
                    context.Posts.AddRangeAsync(theirPosts);
                }
                if (subject == "AcceptedFriendRequest")
                {
                    var notification = new Notification("FriendRequestAccepted", friend.Email, friend.userName);
                    context.Notifications.Add(notification);
                }
                context.SaveChangesAsync().Wait();
                return 1;
            }
            return -1;
        }

        private static int RecieveDeniedFriendRequest(string decryptedMessage, MariaContext context)
        {
            string temp = JsonConvert.DeserializeObject<string>(decryptedMessage);
            var notNewFriend = JsonConvert.DeserializeObject<MyFriend>(temp);

            if (notNewFriend is not null)
            {
                var myfriend = context.MyFriends.Where(x => x.Email == notNewFriend.Email).FirstOrDefault();
                context.MyFriends.Remove(myfriend);
                var notification = new Notification("FriendRequestDenied", notNewFriend.Email, notNewFriend.userName);
                context.Notifications.Add(notification);
                context.SaveChangesAsync().Wait();
                return 1;
            }
            return -1;
        }

        private static int RecieveFriendRequest(string decryptedMessage, MariaContext context)
        {
            string temp = JsonConvert.DeserializeObject<string>(decryptedMessage);
            var potentialfriend = JsonConvert.DeserializeObject<MyFriend>(temp);

            if (potentialfriend is not null)
            {
                context.MyFriends.Add(potentialfriend);
                var notification = new Notification("FriendRequestRecieved", potentialfriend.Email, potentialfriend.userName);
                context.Notifications.Add(notification);
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
        private static bool CheckifIHavePost(Comment commment, MariaContext context)
        {
            var anyPost = context.Posts.Where(x => x.ID == commment.postID).Any();

            return anyPost;
        }

        private static int RecieveAndDeleteDiscussion(string decryptedMessage, MariaContext context)
        {
            var discussion = JsonConvert.DeserializeObject<Discussion>(decryptedMessage);
            

            if (discussion is not null)
            {
                var resultDisc = context.Notifications.Where(x => x.ID == discussion.ID).FirstOrDefault();
                context.Notifications.Remove(resultDisc);
                context.SaveChangesAsync().Wait();

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
                        if (myInfo.Email == post.discussionEmail)
                        {
                            if (context.Notifications.Where(x => x.infoID == post.discussionID && x.hasBeenRead == false).Any())
                            {
                                var a = context.Notifications.Where(x => x.infoID == post.discussionID && x.hasBeenRead == false).FirstOrDefault();
                                a.info = post.userName;
                                context.Notifications.Update(a);
                                a.counter++;
                            }
                            else
                            {
                                Notification not = new Notification("Post", Email, post.userName, post.discussionID);
                                context.Notifications.Add(not);
                            }
                        }
                        var bookmarklist = context.Bookmarks.ToList();
                        foreach (var bookmark in bookmarklist)
                        {
                            if (bookmark.ID == post.discussionID && myInfo.Email != post.discussionEmail)
                            {
                                if (context.Notifications.Where(x => x.infoID == post.discussionID && x.hasBeenRead == false).Any())
                                {
                                    var a = context.Notifications.Where(x => x.infoID == post.discussionID && x.hasBeenRead == false).FirstOrDefault();
                                    a.info = post.userName;
                                    context.Notifications.Update(a);
                                    a.counter++;
                                }
                                else
                                {
                                    Notification bookmarknot = new Notification("Post", Email, post.userName, post.discussionID);

                                    context.Notifications.Add(bookmarknot);
                                }
                            }
                        }
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