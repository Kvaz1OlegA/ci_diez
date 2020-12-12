using System;

namespace Models
{
    public class Password
    {
        public string Hash { get; set; }
        public string Salt { get; set; }
        public Password() { }
    }
}
