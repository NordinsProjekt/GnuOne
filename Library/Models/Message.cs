﻿using System.ComponentModel.DataAnnotations.Schema;

namespace GnuOne.Data.Models
{
    public class Message
    {
        public int? ID { get; set; }
        public string messageText { get; set; }
        public DateTime Sent { get; set; }
        public string From { get; set; }
        public string To{ get; set; }

        [NotMapped]
        public string FromUserName { get; set; }


        public Message()
        {

        }
    }
}
