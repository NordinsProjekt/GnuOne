﻿using Library;
using Library.HelpClasses;
using Library.Models;
using MailKit.Net.Pop3;
using Newtonsoft.Json;
using System.Text.Json;

namespace GnuOne.Data
{
    public static class MailReader
    {
        public static void ReadUnOpenEmails(MariaContext _newContext, string ConnectionString)
        {
            var me = _newContext.MySettings.First();
            using (var client = new Pop3Client())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                client.Connect("pop.gmail.com", 995, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(me.Email, me.Password);

                for (int i = 0; i < client.Count; i++)
                {
                    var message = client.GetMessage(i);
                    var subjet = message.Subject;
                    var body = message.GetTextBody(MimeKit.Text.TextFormat.Text);
                    string[] relativData = body.Split("XYXY/(/(XYXY7");
                    string[] Sub;

                    if (subjet.Contains("/()/"))
                    {
                        Sub = subjet.Split("/()/");
                        string decrypted = AesCryption.Decrypt(relativData[0], me.Secret);
                        string[] Data = decrypted.Split("\"");
                        var LocalDate = _newContext.LastUpdates.First();
                        DateTime IncomeDate = Convert.ToDateTime(Sub[0]);
                        if (IncomeDate > LocalDate.timeSet)
                        {
                            switch (Sub[1])
                            {
                                case "DELETE":

                                    DbCommand.CreateCommand(decrypted, ConnectionString);
                                    break;

                                case "PUT":
                                    DbCommand.CreateCommand(decrypted, ConnectionString);
                                    break;

                                case "friendRequest":
                                    var bodymessage = decrypted.Split("/()/");
                                    var potentialfriend = new MyFriend(bodymessage);
                                    _newContext.MyFriends.Add(potentialfriend);
                                    _newContext.SaveChangesAsync();

                                    break;
                                case "DeniedfriendRequest":
                                    var bodymessage1 = decrypted.Split("/()/");
                                    var myfriend = _newContext.MyFriends.Where(x => x.Email == bodymessage1[1]).FirstOrDefault();
                                    _newContext.MyFriends.Remove(myfriend);
                                    _newContext.SaveChangesAsync();
                                    break;

                                case "AcceptedfriendRequest":
                                    var bodymessages = decrypted.Split("/()/");
                                    var friend = _newContext.MyFriends.Where(x => x.Email == bodymessages[0]).FirstOrDefault();
                                    friend.isFriend = true;
                                    _newContext.Update(friend);
                                    _newContext.SaveChangesAsync();

                                    try
                                    {
                                        var deserializedItemsFromItems = System.Text.Json.JsonSerializer.Deserialize<List<Discussion>>(bodymessages[1]);
                                        if (deserializedItemsFromItems != null)
                                        {
                                            foreach (Discussion x in deserializedItemsFromItems)
                                            {
                                                Discussion discdisc = new Discussion() { ID = x.ID, Email = x.Email, userName = x.userName, Headline = x.Headline, discussionText = x.discussionText, Date = x.Date };
                                                _newContext.Discussions.Add(discdisc);
                                            };
                                            _newContext.SaveChangesAsync();
                                        }

                                        var deserializedItemsFromItems1 = System.Text.Json.JsonSerializer.Deserialize<List<Post>>(bodymessages[2]);
                                        if (deserializedItemsFromItems1 != null)
                                        {
                                            foreach (Post x in deserializedItemsFromItems1)
                                            {
                                                Post pospos = new Post() { ID = x.ID, Email = x.Email, userName = x.userName, Date = x.Date, postText = x.postText }; //Discussion ID!? DÖÖÖÖÖDEN! =D 
                                                _newContext.Posts.Add(x);
                                            };
                                            _newContext.SaveChangesAsync();
                                        }
                                        var deserializedItemsFromItems2 = System.Text.Json.JsonSerializer.Deserialize<List<MyFriendsFriends>>(bodymessages[3]);
                                        if (deserializedItemsFromItems2 != null)
                                        {
                                            foreach (MyFriendsFriends x in deserializedItemsFromItems2)
                                            {
                                                MyFriendsFriends friefrie = new MyFriendsFriends() { Email = x.Email, userName = x.userName, myFriendID = friend.ID };
                                                _newContext.MyFriendsFriends.Add(friefrie);
                                            };
                                            _newContext.SaveChangesAsync();
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                    break;

                                case "deleteFriend":
                                    var bodymessage3 = decrypted.Split("/()/");
                                    var allDiscussions = _newContext.Discussions.Where(x => x.Email == bodymessage3[1]).ToList();
                                    _newContext.RemoveRange(allDiscussions);
                                    var deletemyFriend = _newContext.MyFriends.Where(y => y.Email == bodymessage3[1]).FirstOrDefault();
                                    _newContext.MyFriends.Remove(deletemyFriend);
                                    _newContext.SaveChangesAsync();
                                    break;

                                default:
                                    DbCommand.CreateCommand(decrypted, ConnectionString);
                                    break;
                            }
                            LocalDate.timeSet = IncomeDate;
                            _newContext.LastUpdates.Update(LocalDate);
                            _newContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }
}
