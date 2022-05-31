
using GnuOne.Data;
using Library;
using Library.HelpClasses;
using Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Welcome_Settings;

namespace GnuOne.Controllers
{
    /// <summary>
    /// Controller for User's Profile
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class myProfileController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly MySettings _settings;

        public myProfileController(ApiContext context)
        {
            _context = context;
            _settings = _context.MySettings.First();
        }

        /// <summary>
        /// Get profile information
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //Sidan verkar komma hit hela tiden.
            //Kontrollerar mailen varje gång sidan laddas om.
            string _connectionstring = Global.ConnectionString;
            //Seg, lägg detta till en knapp istället.

            //using (MariaContext context = new MariaContext(_connectionstring))
            //{
            //    MailReader.ReadUnOpenEmails(context, _connectionstring);
            //}
            var a = await _context.MyProfile.ToListAsync();
            var json = JsonConvert.SerializeObject(a);
            return Ok(json);
            


    }
        /// <summary>
        /// Gets a specific picture's information
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPicture(int? id)
        {
            var a = await _context.Standardpictures.Where(x => x.pictureID == id).Select(x => x.PictureSrc).FirstAsync();
            var json = JsonConvert.SerializeObject(a);
            return Ok(json);
            //return Ok();
        }

        /// <summary>
        /// Update users profile information and pings friends that information has changed
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> PutMyProfile([FromBody] myProfile Profile)
        {
            var myprofile = await _context.MyProfile.FirstOrDefaultAsync();

            myprofile.myUserInfo = Profile.myUserInfo;
            myprofile.pictureID = Profile.pictureID;
            myprofile.tagOne = Profile.tagOne;
            myprofile.tagTwo = Profile.tagTwo;
            myprofile.tagThree = Profile.tagThree;


            var myInfo = new MyFriend
            {
                Email = _settings.Email,
                userName = _settings.UserName,
                userInfo = Profile.myUserInfo,
                pictureID = Profile.pictureID,
                tagOne = Profile.tagOne,
                tagTwo = Profile.tagTwo,
                tagThree = Profile.tagThree
            };
            _context.MyProfile.Update(myprofile);
            await _context.SaveChangesAsync();
            var jsonProfileInfo = JsonConvert.SerializeObject(myInfo);

            //Skickar uppdateringen till ens vänner.
            foreach (var user in _context.MyFriends)
            {
                if (user.isFriend == false) { continue; }
                MailSender.SendObject(jsonProfileInfo, user.Email, _settings, "PutFriendsProfile");
                //_context.MyProfile.Update(myprofile);
               // await _context.SaveChangesAsync();

            }
                return Ok("Updated profile");
        }
    }
}
