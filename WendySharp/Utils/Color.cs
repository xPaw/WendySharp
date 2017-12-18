using System;

namespace WendySharp
{
    static class Color
    {
        private const char IrcColor = '\x3';
        public const char NORMAL = '\xf';
        public static readonly string DARKBLUE = IrcColor + "02";
        //public static readonly string DARKGREEN = IrcColor + "03";
        public static readonly string RED = IrcColor + "04";
        //public static readonly string BROWN = IrcColor + "05";
        //public static readonly string PURPLE = IrcColor + "06";
        public static readonly string OLIVE = IrcColor + "07";
        //public static readonly string YELLOW = IrcColor + "08";
        public static readonly string GREEN = IrcColor + "09";
        //public static readonly string TEAL = IrcColor + "10";
        //public static readonly string CYAN = IrcColor + "11";
        public static readonly string BLUE = IrcColor + "12";
        //public static readonly string MAGENTA = IrcColor + "13";
        public static readonly string DARKGRAY = IrcColor + "14";
        public static readonly string LIGHTGRAY = IrcColor + "15";
    }
}
