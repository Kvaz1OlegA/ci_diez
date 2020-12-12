using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;
using Models;

namespace ServiceLayer
{
    public class ServiceLayer
    {
        public DataAccessLayer.DataAccessLayer accessLayer;
        public ServiceLayer()
        {
            accessLayer = new DataAccessLayer.DataAccessLayer();
        }

        public void GetPersons(List<Person> persons, string connectionString)
        {
            accessLayer.ConfigureList(persons, connectionString);
        }

        public void SetPersons(List<Person> persons, string connectionString)
        {
            accessLayer.FillTheTable(persons, connectionString);
        }
    }
}
