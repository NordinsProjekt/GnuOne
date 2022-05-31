using GnuOne.Data;
using Library;
using Library.HelpClasses;
using Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace GnuOne.Controllers
{
    /// <summary>
    /// Controller for establishing connection between users
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MyFriendsController : ControllerBase
    {
        private ApiContext _context;
        private MySettings _settings;

        public MyFriendsController(ApiContext context)
        {
            _context = context;
            _settings = _context.MySettings.First();
        }

        /// <summary>
        /// Sends FriendRequest with the User's information
        /// </summary>

        [HttpPost]
        public async Task<IActionResult> PostSendFriendRequest([FromBody] MyFriend Email)
        {
            var potentialnewfriend = new MyFriend();
            potentialnewfriend.Email = Email.Email;

            var myProfile = await _context.MyProfile.FirstAsync();

            var myInfo = new MyFriend
            {
                Email = _settings.Email,
                userName = _settings.UserName,
                isFriend = false,
                userInfo = myProfile.myUserInfo,
                pictureID = myProfile.pictureID,
                tagOne = myProfile.tagOne,
                tagTwo = myProfile.tagTwo,
                tagThree = myProfile.tagThree,
                pubKey = myProfile.MyPubKey
            };

            var jsonMyInfoInObject = JsonConvert.SerializeObject(myInfo);
            //SerializeObject efter programmet har sparat
            MailSender.SendObject(jsonMyInfoInObject, Email.Email, _settings, "FriendRequest");

            _context.MyFriends.Add(potentialnewfriend);
            await _context.SaveChangesAsync();

            return Ok();
        }
        /// <summary>
        /// Get all the users friends
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var listaDiscussion = await _context.MyFriends.ToListAsync();
            var converted = JsonConvert.SerializeObject(listaDiscussion);

            return Ok(converted);
        }

        /// <summary>
        /// Finds a specific friend and their friends.
        /// </summary>
        /// <param name="email"></param>
        
        [HttpGet("{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var friend = _context.MyFriends.Where(x => x.Email == email).FirstOrDefault();
            var friendsfriends = _context.MyFriendsFriends.Where(x => x.myFriendEmail == email).ToList();

            JsonFF jsonFF = new JsonFF(friend, friendsfriends);
            var jsonFFSerialized = JsonConvert.SerializeObject(jsonFF);

            return Ok(jsonFFSerialized);
        }

        /// <summary>
        /// Accepts or Denies a friendrequest
        /// If denies, send mail to that user
        /// if Accepts. Sends them ALL my information so it ends up in their database. 
        /// </summary>
        /// <param name="potentialFriend"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> PutDeniedOrAcceptFriendRequest([FromBody] MyFriend potentialFriend)
        {
            var friend = await _context.MyFriends.Where(x => x.Email == potentialFriend.Email).FirstAsync();
            if (friend == null)
            {
                return BadRequest("Could not find friend with this email");
            }
            var myInfo = new MyFriend
            {
                Email = _settings.Email.ToString(),
                userName = _settings.UserName.ToString()
            };
            var jsonMyInfoInObject = JsonConvert.SerializeObject(myInfo);

            if (potentialFriend.isFriend == false)
            {
                
                MailSender.SendObject(jsonMyInfoInObject, friend.Email, _settings, "DeniedFriendRequest");

                _context.MyFriends.Remove(friend);
                await _context.SaveChangesAsync();

                return Ok("Dont want to be friends");
            }
            //Annars Accepterar vi vännen
                //Gör om ens discuss,vänner och post till JSON och sänder iväg ett mail
            else
            {
                friend.isFriend = true;
                var myProfile = await _context.MyProfile.FirstOrDefaultAsync();

                var bigListWithMyInfo = BigList.FillingBigListWithMyInfo(_context, myInfo.Email, true, myProfile);
                bigListWithMyInfo.username = _settings.UserName.ToString();

                var jsonBigListObject = JsonConvert.SerializeObject(bigListWithMyInfo);

                MailSender.SendObject(jsonBigListObject, friend.Email, _settings, "AcceptedFriendRequest");

                var newFriendFriendForMyFriend = new MyFriendsFriends(friend, myInfo.Email);
                var jsonFriendFriendToSend = JsonConvert.SerializeObject(newFriendFriendForMyFriend);

                foreach (var user in _context.MyFriends)
                {
                    if (user.isFriend == false) { continue; }
                    MailSender.SendObject(jsonFriendFriendToSend, user.Email, _settings, "FriendGotAFriend");
                }


                _context.MyFriends.Update(friend);
                await _context.SaveChangesAsync();

                return Ok("You have accepted the Friend Request");
            }
        }
        /// <summary>
        /// Removes a friend in DB and pings the notlongerfriend to remove User from their DB .
        /// And pings current friends that the friendship ended and updates their DB aswell
        /// </summary>
        /// <param name="MyFriend"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteFriend([FromBody] MyFriend MyFriend)
        {

            var notFriend = _context.MyFriends.Where(x => x.Email == MyFriend.Email).FirstOrDefault();


            var allOurMessage = await _context.Messages.Where(x => x.To == notFriend.Email || x.From == notFriend.Email).ToListAsync();
            _context.Messages.RemoveRange(allOurMessage);
            var theirDiscussion = await _context.Discussions.Where(x => x.Email == MyFriend.Email).ToListAsync();
            _context.Discussions.RemoveRange(theirDiscussion);
            _context.MyFriends.Remove(notFriend);
            await _context.SaveChangesAsync();


            var myInfo = _context.MySettings.FirstOrDefault();
            var jsonmyInfo = JsonConvert.SerializeObject(myInfo);
            MailSender.SendObject(jsonmyInfo, notFriend.Email, _settings, "ItsNotMeItsYou");

            var jsonNotFriend = JsonConvert.SerializeObject(notFriend);
 
            foreach (var user in _context.MyFriends)
            {
                if (user.isFriend == false) { continue; }
                MailSender.SendObject(jsonNotFriend, user.Email, _settings, "FriendsFriendGotRemoved");
            }

            return Ok();
        }
        /// <summary>
        /// Lets a user change visible in friends network.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="hide"></param>
        /// <returns></returns>
        [HttpPatch("{hide}")]
        public async Task<IActionResult> Visibility([FromBody] string email, bool hide)
        {
            var friend = await _context.MyFriends.Where(x => x.Email.Equals(email)).FirstOrDefaultAsync();

            if (hide == true)
            {
                friend.hideMe = true;
                _context.MyFriends.Update(friend);
                await _context.SaveChangesAsync();

                MailSender.SendObject("true", email, _settings, "FriendHiding");
                return Ok($"You're now hiding from {friend.userName}'s network.");
            }

            else
            {
                friend.hideMe = false;
                _context.MyFriends.Update(friend);
                await _context.SaveChangesAsync();

                MailSender.SendObject("false", email, _settings, "FriendShowing");
                return Ok($"You're now visable for {friend.userName}'s network.");
            }
        }
    }
}
