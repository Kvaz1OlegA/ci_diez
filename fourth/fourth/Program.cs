using System;
using DataManager;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManager;

namespace fourth
{
    class Program
    {
        static void Main(string[] args)
        {
            string dbConfig = @"E:\university\progs\C#\третий_сем\fourth\fourth\Files\dbconfig.xml";
            string dbSettings = @"E:\university\progs\C#\третий_сем\fourth\fourth\Files\dbsettings.json";
            List<Person> persons = new List<Person>();
            DataManager.DataManager dataManager = new DataManager.DataManager(dbConfig, dbSettings);
            dataManager.GetData(persons);
            dataManager.SetData(persons);
        }
    }
}
