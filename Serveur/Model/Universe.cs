namespace Server.Model
{
    public class Universe
    {
        public int? Id { get; set; }
        public String Name { get; set; } = "";
        public String? Password { get; set; }
        public bool? HasPassword { get; set; }
        public int? Town { get; set; }
    }
}
