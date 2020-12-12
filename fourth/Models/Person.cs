using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Person
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public Address Address { get; set; }
        public EmailAddress EmailAddress { get; set; }
        public Password Password{ get; set; }
        public PersonPhone PersonPhone { get; set; }

        public Person(string name, string lastName)
        {
            Name = name;
            LastName = lastName;
            Address = new Address();
            EmailAddress = new EmailAddress();
            Password = new Password();
            PersonPhone = new PersonPhone();
        }
    }
}
