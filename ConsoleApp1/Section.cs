namespace ConsoleApp1
{
    public class Section
    {
        public string StartKey { get; set; }
        public string EndKey { get; set; }
        public List<Area> Areas { get; set; } = new List<Area>();
    }
}
