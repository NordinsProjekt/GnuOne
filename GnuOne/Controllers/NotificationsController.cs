﻿using GnuOne.Data;
using GnuOne.Data.Models;
using Library;
using Library.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GnuOne.Controllers
{

    /// <summary>
    /// Controller for handeling notifications for the User
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly MySettings _settings;
        public NotificationsController(ApiContext context)
        {
            _context = context;
            _settings = _context.MySettings.First();
        }
        /// <summary>
        /// Gathers all notifications depending on type
        /// </summary>
        /// <returns>Notifcations in json format</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notications = await _context.Notifications/*.Where(x => x.hasBeenRead == false)*/.ToListAsync();
            //ändra hasbeenread == true  vart & när?

            //Ändra i dto

            List<NotificationDTO> listOfDtos = new List<NotificationDTO>();
            
            foreach (var item in notications)
            {
                switch (item.messageType)
                {
                    case "Post":
                        Discussion b = _context.Discussions.Where(x => x.ID == item.infoID).FirstOrDefault();
                        NotificationDTO a = new NotificationDTO(item, b);
                        listOfDtos.Add(a);  
                        break;
                    case "Comment":
                        Post c = _context.Posts.Where(x => x.ID == item.infoID).FirstOrDefault();
                        NotificationDTO d = new NotificationDTO(item, c);
                        listOfDtos.Add(d);
                        break;
                    case "FriendRequestAccepted":
                        MyFriend friend = _context.MyFriends.Where(x => x.Email == item.mail).FirstOrDefault();
                        NotificationDTO friender = new NotificationDTO(item, friend, item.messageType);
                        listOfDtos.Add(friender);
                        break;
                    case "FriendRequestRecieved":
                        MyFriend friend1 = _context.MyFriends.Where(x => x.Email == item.mail).FirstOrDefault();
                        NotificationDTO friender1 = new NotificationDTO(item, friend1, item.messageType);
                        listOfDtos.Add(friender1);
                        break;
                    default:
                        break;
                }
            }


            var json = JsonConvert.SerializeObject(listOfDtos);
            return Ok(json);
        }
        /// <summary>
        /// Updates that a specific notification has been seen
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int? id)
        {
            var a = _context.Notifications.Where(x => x.ID == id).FirstOrDefault();
            a.hasBeenRead = true;
            a.counter = 0;
            _context.Notifications.Update(a);
            await _context.SaveChangesAsync();
            return Ok("Its been seen");
        }
        /// <summary>
        ///  Reset the counter for a specific notifation
        /// </summary>
        [HttpPatch]
        public async Task<IActionResult> Patch(int? id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            notification.counter = 0;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }
        /// <summary>
        /// Deletes a specific notification
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSingle(int? id)
        {
            var a  = _context.Notifications.Where(x => x.ID == id).FirstOrDefault();
            if(a is null)
            {
                return BadRequest();
            }
            else
            {
                _context.Notifications.Remove(a);
                await _context.SaveChangesAsync();
                return Ok("Notification has been deleted");
            }
          
        }
        /// <summary>
        /// Deletes all notifications
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var a = await _context.Notifications.ToListAsync();
            _context.Notifications.RemoveRange(a);
            await _context.SaveChangesAsync();
            return Ok("All notifications has been deleted");
        }
    }
}

