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
        protected Dictionary<string, string> res;
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
            res = new Dictionary<string, string>();
            this.id = id;
            comm.CommandText = "SELECT item, state FROM cells WHERE id = " + id.ToString();
            reader = comm.ExecuteReader();
            reader.Read();
            res.Add("state", reader["state"].ToString());
            res.Add("item", reader["item"].ToString());
            reader.Close();
            
        }
        public int Insert(string state, string item)
        {
            comm.CommandText = "INSERT INTO " + tableName  + " (state, item) " + "VALUES ('" + state + "', '" + item + "')";
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
                res = new Dictionary<string, string>();
            }
            else if (this.id > id) this.id -= 1;
        }
        public void Update(int id, string state, string item)
        {
            if (this.id == id)
            {
                res = new Dictionary<string, string>();
                res.Add("state", state);
                res.Add("item", item);
            }
            comm.CommandText = "UPDATE " + tableName + " SET state = '" + state + "', item = '" + item + "' WHERE id = " + id.ToString();
            comm.ExecuteNonQuery();
            Console.WriteLine("обновление прошло успешно!");
        }
    }

    class Cell : DomainModel
    {
        public Cell()
        {
            tableName = "cells";
        }
        private string state;
        private string item;
        public override void FillObj()
        {
            state = res["state"];
            item = res["item"];
        }
        public void GetInfo()
        {
            string str = "в ячейке под номером " + id;
            if(item != "")
            {
                Console.WriteLine(str + " находится " + item);
            }
            else
            {
                Console.WriteLine(str + "пусто");
            }
            
        }
        public void Put(string item)
        {
            if(state == "свободна")
            {
                this.item = item;
                state = "занята";
                Update(id, state, item);
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
            
            Cell c = new Cell();
            c.GetById(13);
            c.FillObj();
            c.GetInfo();
        }
    }

}