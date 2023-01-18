using System;
using System.Data;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization;
using Google.Protobuf.WellKnownTypes;
using MySql.Data;
using MySql.Data.MySqlClient;
using Server.Database;
using Server.Model;

namespace Server.Database
{
    public class VillageDB
    {


        /// <summary>
        /// Create the village of the player in a universe 
        /// </summary>
        /// <param name="v">Village</param>
        /// <param name="idJ">ID of the player</param>
        /// <returns></returns>
        static public async Task<bool> InsertVillage(Model.Village v,  int idJ)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "INSERT INTO VILLAGE (FACTION,BOIS,PIERRE,ORS,ID_JOUEUR,ID_UNIVERS,NOM_VILLAGE) VALUES (@faction,250,250,250,@idJ,@idU,@nomV);";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@faction", v.Faction);
                    cmd.Parameters.AddWithValue("@idJ", idJ);
                    cmd.Parameters.AddWithValue("@idU", v.IdUnivers);
                    cmd.Parameters.AddWithValue("@nomV", v.Name);
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
        /// renvoie les ressources disponible sous formes de tableau 
        /// </summary>
        /// <param name="idV"></param>
        /// <returns></returns>
        static public async Task<Model.Ressources> GetRessource(int idV)
        {
            Model.Ressources res = new Model.Ressources();

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
             
                string query = "SELECT BOIS,PIERRE,ORS FROM VILLAGE WHERE ID_VILLAGE = @idV;";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idV", idV);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        res.Bois = dataReader.GetInt32(0);
                        res.Pierre = dataReader.GetInt32(1);
                        res.Or = dataReader.GetInt32(2);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        static public async Task<bool> UpdateRessources(int idV,int Bois,int Pierre, int Or)
        {
            bool res = false;


            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "UPDATE VILLAGE SET BOIS = @bois, PIERRE = @pierre, ORS = @ors WHERE ID_VILLAGE = @idVillage";

                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@bois", Bois);
                    cmd.Parameters.AddWithValue("@pierre", Pierre);
                    cmd.Parameters.AddWithValue("@ors", Or);
                    cmd.Parameters.AddWithValue("@idVillage", idV);
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
        /// initialise tout les batiment d'un village
        /// </summary>
        /// <param name="idV"></param>
        /// <returns></returns>
        static public async Task<bool> InitBatiment(int idV)
        {
            bool res = false;
            string[] bat = { "TAVERN", "GUNSMITH", "WAREHOUSE", "TRAINING_CAMP", "HEALER_HUT" };

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "INSERT INTO BATIMENT (TYPE_BATIMENT,NIVEAU,ID_VILLAGE,EN_CONSTRUCTION,START_TIME) VALUES (@TB,1,@idV,FALSE,@date);";
                try
                {
                    for (int i = 0; i < 5; i++)
                    {
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@idV", idV);
                        cmd.Parameters.AddWithValue("@TB", bat[i]);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now);
                        await cmd.ExecuteNonQueryAsync();
                        res = true;
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
        /// augemente le niveau d'un batiment de plus 1 et retire les ressource spécifié
        /// </summary>
        
        /// <returns></returns>
        static public async Task<bool> UpBatiment(int idV, string building)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                int[] temp = new int[4];
                await conn.OpenAsync();
                try
                {
                    string query3 = "UPDATE BATIMENT SET " +
                                    "NIVEAU = NIVEAU + 1 " +
                                    "WHERE ID_VILLAGE = @idV AND TYPE_BATIMENT = @building;";
                    MySqlCommand cmd = new MySqlCommand(query3, conn);
                            cmd.Parameters.AddWithValue("@idV", idV);
                            cmd.Parameters.AddWithValue("@building", building);
                            await cmd.ExecuteNonQueryAsync();
                            res = true;

                }
                catch(MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        /// <summary>
        /// renvoie le niveau de batiment d'un village
        /// </summary>
        /// <param name="idV"></param>
        /// <returns></returns>
        static public async Task<Dictionary<string, int>> GetLevelBatiment(int idV)
        {
            Dictionary<string, int> res = new Dictionary<string, int>();

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "SELECT NIVEAU,TYPE_BATIMENT FROM BATIMENT WHERE ID_VILLAGE = @idV;";

                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idV", idV);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        res.Add(dataReader.GetString(1), dataReader.GetInt32(0));
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
        /// retourne plusieurs liste contenant le personnage, l'heure d'arriver et le slot des personnage contenu dans un batiment
        /// </summary>
        /// <param name="idB"></param>
        /// <returns></returns>
        static public async Task<List<(Server.Model.Perso, int, int)>> GetPersoInBatiment(int idB)
        {
            List<(Server.Model.Perso, int, int)> res = new List<(Server.Model.Perso, int, int)>();

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "SELECT ID_PERSO,ENTREE,SLOT FROM BATIMENT WHERE ID_BATIMENT = @idB;";
                string query2 = "SELECT ID_PERSONNAGE,LEVEL,PV,PV_MAX,NOM,PM_MAX,PA_MAX,ID_VILLAGE,IMG,RACE,CLASSE,ID_EQUIPEMENT FROM PERSONNAGE WHERE ID_PERSONNAGE = @idP;";
                Server.Model.Perso perso = new Server.Model.Perso();
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlCommand cmd2 = new MySqlCommand(query2, conn); ;
                    cmd.Parameters.AddWithValue("@idB", idB);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        cmd2.Parameters.AddWithValue("@idP", dataReader.GetInt32(0));
                        MySqlDataReader dataReader2 = cmd2.ExecuteReader();
                        if(dataReader2.Read())
                        {
                            perso.Id = dataReader2.GetInt32(0);
                            perso.Level = dataReader2.GetInt32(1);
                            perso.PV = dataReader2.GetInt32(2);
                            perso.PV_MAX = dataReader2.GetInt32(3);
                            perso.Name = dataReader2.GetString(4);
                            perso.PM_MAX = dataReader2.GetInt32(5);
                            perso.PA_MAX = dataReader2.GetInt32(6);
                            perso.ID_VILLAGE = dataReader2.GetInt32(7);
                            perso.IMG = dataReader2.GetInt32(8);
                            perso.RACE = dataReader2.GetString(9);
                            perso.CLASSE = dataReader2.GetString(10);
                            perso.ID_EQUIPEMENT = dataReader2.GetInt32(11);
                        }
                        TimeSpan sec = (DateTime.Now - dataReader.GetDateTime(1));
                        res.Add((perso, (int)sec.TotalSeconds, dataReader.GetInt32(2)));
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
        /// retourne une liste contenant les personnages qui sont dans l'inventaire
        /// </summary>
        /// <param name="idB"></param>
        /// <returns></returns>
        static public async Task<List<Server.Model.Perso>> GetPersoInInventaire(int idB)
        {
            List<Server.Model.Perso> res = new List<Server.Model.Perso>();

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "select ID_PERSONNAGE from PERSONNAGE WHERE ID_PERSONNAGE NOT IN (SELECT ID_PERSONNAGE FROM STOCK_PERSONNAGE);";
                string query2 = "SELECT ID_PERSONNAGE,LEVEL,PV,PV_MAX,NOM,PM_MAX,PA_MAX,ID_VILLAGE,IMG,RACE,CLASSE,ID_EQUIPEMENT FROM PERSONNAGE WHERE ID_PERSONNAGE = @idP;";
                Server.Model.Perso perso = new Server.Model.Perso();
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idB", idB);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    
                    int[] idTemp = new int[dataReader.FieldCount];
                    int i = 0;
                    
                    while (dataReader.Read())
                    {
                        idTemp[i] = (dataReader.GetInt32(0));
                    }

                    
                    foreach(int id in idTemp)
                    {
                        MySqlCommand cmd2 = new MySqlCommand(query2, conn); ;
                        cmd2.Parameters.AddWithValue("@idP", id);
                        MySqlDataReader dataReader2 = cmd.ExecuteReader();

                        perso.Id = dataReader2.GetInt32(0);
                        perso.Level = dataReader2.GetInt32(1);
                        perso.PV = dataReader2.GetInt32(2);
                        perso.PV_MAX = dataReader2.GetInt32(3);
                        perso.Name = dataReader2.GetString(4);
                        perso.PM_MAX = dataReader2.GetInt32(5);
                        perso.PA_MAX = dataReader2.GetInt32(6);
                        perso.ID_VILLAGE = dataReader2.GetInt32(7);
                        perso.IMG = dataReader2.GetInt32(8);
                        perso.RACE = dataReader2.GetString(9);
                        perso.CLASSE = dataReader2.GetString(10);
                        perso.ID_EQUIPEMENT = dataReader2.GetInt32(11);

                        res.Add(perso);
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
        /// retourne une liste de tout les perso présent dans le village
        /// </summary>
        /// <param name="idV"></param>
        /// <returns></returns>
        static public async Task<List<Server.Model.Perso>> GetPersoInVillage(int idV)
        {
            List<Server.Model.Perso> res = new List<Server.Model.Perso>();

            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "SELECT ID_PERSONNAGE,LEVEL,PV,PV_MAX,NOM,PM_MAX,PA_MAX,ID_VILLAGE,IMG,RACE,CLASSE,ID_EQUIPEMENT FROM PERSONNAGE WHERE ID_VILLAGE = @idV";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idV", idV);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Perso perso = new Perso();
                        perso.Id = dataReader.GetInt32(0);
                        perso.Level = dataReader.GetInt32(1);
                        perso.PV = dataReader.GetInt32(2);
                        perso.PV_MAX = dataReader.GetInt32(3);
                        perso.Name = dataReader.GetString(4);
                        perso.PM_MAX = dataReader.GetInt32(5);
                        perso.PA_MAX = dataReader.GetInt32(6);
                        perso.ID_VILLAGE = dataReader.GetInt32(7);
                        perso.IMG = dataReader.GetInt32(8);
                        perso.RACE = dataReader.GetString(9);
                        perso.CLASSE = dataReader.GetString(10);
                        perso.ID_EQUIPEMENT = dataReader.GetInt32(11);
                        res.Add(perso);
                    }
                }
                catch(MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        

        /// <summary>
        /// ajoute un perso dans un batiement au slot spécifié, si le personnage est déjà dedans ne fait rien 
        /// </summary>
        /// <param name="idP"></param>
        /// <param name="idB"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        static public async Task<bool> InsertPersoInBatiment(int idP, int idB, int slot)
        {
            bool result = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                try
                {
                    string queryVerif = "SELECT ID_BATIMENT FROM STOCK_PERSONNAGE WHERE ID_PERSONNAGE = @idP;";
                    MySqlCommand cmd = new MySqlCommand(queryVerif, conn);
                    cmd.Parameters.AddWithValue("@idP", idP);
                    
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    
                    if (dataReader.Read() && dataReader.GetInt32(0) != idB)
                    {
                        string queryDelete = "DELETE FROM STOCK_PERSONNAGE WHERE ID_PERSONNAGE = @idP;";
                        cmd = new MySqlCommand(queryDelete, conn);
                        await cmd.ExecuteNonQueryAsync();
                        string query = "INSERT INTO STOCK_PERSONNAGE (ID_PERSONNAGE,ID_BATIMENT,ENTREE,SLOT) VALUES (@idP,@idB,@hEntree,@slot);";
                        cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@idP", idP);
                        cmd.Parameters.AddWithValue("@mdp",idB);
                        cmd.Parameters.AddWithValue("@hEntree", DateTime.Now);
                        cmd.Parameters.AddWithValue("@slot", slot);
                        await cmd.ExecuteNonQueryAsync();
                        result = true;
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
        /// augemente le niveau de l'arme d'un personnage de plus 1 et retire les ressource spécifié
        /// </summary>
        /// <param name="idB"></param>
        /// <param name="coutB"></param>
        /// <param name="coutP"></param>
        /// <param name="coutO"></param>
        /// <returns></returns>
        static public async Task<bool> UpArmePerso(int idEquipement)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "UPDATE EQUIPEMENT SET" +
                                 "NIVEAU_ARME = NIVEAU_ARME + 1"+ 
                                 "WHERE ID_EQUIPEMENT = @idE";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idE", idEquipement);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        /// <summary>
        /// augemente le niveau de l'armure d'un personnage de plus 1 et retire les ressource spécifié
        /// </summary>
        /// <param name="idB"></param>
        /// <param name="coutB"></param>
        /// <param name="coutP"></param>
        /// <param name="coutO"></param>
        /// <returns></returns>
        static public async Task<bool> UpArmurePerso(int idEquipement)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "UPDATE EQUIPEMENT SET" +
                                 "NIVEAU_ARMURE = NIVEAU_ARMURE + 1" +
                                 "WHERE ID_EQUIPEMENT = @idE";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idE", idEquipement);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        /// <summary>
        /// remet la vie d'un personnage au maximum
        /// </summary>
        /// <param name="idP"></param>
        /// <returns></returns>
        static public async Task<bool> HealPerso(int idP)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "UPDATE PERSONNAGE SET "+
                                "PV = (SELECT PV_MAX FROM PERSONNAGE WHERE ID_PERSONNAGE = @idP) "+
                                "WHERE ID_PERSONNAGE = @idP;";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idP", idP);
                    cmd.Parameters.AddWithValue("@idP",idP);
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
        /// intialise un personnage aléatoire
        /// </summary>
        /// <param name="idP"></param>
        /// <returns>l'id du PERSONNAGE</returns>
        static public async Task<int> InitRandomPerso(int idV)
        {
            string[] classe = {"ARCHER","GUERRIER","SORCIER","TANK"};
            string[] race = { "ELFE", "NAIN", "HUMAIN", "HOBBIT" };
            int idPR = -1;
            Random random = new Random();
            int raceI = random.Next(3);
            int classeI = random.Next(3);
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();

                string query = "INSERT INTO PERSONNAGE (LEVEL,PV,PV_MAX,NOM,CLASSE,RACE,PA_MAX,PM_MAX,ID_EQUIPEMENT,ID_VILLAGE,IMG) VALUES (1,250,250,@name,@classe,@race,6,@PM,@idE,@idV,@img);";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", Utils.Util.GenerationNom());
                    cmd.Parameters.AddWithValue("@classe",classe[classeI]);
                    cmd.Parameters.AddWithValue("@race",race[raceI]);
                    int PM = 3;
                    if(race[raceI] == "NAIN")
                    {
                        PM = 2;
                    }
                    cmd.Parameters.AddWithValue("@PM", PM);
                    cmd.Parameters.AddWithValue("@idE",await InitEquipement());
                    cmd.Parameters.AddWithValue("@idV",idV);
                    cmd.Parameters.AddWithValue("@img", raceI + classeI);
                    await cmd.ExecuteNonQueryAsync();
                    string query2 = "SELECT MAX(ID_PERSONNAGE) FROM PERSONNAGE;";
                    MySqlCommand cmd2 = new MySqlCommand(query2, conn);
                    MySqlDataReader dataReader = cmd2.ExecuteReader();
                    while (dataReader.Read())
                    {
                        idPR = dataReader.GetInt32(0);
                    }

                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return idPR;
        }


        static public async Task<int?> InitEquipement()
        {
            int? idE = null;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                string query = "INSERT INTO EQUIPEMENT (NIVEAU_ARME,NIVEAU_ARMURE,IMG_ARME,IMG_ARMURE,BONUS_ARME,BONUS_ARMURE) VALUES (1,1,1,1,10,10);";
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    await cmd.ExecuteNonQueryAsync();
                    string query2 = "SELECT MAX(ID_EQUIPEMENT) FROM EQUIPEMENT";
                    MySqlCommand cmd2 = new MySqlCommand(query2, conn);
                    MySqlDataReader dataReader = cmd2.ExecuteReader();
                    while (dataReader.Read()) 
                    { 
                        idE = dataReader.GetInt32(0);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return idE;
        }

        static public async Task<Model.Equipement> GetEquipement(int idPersonnage)
        {
            Model.Equipement equipement = new Model.Equipement();
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "SELECT ID_EQUIPEMENT,BONUS_ARMURE, BONUS_ARME, IMG_ARMURE, IMG_ARME, NIVEAU_ARME, NIVEAU_ARMURE FROM EQUIPEMENT WHERE ID_EQUIPEMENT = (SELECT ID_EQUIPEMENT FROM PERSONNAGE WHERE ID_PERSONNAGE = @idP)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idP", idPersonnage);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        equipement.Id = dataReader.GetInt32(0);
                        equipement.BonusArmor = dataReader.GetInt32(1);
                        equipement.BonusWeapon = dataReader.GetInt32(2);
                        equipement.ImgArmor = dataReader.GetInt32(3);
                        equipement.ImgWeapon = dataReader.GetInt32(4);
                        equipement.LevelWeapon = dataReader.GetInt32(5);
                        equipement.LevelArmor = dataReader.GetInt32(6);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return equipement;
        }

        static public async Task<Model.Perso> GetPersoById(int idP)
        {
            Model.Perso res = new Model.Perso();
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "SELECT ID_PERSONNAGE,LEVEL,PV,PV_MAX,NOM,CLASSE,RACE,PA_MAX,PM_MAX,ID_EQUIPEMENT,ID_VILLAGE,IMG FROM PERSONNAGE WHERE ID_PERSONNAGE = @idP";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idP", idP);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        res.Id = dataReader.GetInt32(0);
                        res.Level = dataReader.GetInt32(1);
                        res.PV = dataReader.GetInt32(2);
                        res.PV_MAX = dataReader.GetInt32(3);
                        res.Name = dataReader.GetString(4);
                        res.CLASSE = dataReader.GetString(5);
                        res.RACE = dataReader.GetString(6);
                        res.PA_MAX = dataReader.GetInt32(7);
                        res.PM_MAX = dataReader.GetInt32(8);
                        res.ID_EQUIPEMENT = dataReader.GetInt32(9);
                        res.ID_VILLAGE = dataReader.GetInt32(10);
                        res.IMG = dataReader.GetInt32(11);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }



        static public async Task<bool> GetBuildingInConstruction(int idV, string building)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "SELECT EN_CONSTRUCTION FROM BATIMENT WHERE ID_VILLAGE = @idV AND TYPE_BATIMENT = @building";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idV", idV);
                    cmd.Parameters.AddWithValue("@building", building);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        res = dataReader.GetBoolean(0);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }


        static public async Task<bool> SetBuildingInConstruction(int idV, string building)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "UPDATE BATIMENT SET EN_CONSTRUCTION = TRUE, START_CONSTRUCTION = @date WHERE ID_VILLAGE = @idV AND TYPE_BATIMENT = @building";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@idV", idV);
                    cmd.Parameters.AddWithValue("@building", building);
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

        static public async Task<int> GetBuildingInConstructionTime(int idV, string building)
        {
            int res = 0;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "SELECT START_CONSTRUCTION FROM BATIMENT WHERE ID_VILLAGE = @idV AND TYPE_BATIMENT = @building";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idV", idV);
                    cmd.Parameters.AddWithValue("@building", building);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        res = (int)Math.Round(DateTime.Now.TimeOfDay.TotalSeconds - dataReader.GetMySqlDateTime(0).GetDateTime().TimeOfDay.TotalSeconds);
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
        /// update l'attribut Start_TIme de batiment et le set à l'heure actuel 
        /// </summary>
        /// <param name="idP"></param>
        /// <returns></returns>

        static public async Task<bool> SetStartTimeTaverne(int idV)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "UPDATE BATIMENT SET START_TIME = @date WHERE ID_VILLAGE = @idV and TYPE_BATIMENT = 'TAVERN'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@idV", idV);
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
        /// retourne le nombre de seconde depuis la dernière update du start time de la taverne 
        /// </summary>
        /// <param name="idV"></param>
        /// <returns></returns>
        static public async Task<int> GetStartTimeTavern(int idV)
        {
            int res = -1;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "SELECT START_TIME FROM BATIMENT  WHERE ID_VILLAGE = @idV and TYPE_BATIMENT = 'TAVERN'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idV", idV);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        res = (int)Math.Round(DateTime.Now.TimeOfDay.TotalSeconds - dataReader.GetMySqlDateTime(0).GetDateTime().TimeOfDay.TotalSeconds);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return res;
        }

        static public async Task<bool> DeleteAllCaracterFromTaverne(int idVillage)
        {
            bool res = false;
            using (MySqlConnection conn = DatabaseConnection.NewConnection())
            {
                await conn.OpenAsync();
                try
                {
                    string query = "DELETE FROM STOCK_PERSONNAGE WHERE ID_VILLAGE = @idVillage and ID_BATIMENT = (select ID_BATIMENT FROM BATIMENT where TYPE_BATIMENT = TAVERN);";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idV", idVillage);
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


    }

}
