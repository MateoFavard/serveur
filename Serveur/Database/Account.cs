using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Server.Utils;

namespace Server.Database
{
	 public class Account
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
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return result;
        }

		static public async Task<int?> UserExistsEmail(string emailAddress)
        {
            int? result = null;

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                
                string query = "SELECT ID_JOUEUR FROM JOUEUR WHERE ADRESS_MAIL = @mail;";

                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@mail", emailAddress);

                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
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

        /// <summary>
        /// créer un utilisateur temporaire et renvoie le mot de passe temporaire
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        static public async Task<bool> CreateTemp(string email, string username, string password)
        {
            string sel = Util.RandomPassword(32);
            
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
               
                string query = "INSERT INTO JOUEUR (ADRESS_MAIL,NOM_COMPTE,MDP,VALIDE,DATE_CREATION,SEL) VALUES (@adresse_mail,@nom_compte,@password,false,@date_creation,@sel);";
            
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@adresse_mail",email );
                    cmd.Parameters.AddWithValue("@nom_compte", username);
                    cmd.Parameters.AddWithValue("@password", Util.BtoH(password, sel));
                    cmd.Parameters.AddWithValue("@date_creation", DateTime.Now);
                    cmd.Parameters.AddWithValue("@sel", sel);

                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return false;
        }

        /// <summary>
        /// check for the connexion return the id of the player 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        static public async Task<(int?, bool)> CheckUsernamePassword(string username, string password)
        {
            int? result = null;
            bool validate = false;

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "select SEL from JOUEUR where NOM_COMPTE = @nomcompte ;";
                string query2 = "select ID_JOUEUR , VALIDE from JOUEUR where NOM_COMPTE = @nomcompte and MDP = @mdp ;";
         
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nomcompte", username);
                    string sel;
                    using (MySqlDataReader dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        sel = dataReader.GetString(0);
                    }
                    cmd = new MySqlCommand(query2, conn);
                    cmd.Parameters.AddWithValue("@nomcompte", username);
                    cmd.Parameters.AddWithValue("@mdp", Util.BtoH(password,sel));
                    using (MySqlDataReader dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            result = dataReader.GetInt32(0);
                            validate = dataReader.GetBoolean(1);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (result,validate);
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

                string query = "select ID_JOUEUR from SESSION where TOKEN = @token;";
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

        static public async Task<bool> ValidateUser(int id)
        {
            bool result = false;

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "UPDATE JOUEUR SET VALIDE = TRUE WHERE ID_JOUEUR = @id; ";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    result = true;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return result;
        }

        static public async Task<Model.Account?> UserInfo(int id)
        {
            Model.Account? result = null;

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "SELECT NOM_COMPTE, ADRESS_MAIL, VALIDE FROM JOUEUR WHERE ID_JOUEUR = @id; ";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader reader = cmd.ExecuteReader()) 
                    {
                        if (reader.Read())
                        {
                            result = new Model.Account(
                                reader.GetString(0),
                                reader.GetString(1),
                                reader.GetBoolean(2)
                            );
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return result;
        }

        static public async Task<bool> UpdateMDP(string mdp, int id)
        {
            bool result = false;
            string salt = Util.RandomPassword(32);

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "UPDATE JOUEUR SET MDP = @mdp, SEL = @salt WHERE ID_JOUEUR = @id;";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@mdp", Util.BtoH(mdp, salt));
                    cmd.Parameters.AddWithValue("@salt", salt);
                    cmd.Parameters.AddWithValue("@id", id);
                    await cmd.ExecuteNonQueryAsync();
                    result = true;
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
