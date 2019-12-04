using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections;

namespace dapper
{
    abstract class DomainModel
    {
        protected MySqlConnection conn;
        protected MySqlCommand comm;
        protected MySqlDataReader reader;
        protected string tableName;
        protected int id;
        protected ArrayList res;
        protected DomainModel()
        {
            Console.WriteLine("connection...");
            string connString = "Server=localhost;Port=3306;Database=mybase;Uid=root;password='';";
            conn = new MySqlConnection(connString);
            comm = conn.CreateCommand();
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public abstract void FillObj();
        public void GetById(int id)
        {
            res = new ArrayList();
            this.id = id;
            comm.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
            int cnt = Convert.ToInt32(comm.ExecuteScalar());
            comm.CommandText = "SELECT * FROM " + tableName + " WHERE id = " + id.ToString();
            reader = comm.ExecuteReader();
            reader.Read();
            int i = 1;
            while (i < cnt)
            {
                res.Add(reader[i].ToString());
                i += 1;
            }
            reader.Close();

        }
        public int Insert(ArrayList args)
        {
            int i = 0;
            string str = "INSERT INTO " + tableName + "(";
            ArrayList columns = GetColumns();
            for (int j = 1 ; j < columns.Count - 1 ; j++)
            {
                str += columns[j] + ", ";
            }
            str += columns[columns.Count - 1] + ") VALUES (";
            while (i < args.Count - 1)
            {
                str += "'" + args[i] + "', ";
                i += 1;
            }
            str += "'" + args[args.Count - 1] + "')";
            comm.CommandText = str;
            Console.WriteLine(str);
            comm.ExecuteNonQuery();
            comm.CommandText = "SELECT LAST_INSERT_ID()";
            return Convert.ToInt32(comm.ExecuteScalar());
        }
        public void Delete(int id)
        {
            comm.CommandText = "DELETE FROM " + tableName + " WHERE id = " + id;
            comm.ExecuteNonQuery();
            if (this.id == id)
            {
                this.id = default(int);
                res = new ArrayList();
            }
        }
        public void Update(ArrayList args)
        {
            if (id == Convert.ToInt32(args[0]))
            {
                int j = 0;
                while (j < res.Count)
                {
                    res[j] = args[j + 1];
                }
            }
            ArrayList columns = GetColumns();
            int i = 1;
            string str = "UPDATE " + tableName + " SET ";
            while (i < args.Count - 1)
            {
                str += columns[i] + " = '" + args[i] + "', ";
                i += 1;
            }
            str += columns[args.Count - 1] + " = '" + args[args.Count - 1] + "'";
            comm.ExecuteNonQuery();
            str += " WHERE id = " + args[0].ToString();
            comm.CommandText = str;
            comm.ExecuteNonQuery();
            Console.WriteLine(str);
            Console.WriteLine("обновление прошло успешно!");
        }
        private ArrayList GetColumns()
        {
            comm.CommandText = "SHOW COLUMNS FROM " + tableName;
            reader = comm.ExecuteReader();
            ArrayList columns = new ArrayList();
            while (reader.Read())
            {
                columns.Add(reader[0].ToString());
            }
            reader.Close();
            return columns;
        }
    }

    class Cats : DomainModel
    {
        public Cats()
        {
            tableName = "cats";
        }
        private string age;
        private string name;
        public override void FillObj()
        {
            age = res[0].ToString();
            name = res[1].ToString();
        }
        public void GetInfo()
        {
            Console.WriteLine(name + ", " + age);
        }
    }

    class Cells : DomainModel
    {
        public Cells()
        {
            tableName = "cells";
        }
        private string state;
        private string item;
        public override void FillObj()
        {
            state = res[0].ToString();
            item = res[1].ToString();
        }
        public void GetInfo()
        {
            string str = "в ячейке под номером " + id;
            if (item != "")
            {
                Console.WriteLine(str + " находится " + item);
            }
            else
            {
                Console.WriteLine(str + "пусто");
            }
        }
        public void PutItem(string item)
        {
            if (state == "свободна")
            {
                this.item = item;
                state = "занята";
                comm.CommandText = "UPDATE " + tableName + " SET state = " + state + " WHERE id = " + id.ToString();
                comm.ExecuteNonQuery();
            }
            else
            {
                Console.WriteLine("в ячейке уже лежит другой предмет");
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {

            Cats c = new Cats();
            c.GetById(1);
            c.Update(new ArrayList() {2, 6, "Salamon"} );
            c.FillObj();
            c.GetInfo();
            c.GetById(c.Insert(new ArrayList() { 8, "Kirito"}));
            c.Delete(1);
            c.FillObj();
            c.GetInfo();
        }
    }

}