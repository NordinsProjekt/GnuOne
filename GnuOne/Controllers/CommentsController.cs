using GnuOne.Data;
using Library;
using Library.HelpClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Welcome_Settings;

namespace GnuOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly MySettings _settings;

        public CommentsController(ApiContext context)
        {
            _context = context;
            _settings = _context.MySettings.First();

        }

        /// <summary>
        /// Get all comments
        /// </summary>
        /// <returns>
        /// Comments in Jsonformat
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetComments()
        {
            var lista = await _context.Comments.ToListAsync();
            var converted = JsonConvert.SerializeObject(lista);
            return Ok(converted);
        }

        /// <summary>
        /// Get comment with ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Comment in Json</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComment(int? id)
        {

            var comment = await _context.Comments.Where(x => x.ID == id).ToListAsync();
            if (comment.Count == 0)
            {
                return NotFound();
            }

            var converted = JsonConvert.SerializeObject(comment);

            return Ok(converted);
        }

        /// <summary>
        /// Post a comment. Sets the information. Serialzes it.
        /// Sends mail to the the owner of the discussion to spread it to the network.
        /// Saves it locally
        /// </summary>
        /// <param name = "comment" ></ param >
        /// < returns ></ returns >
        [HttpPost]
        public async Task<IActionResult> PostComment([FromBody] Comment comment)
        {
            comment.Date = DateTime.Now;
            DateTime foo = DateTime.Now;
            long unixID = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            comment.ID = Convert.ToInt32(unixID);
            comment.Email = _settings.Email;
            comment.userName = _settings.UserName;

            var jsonComment = JsonConvert.SerializeObject(comment);

            var postDiscussionId = await _context.Posts.Where(x => x.ID == comment.postID).Select(x => x.discussionID).SingleAsync();
            var authorEmail = await _context.Discussions.Where(x => x.ID == postDiscussionId).Select(x => x.Email).SingleAsync();

            MailSender.SendObject(jsonComment, authorEmail, _settings, "PostComment");

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("PostComment", new { id = comment.ID }, comment);
        }

        // PUT: api/Comments
        /// <summary>
        /// Change an already existing comment
        /// Sending mail to the owner so he can send to the rest of the network
        /// saves it locally
        /// </summary>
        /// <param name="id"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, [FromBody] Comment comment)
        {
            if (id != comment.ID)
            {
                return BadRequest();
            }
            if (!CommentExists(id))
            {
                return NotFound();
            }

            comment.Date = DateTime.Now;

            var jsonComment = JsonConvert.SerializeObject(comment);

            var postDiscussionId = await _context.Posts.Where(x => x.ID == comment.postID).Select(x => x.discussionID).SingleAsync();
            var authorEmail = await _context.Discussions.Where(x => x.ID == postDiscussionId).Select(x => x.Email).SingleAsync();

            MailSender.SendObject(jsonComment, authorEmail, _settings, "PutComment");

            _context.Update(comment);
            await _context.SaveChangesAsync();
            return Accepted(comment);
        }

        // DELETE: api/Comments/5
        /// <summary>
        /// Remove a specific comment
        /// Send mail so it gets removed in the network
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int? id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            comment.commentText = "Deleted comment";
            comment.userName = "Deleted comment";
            
            _context.Update(comment);
            await _context.SaveChangesAsync();

            var jsonComment = JsonConvert.SerializeObject(comment);
            var postDiscussionId = await _context.Posts.Where(x => x.ID == comment.postID).Select(x => x.discussionID).SingleAsync();
            var authorEmail = await _context.Discussions.Where(x => x.ID == postDiscussionId).Select(x => x.Email).SingleAsync();

            MailSender.SendObject(jsonComment, authorEmail, _settings, "PutComment");

            return Accepted(comment);
        }
        private bool CommentExists(int? id)
        {
            return _context.Comments.Any(e => e.ID == id);
        }
    }
}
