using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    public class MySettings
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string userName { get; set; }
        public string Secret { get; set; }
        public string MyPrivKey { get; set; }
        public bool DarkMode { get; set; } = false;
    }
}
