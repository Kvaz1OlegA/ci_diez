using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace DataAccessLayer
{
    public class DataAccessLayer
    {
        public DataAccessLayer() { }

        public void ConfigureList(List<Person> persons, string connectionString)
        {
            GetPersons(persons, connectionString);
            GetPasswords(persons, connectionString);
            GetAddresses(persons, connectionString);
            GetEmailAddresses(persons, connectionString);
            GetPersonPhones(persons, connectionString);
        }

        public void FillTheTable(List<Person> persons, string connectionString)
        {
            foreach(Person person in persons)
            {
                AddPerson(person, connectionString);
            }
        }

        void AddPerson(Person person, string connectionString)
        {
            string sqlExpression = "dbo.sp_InsertPerson";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Transaction = transaction;
                SqlParameter nameParam = new SqlParameter
                {
                    ParameterName = "@name",
                    Value = person.Name
                };
                command.Parameters.Add(nameParam);

                SqlParameter lastNameParam = new SqlParameter
                {
                    ParameterName = "@lastname",
                    Value = person.LastName
                };
                command.Parameters.Add(lastNameParam);

                SqlParameter addressLineParam = new SqlParameter
                {
                    ParameterName = "@addressline",
                    Value = person.Address.AddressLine
                };
                command.Parameters.Add(addressLineParam);

                SqlParameter cityParam = new SqlParameter
                {
                    ParameterName = "@city",
                    Value = person.Address.City
                };
                command.Parameters.Add(cityParam);

                SqlParameter emailParam = new SqlParameter
                {
                    ParameterName = "@email",
                    Value = person.EmailAddress.Email
                };
                command.Parameters.Add(emailParam);

                SqlParameter phoneParam = new SqlParameter
                {
                    ParameterName = "@phone",
                    Value = person.PersonPhone.Number
                };
                command.Parameters.Add(phoneParam);

                SqlParameter hashParam = new SqlParameter
                {
                    ParameterName = "@hash",
                    Value = person.Password.Hash
                };
                command.Parameters.Add(hashParam);

                SqlParameter saltParam = new SqlParameter
                {
                    ParameterName = "@salt",
                    Value = person.Password.Salt
                };
                command.Parameters.Add(saltParam);

                command.ExecuteNonQuery();
            }
        }

        void GetPersons(List<Person> persons, string connectionString)
        {
            string sqlExpression = "Person.sp_GetPersons";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = command.ExecuteReader();
                if(reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string lastName = reader.GetString(1);
                        persons.Add(new Person(name, lastName));
                    }
                }
            }
        }
        void GetPasswords(List<Person> persons, string connectionString)
        {
            string sqlExpression = "Person.sp_GetPasswords";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        persons[i].Password.Hash = reader.GetString(0);
                        persons[i].Password.Salt = reader.GetString(1);
                        i++;
                    }
                }
            }
        }
        void GetAddresses(List<Person> persons, string connectionString)
        {
            string sqlExpression = "Person.sp_GetAddresses";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        persons[i].Address.AddressLine = reader.GetString(0);
                        persons[i].Address.City = reader.GetString(1);
                        i++;
                    }
                }
            }
        }
        void GetEmailAddresses(List<Person> persons, string connectionString)
        {
            string sqlExpression = "Person.sp_GetEmailAddresses";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        persons[i].EmailAddress.Email = reader.GetString(0);
                        i++;
                    }
                }
            }
        }
        void GetPersonPhones(List<Person> persons, string connectionString)
        {
            string sqlExpression = "Person.sp_GetPersonPhones";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        persons[i].PersonPhone.Number = reader.GetString(0);
                        i++;
                    }
                }
            }
        }
    }
}
