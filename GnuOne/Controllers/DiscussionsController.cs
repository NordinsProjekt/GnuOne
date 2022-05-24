using GnuOne.Data;
using Library;
using Library.HelpClasses;
using Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Welcome_Settings;


namespace GnuOne.Controllers
{
    /// <summary>
    /// Controller for handling Discussions
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DiscussionsController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly MySettings _settings;
        public DiscussionsController(ApiContext context)
        {
            _context = context;
            _settings = _context.MySettings.First();
        }


        /// <summary>
        /// Gets all discussion
        /// </summary>
        /// <returns>Discussion with tagname-string and number of posts that is under each Discussion</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var listaDiscussion = _context.Discussions.ToList();
            var taglista = _context.tags.ToList();

            var bookmarks = _context.Bookmarks.ToList();

            var posts = _context.Posts.ToList();

            foreach (var discussion in listaDiscussion)
            {
                if(bookmarks.Where(x => x.ID == discussion.ID).Any())
                {
                    discussion.isBookmarked = true;
                }

                if (discussion.tagOne is not null)
                {
                    var tagOne = taglista.Where(x => x.ID == discussion.tagOne).Select(x => x.tagName).Single();
                    discussion.tags.Add(tagOne);
                }
                if (discussion.tagTwo is not null)
                {
                    var tagTwo = taglista.Where(y => y.ID == discussion.tagTwo).Select(x => x.tagName).Single();
                    discussion.tags.Add(tagTwo);

                }
                if (discussion.tagThree is not null)
                {
                    var tagThree = taglista.Where(z => z.ID == discussion.tagThree).Select(x => x.tagName).Single();
                    discussion.tags.Add(tagThree);
                }

                discussion.numberOfPosts = posts.Where(x => x.discussionID == discussion.ID).Count();
            }



            var converted = JsonConvert.SerializeObject(listaDiscussion);

            return Ok(converted);
        }


        /// <summary>
        /// Gets a specific Discussion
        /// </summary>
        /// <returns>
        /// The Discussion and all posts relateing to the discussion. Also the number of comments under each post.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var discussion = await _context.Discussions.FindAsync(id);

            if (discussion == null)
            {
                return NotFound();
            }
            var taglista = await _context.tags.ToListAsync();

            if (discussion.tagOne is not null)
            {
                var tagOne = taglista.Where(x => x.ID == discussion.tagOne).Select(x => x.tagName).Single();
                discussion.tags.Add(tagOne);
            }
            if (discussion.tagTwo is not null)
            {
                var tagTwo = taglista.Where(y => y.ID == discussion.tagTwo).Select(x => x.tagName).Single();
                discussion.tags.Add(tagTwo);

            }
            if (discussion.tagThree is not null)
            {
                var tagThree = taglista.Where(z => z.ID == discussion.tagThree).Select(x => x.tagName).Single();
                discussion.tags.Add(tagThree);
            }

            var commentList = await _context.Comments.ToListAsync();
            var postlist = await _context.Posts.Where(x => x.discussionID == id).ToListAsync();

            foreach (var post in postlist)
            {
                post.numberOfComments = commentList.Where(x => x.postID == post.ID).Count();
            }

            var dto = new DiscussionDTO(discussion, postlist);
            dto.numberOfPosts = postlist.Count;
            return Ok(dto);
        }

        /// <summary>
        /// Creates a discussion
        /// Sends out the discussion to the network
        /// Saves it locally
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PostDiscussion([FromBody] Discussion discussion)
        {
            discussion.Date = DateTime.Now;
            DateTime unixTime = DateTime.Now;
            long unixID = ((DateTimeOffset)unixTime).ToUnixTimeSeconds();
            discussion.ID = Convert.ToInt32(unixID);
            discussion.Email = _settings.Email;
            discussion.userName = _settings.userName;

            var jsonDiscussion = JsonConvert.SerializeObject(discussion);

            foreach (var user in _context.MyFriends)
            {
                if (user.isFriend == false) { continue; }
                MailSender.SendObject(jsonDiscussion, user.Email, _settings, "PostedDiscussion",user.pubKey);
            }

            await _context.AddAsync(discussion);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDiscussion", new { id = discussion.ID }, discussion);
        }

        /// <summary>
        /// Edits a specific Discussion
        /// tries to save it locally
        /// Pings the network about the change.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiscussion(int id, Discussion discussion)
        {
            if (id != discussion.ID)
            {
                return BadRequest();
            }
            if (!DiscussionExists(id))
            {
                return NotFound();
            }


            var jsonDiscussion = JsonConvert.SerializeObject(discussion);
            try
            {
                _context.Update(discussion);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Could not update discussion");
            }

            foreach (var user in _context.MyFriends)
            {
                if (user.isFriend == false) { continue; }
                MailSender.SendObject(jsonDiscussion, user.Email, _settings, "PutDiscussion");
            }
            return Accepted(discussion);
        }

        /// <summary>
        /// Delets a specific Discussion
        /// Pings network that the discussion has been deleted
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscussion(int id)
        {
            var discussion = await _context.Discussions.FindAsync(id);
            if (discussion == null)
            {
                return NotFound();
            }
            _context.Remove(discussion);
            await _context.SaveChangesAsync();

            var jsonDiscussion = JsonConvert.SerializeObject(discussion);

            foreach (var user in _context.MyFriends)
            {
                if (user.isFriend == false) { continue; }
                MailSender.SendObject(jsonDiscussion, user.Email, _settings, "DeleteDiscussion");
            }
            return Accepted(discussion);
        }
        private bool DiscussionExists(int? id)
        {
            return _context.Discussions.Any(e => e.ID == id);
        }
    }
}
