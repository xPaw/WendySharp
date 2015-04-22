using System;

namespace WendySharp
{
    public class Channel
    {
        public string Name { get; private set; }

        public Channel(string name)
        {
            Name = name;
        }
    }
}
