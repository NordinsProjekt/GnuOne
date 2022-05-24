using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GnuOne.Data
{
    //Klassen som skall hantera hela menysystemet.
    //Snyggare meny.
    class Meny
    {
        public static void DefaultWindow()
        {
            Console.Clear();
            Meny.DefaultConsoleSettings();
            Meny.Draw(File.ReadAllLines("ascii\\gnu2.txt"), 2, 2, ConsoleColor.DarkCyan);
        }

        public static void DefaultWindow2(string align)
        {
            Console.WindowWidth = 100;
            Console.WindowHeight = 30;
            switch (align.ToLower())
            {
                case "center":
                    Meny.DefaultConsoleSettings();
                    Meny.DrawCenter(File.ReadAllLines("ascii\\gnuMini.txt"), 2, ConsoleColor.Green);
                    break;
                default:
                    Meny.DefaultConsoleSettings();
                    Meny.Draw(File.ReadAllLines("ascii\\gnuMini.txt"), 2, 2, ConsoleColor.Green);
                    break;
            }
        }

        public static string[] FirstTimeUserMenu(string titel)
        {
            string[] text =
            {
                titel.ToString(),
                "Write your Email address:  ",
                "Write your Email password: ",
                "\n",
                "Choose your username: "
            };
            return text;
        }
        public static string[] EnterCredMenu(string titel)
        {
            string[] text =
{
                titel.ToString(),
                "Hello",
                "Welcome to the Gnu-network",
                "Please enter your heidi-username and password.",
                "\n",
                "Username: ",
                "Password: "
            };
            return text;
        }

        public static string[] EnterMailInfo(string titel)
        {
            string[] text =
            {
                titel.ToString(),
                "Hello",
                "Welcome to the Gnu-network",
                "Please enter your mail and password",
                "\n"
            };
            return text;
        }

        public static string[] EnterUsername(string titel)
        {
            string[] text =
            {
                titel.ToString(),
                "Hello",
                "Welcome to the Gnu-network",
                "Please choose your username",
                "\n"
            };
            return text;
        }

        /// <summary>
        /// Ritar upp all text som skickas till den.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="textColor"></param>
        public static void Draw(string[] text, int x = 5, int y = 2, ConsoleColor textColor = ConsoleColor.White)
        {
            //Ritar upp all text som skickas till den.
            //Enligt en specifik standard.
            Console.ForegroundColor = textColor;
            for (int i = 0; i < text.Length; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(text[i]);
            }
        }
        public static void DrawCenter(string[] text, int y = 2, ConsoleColor textColor = ConsoleColor.White)
        {
            //Ritar upp all text som skickas till den.
            //Enligt en specifik standard.
            Console.ForegroundColor = textColor;
            int x = (Console.WindowWidth / 2) - (LongestString(text) / 2);
            for (int i = 0; i < text.Length; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(text[i]);
            }

        }
        public static int LongestString(string[] text, int skipRowIndex = 0)
        {
            int max = 0;
            for (int i = skipRowIndex; i < text.Length; i++)
            {
                if (text[i].Length > max)
                {
                    max = text[i].Length;
                }
            }
            return max;
        }
        public static void DefaultConsoleSettings()
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = true;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }

}
