using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Server.Database;

namespace Server.Database
{
    public class Universe
    {
        /// <summary>
        /// creer un univers 
        /// </summary>
        /// <param name="mdp">mot de passe de l'univers</param>
        /// <param name="nom_univers">nom de l'univers</param>
        static public async Task<bool> InsertUniverse(Model.Universe universe, int owner)
        {
            bool result = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "INSERT INTO UNIVERS (NOM_UNIVERS,MDP_SERVEUR,OWNER) VALUES (@nom,@mdp,@owner);";

                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nom", universe.Name);
                    cmd.Parameters.AddWithValue("@mdp", universe.Password);
                    cmd.Parameters.AddWithValue("@owner", owner);
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


        /// <summary>
        /// retourne la liste des univers
        /// </summary>
        /// <returns>une liste a deux dimensions se composant de la façons suivant [univers,0(id_univers) 1(nom univers)]</returns>
       static public async Task<List<Model.Universe>> ReturnUniverse()
       {
            List<Model.Universe> res =  new List<Model.Universe>();

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                { 
                    string query = "select ID_UNIVERS, NOM_UNIVERS, MDP_SERVEUR from UNIVERS;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    
                    while (dataReader.Read())
                    {
                        res.Add(new Model.Universe {
                            Id = dataReader.GetInt32(0),
                            Name = dataReader.GetString(1),
                            HasPassword = !dataReader.IsDBNull(2),
                        });
                        
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
       }

        /// <summary>
        /// retourne la liste des univers
        /// </summary>
        /// <returns>une liste a deux dimensions se composant de la façons suivant [univers,0(id_univers) 1(nom univers)]</returns>
        static public async Task<Model.Universe> ReturnUniverseById(int id)
        {
            Model.Universe res = new Model.Universe();

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "select NOM_UNIVERS, MDP_SERVEUR from UNIVERS WHERE ID_UNIVERS = @id;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read())
                    {
                        res = new Model.Universe{
                            Id = id,
                            Name = dataReader.GetString(0),
                            HasPassword = !dataReader.IsDBNull(1)
                        };

                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        /// <summary>
        /// delete l'univers choisi
        /// </summary>
        /// <returns>si la requete a été correctement executé</returns>
        static public async Task<bool> DeleteUnviers(int idU)
        {
            bool res = false;

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "DELETE from UNIVERS WHERE ID_UNIVERS = @id;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idU);
                    await cmd.ExecuteNonQueryAsync();
                    res = true;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        static public async Task<List<Model.Universe>> UniversPlayer(int playerId)
        {
            List<Model.Universe> result = new List<Model.Universe>();

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "SELECT ID_UNIVERS, NOM_UNIVERS FROM UNIVERS NATURAL JOIN VILLAGE WHERE ID_JOUEUR = @id AND OWNER != @id;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", playerId);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read()) {
                        result.Add(new Model.Universe {
                            Id = dataReader.GetInt32(0),
                            Name = dataReader.GetString(1),
                        });
                    }

                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return result;
        }

        static public async Task<Model.Universe> UniversOwned(int playerId)
        {
            Model.Universe result = new Model.Universe { };

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "select ID_UNIVERS, NOM_UNIVERS from UNIVERS WHERE OWNER = @id;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", playerId);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read()) {
                        result.Id = dataReader.GetInt32(0);
                        result.Name = dataReader.GetString(1);
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
        /// renvoie l'id du village du joueur renvoie null si aucun 
        /// </summary>
        /// <param name="idJ"></param>
        /// <param name="idU"></param>
        /// <returns></returns>
        static public async Task<int?> PlayerHaveVillageInUnivers(int idJ, int idU)
        {
            int? res = null;

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "select ID_VILLAGE from VILLAGE WHERE ID_JOUEUR = @idJ and ID_UNIVERS = @idU;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idJ", idJ);
                    cmd.Parameters.AddWithValue("@idU", idU);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        res = dataReader.GetInt32(0);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }



        /// <summary>
        /// Return the number of village in a universe
        /// </summary>
        /// <param name="idU">ID of the universe</param>
        /// <returns>Number of village in the universe</returns>
        static public async Task<int?> GetVillageCountInUniverse(int idU)
        {
            int? res = null;

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "SELECT COUNT(ID_UNIVERS) FROM VILLAGE WHERE ID_UNIVERS = @idU;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idU", idU);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        res = dataReader.GetInt32(0);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }




        /// <summary>
        /// renvoie le nom du village du joueur 
        /// </summary>
        /// <param name="idJ"></param>
        /// <param name="idV"></param>
        /// <returns></returns>
        static public async Task<string?> PlayerVillageName(int idJ, int idV)
        {
            string? res = null;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "select NOM_VILLAGE from VILLAGE WHERE ID_JOUEUR = @idJ and ID_VILLAGE = @idV;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idJ", idJ);
                    cmd.Parameters.AddWithValue("@idV", idV);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        res = dataReader.GetString(0);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        static public async Task<string?> MajoritaryFaction(int idU)
        {
            string? res = null;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "select MAX(FACTION) from VILLAGE WHERE ID_UNIVERS = @idU;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idU", idU);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    if (dataReader.Read())
                    {
                        if (!dataReader.IsDBNull(0))
                        {
                            res = dataReader.GetString(0);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        /// <summary>
        /// Verify if an user can access to a universe
        /// </summary>
        /// <param name="idU">id of the universe</param>
        /// <param name="mdp">password entered</param>
        /// <returns>bool that tells if the player can have access to it</returns>
        static public async Task<bool> VerifyAccess(int idU, string mdp)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = " SELECT NOM_UNIVERS FROM UNIVERS WHERE ID_UNIVERS = @idU AND MDP_SERVEUR = @mdp;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idU", idU);
                    cmd.Parameters.AddWithValue("@mdp", mdp);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    if (dataReader.Read())
                    {
                        if (!dataReader.IsDBNull(0))
                        {
                            if (!string.IsNullOrEmpty(dataReader.GetString(0)))
                            {
                                res = true;
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }
    }
}

    