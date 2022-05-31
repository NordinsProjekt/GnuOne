using GnuOne.Data;
using Library;
using Library.HelpClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GnuOne.Controllers
{
    /// <summary>
    /// Controller for handeling posts on a discussion
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly MySettings _settings;
        public PostsController(ApiContext context)
        {
            _context = context;
            _settings = _context.MySettings.First();
        }
        /// <summary>
        /// Gets all posts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var result = await _context.Posts.ToArrayAsync();
            var converted = JsonConvert.SerializeObject(result);

            return Ok(converted);
        }


        /// <summary>
        /// Gets a specific Post
        /// </summary>
        /// <returns>
        /// Post and all underlying Comments
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(int? id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }
            var commentList = await _context.Comments.Where(x => x.postID == post.ID).ToListAsync();

            var dto = new PostDTO(post, commentList);
            return Ok(dto);
        }

        /// <summary>
        ///  Creates a Post
        ///  Sets the information
        ///  Send mail to the auther of the discussion for it to spread in the network
        ///  Saves it locally
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PostPost([FromBody] Post post)
        {
            post.Date = DateTime.Now;
            DateTime foo = DateTime.Now;
            long unixID = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            post.ID = Convert.ToInt32(unixID);
            post.Email = _settings.Email;
            post.userName = _settings.UserName;

            var jsonPost = JsonConvert.SerializeObject(post);

            MailSender.SendObject(jsonPost, post.discussionEmail, _settings, "PostedPost");

            await _context.AddAsync(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPost", new { id = post.ID }, post);
        }

        /// <summary>
        /// Edit an existing post.
        /// Pings the edited information to the auther who spreads it in the network
        /// Saves it locally
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int? id, Post post)
        {
            if (id != post.ID)
            {
                return BadRequest();
            }
            if (!PostExists(id))
            {
                return NotFound();
            }

            var jsonPost = JsonConvert.SerializeObject(post);

            MailSender.SendObject(jsonPost, post.discussionEmail, _settings, "PutPost");

            _context.Update(post);
            await _context.SaveChangesAsync();

            return Accepted(post);
        }
        /// <summary>
        /// Edit a post to mark it Deleted
        /// Pings the top-level author who spreads it in the network
        /// Saves i locally
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            post.postText = "Deleted post";
            post.userName = "Deleted post";
            
            var jsonPost = JsonConvert.SerializeObject(post);

            MailSender.SendObject(jsonPost, post.discussionEmail, _settings, "PutPost");

            _context.Update(post);
            await _context.SaveChangesAsync();

            return Accepted(post);
        }
        private bool PostExists(int? id)
        {
            return _context.Posts.Any(e => e.ID == id);
        }
    }
}
