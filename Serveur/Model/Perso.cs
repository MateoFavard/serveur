namespace Server.Model
{
    public class Perso
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public int PV { get; set; }
        public bool Possede { get; set; }
        public int PV_MAX { get; set; }
        public string Name { get; set; } = "";
        public int PA_MAX { get; set; }
        public int PM_MAX { get; set;}
        public int IMG { get; set; }
        public int ID_VILLAGE { get; set; }
        public string CLASSE { get; set; } = "";
        public string RACE { get; set; } = "";
        public int ID_EQUIPEMENT { get; set; }
    }
}
