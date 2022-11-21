using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Server.Database;

namespace Serveur.Database
{
    public static class Universe
    {
        /// <summary>
        /// creer un univers 
        /// </summary>
        /// <param name="mdp">mot de passe de l'univers</param>
        /// <param name="nom_univers">nom de l'univers</param>
        static public async Task<bool> InsertUnivers(string nom_univers, string? mdp, int owner)
        {
            bool result = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "INSERT INTO UNIVERS (NOM_UNIVERS,MDP_SERVEUR,OWNER) VALUES (@nom,@mdp,@owner);";

                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nom", nom_univers);
                    cmd.Parameters.AddWithValue("@mdp", mdp);
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
       static public async Task<String[,]> ReturnUniverse()
        {
            
            int nbUnivers = 0;
            String[,] res = new string[(int)nbUnivers, 2];
            bool exist;

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "select COUNT(ID_UNIVERS) from UNIVERS;";

                try
                { 
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    exist = dataReader.Read();

                    if (exist)
                    {
                        nbUnivers = dataReader.GetInt32(0);

                        query = "select ID_UNIVERS from UNIVERS;";
                        MySqlCommand cmd2 = new MySqlCommand(query, conn);
                        dataReader = cmd2.ExecuteReader();
                        for (int i = 0; i < nbUnivers; i++)
                        {
                            res[i,0] = dataReader.GetString(i);
                        }

                        query = "select NOM_UNIVERS from UNIVERS;";
                        MySqlCommand cmd3 = new MySqlCommand(query, conn);
                        dataReader = cmd3.ExecuteReader();
                        for (int i = 0; i < nbUnivers; i++)
                        {
                            res[i, 1] = dataReader.GetString(i);
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
