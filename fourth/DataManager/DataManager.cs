using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLayer;
using Models;
using ConfigurationManager;
using System.Xml.Linq;
using System.IO;

namespace DataManager
{
    public class DataManager
    {
        Manager manager;
        ServiceLayer.ServiceLayer serviceLayer;
        DBOptions dbOptions;
        string connectionString;
        public DataManager(string xml, string json)
        {
            serviceLayer = new ServiceLayer.ServiceLayer();
            manager = new Manager(json, xml);
            dbOptions = new DBOptions();
            dbOptions = manager.GetOptions<DBOptions>();
            connectionString = $"Data Source={dbOptions.DataSource};Initial Catalog={dbOptions.InitialCatalog};Integrated Security={dbOptions.IntegratedSecurity}";
        }

        public void GetData(List<Person> persons)
        { 
            serviceLayer.GetPersons(persons, connectionString);
            CreateXml(persons);
            SendXml();
        }

        public void SetData(List<Person> persons)
        {
            serviceLayer.SetPersons(persons, connectionString);
        }

        public void CreateXml(List<Person> persons)
        {
            XDocument xDocument = new XDocument();
            XElement people = new XElement("Persons");
            XElement human;
            XElement nameElem;
            XElement lastNameElem;
            XElement address;
            XElement addressLineElem;
            XElement cityElem;
            XElement emailAddress;
            XElement emailElem;
            XElement password;
            XElement hashElem;
            XElement saltElem;
            XElement personPhone;
            XElement numberElem;

            foreach (Person person in persons)
            {
                human = new XElement("Person");
                nameElem = new XElement("Name", person.Name);
                lastNameElem = new XElement("LastName", person.LastName);
                address = new XElement("Address");
                addressLineElem = new XElement("AddressLine", person.Address.AddressLine);
                cityElem = new XElement("City", person.Address.City);
                emailAddress = new XElement("EmailAddress");
                emailElem = new XElement("Email", person.EmailAddress.Email);
                password = new XElement("Password");
                hashElem = new XElement("Hash", person.Password.Hash);
                saltElem = new XElement("Salt", person.Password.Salt);
                personPhone = new XElement("PersonPhone");
                numberElem = new XElement("Number", person.PersonPhone.Number);
                address.Add(addressLineElem, cityElem);
                emailAddress.Add(emailElem);
                password.Add(hashElem, saltElem);
                personPhone.Add(numberElem);
                human.Add(nameElem, lastNameElem, address, emailAddress, password, personPhone);
                people.Add(human);
            }
            xDocument.Add(people);
            xDocument.Save("persons.xml");
        }

        public void SendXml()
        {
            File.Move(dbOptions.File, dbOptions.Directory);
        }
    }
}
