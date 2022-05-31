using GnuOne.Data;
using Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Library.HelpClasses;


namespace GnuOne.Controllers
{
    /// <summary>
    /// Controller for handling User settings
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly MySettings _settings;

        public SettingsController(ApiContext context)
        {
            _context = context;
            _settings = _context.MySettings.First();
        }
        /// <summary>
        /// Gets the users settings information
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var settings = await _context.MySettings.FirstOrDefaultAsync();

            if (settings != null)
            {
                var jsonsettings = JsonConvert.SerializeObject(settings);
                return Ok(settings);
            }
            return NotFound();
        }
        /// <summary>
        /// Gets only the Darkmode setting
        /// </summary>
        [HttpGet("{darkmode}")]
        public async Task<IActionResult> Getmode()
        {
            var darkModeJson = JsonConvert.SerializeObject(_settings.DarkMode);

            return Ok(darkModeJson);
        }

        /// <summary>
        /// Changes only the darkmode setting
        /// </summary>
        [HttpPut("{darkmode}")]
        public async Task<IActionResult> Put(bool darkMode)
        {
            var settings = await _context.MySettings.FirstOrDefaultAsync();
            _context.MySettings.Update(settings);
            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Update any setting for the User
        /// </summary>
        /// <param name="mySettings"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> updateSettingsPut([FromBody] MySettings mySettings)
        {
            _context.MySettings.Update(mySettings);
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Updates the Username and pings the network of the change
        /// </summary>
        [HttpPut("username")]
        public async Task<IActionResult> updateSettingsPut([FromBody] string username)
        {
            var oldUsername = _settings.UserName;
            _settings.UserName = username;
            _context.MySettings.Update(_settings);
            await _context.SaveChangesAsync();

            var jsonUsername = JsonConvert.SerializeObject(username);
            await BigList.UpdateUsername(_context, username, oldUsername, _settings.Email);

            var emailList = new List<string>();
            foreach (var friend in _context.MyFriends)
            {
                MailSender.SendObject(jsonUsername, friend.Email, _settings, "UpdatedUsername");
                emailList.Add(friend.Email);
            }

            var myFriendsFriendsList = _context.MyFriendsFriends.Where(x => x.userName != username).ToList();
            foreach (var friendsFriend in myFriendsFriendsList)
            {
                if (!emailList.Contains(friendsFriend.Email))
                {
                    MailSender.SendObject(jsonUsername, friendsFriend.Email, _settings, "UpdatedFriendsFriendsUsername");
                }
            }
            return Ok();
        }

        /// <summary>
        /// Not complete function. Only the start of a restoring database information if user were to switch computers
        /// </summary>
        [HttpPatch("{GetBackUp}")]
        public IActionResult BackUpDB(bool GetBackUp)
        {
            if (GetBackUp == true)
            {
                Backup.GetBackUp();
                return Ok("Your information is successfully restored.");
            }
            return BadRequest("Something didn work out...");
        }
    }
}