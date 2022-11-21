using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Server.Database
{
	static public class Account
	{
        static public async Task<bool> UserExists(string username)
        {
            bool result = true;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                
                string query = "select ID_JOUEUR from JOUEUR where NOM_COMPTE = @compte ;";
                
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@compte", username);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    result = await dataReader.ReadAsync();
                    Console.WriteLine(result);
                    if (result)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return result;
        }

		static public async Task<bool> UserExistsEmail(string emailAddress)
        {
            bool result = true;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                
                string query = "SELECT ID_JOUEUR FROM JOUEUR WHERE ADRESS_MAIL = @mail;";

                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@mail", emailAddress);

                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    result = await dataReader.ReadAsync();

                    if (result)
                    {
                        result = true;
                    } 
                    else
                    {
                        result = false;
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return result;
        }

        /// <summary>
        /// créer un utilisateur temporaire et renvoie le mot de passe temporaire
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        static public async Task<byte[]> CreateTemp(string email, string username)
        {

            string sel = Utils.Utils.RandomPassword(32);
            byte[] password = Utils.Utils.BtoH(Utils.Utils.RandomPassword(10),sel);
            
            
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
               
                string query = "INSERT INTO JOUEUR (ADRESS_MAIL,NOM_COMPTE,MDP,VALIDE,DATE_CREATION,SEL) VALUES (@adresse_mail,@nom_compte,@password,false,@date_creation,@sel);";
            
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@adresse_mail",email );
                    cmd.Parameters.AddWithValue("@nom_compte", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@date_creation", DateTime.Now);
                    cmd.Parameters.AddWithValue("@sel", sel);

                    await cmd.ExecuteNonQueryAsync();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return password;
        }

        /// <summary>
        /// check for the connexion return the id of the player 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        static public async Task<int?> CheckUsernamePassword(string username, string password)
        {
            int? result = null;
            bool exist;


            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "select SEL from JOUEUR where NOM_COMPTE = @nomcompte ;";
                string query2 = "select ID_JOUEUR from JOUEUR where NOM_COMPTE = @nomcompte and MDP = @mdp ;";
         
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nomcompte", username);
                    MySqlDataReader dataReader1 = cmd.ExecuteReader();
                    string sel = dataReader1.GetString(0);
                    cmd = new MySqlCommand(query2, conn);
                    cmd.Parameters.AddWithValue("@nomcompte", username);
                    cmd.Parameters.AddWithValue("@mdp", Utils.Utils.BtoH(password,sel));
                    result = dataReader1.GetInt32(0);
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return result;
        }

        static public async Task<bool> CreateSession(int userId, string sessionToken)
        {
            bool executed = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "INSERT INTO SESSION (ID_JOUEUR,TOKEN) VALUES(@id_j,@token);";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id_j", userId);
                    cmd.Parameters.AddWithValue("@token", sessionToken);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    executed = true;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return executed;
        }

        static public async Task<bool> DeleteJoueurSession(int id_compte)
        {
            bool execute = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "DELETE FROM SESSION WHERE ID_JOUEUR = @id_j ;";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id_j", id_compte);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    execute = true;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return execute;
        }

        static public async Task<int?> CheckTokenSession(string session)
        {
            int? result = null;
            bool exist;

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "select ID_JOUEUR from SESSION where TOKEN = @session;";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@token", session);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    exist = dataReader.Read();
                    if (exist)
                    {
                        result = dataReader.GetInt32(0);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return result;
        }
    }
}
