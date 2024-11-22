namespace SolveIti.Models
{
    public class SystemModel
    {
        internal long id { get; set; }
        public string Name { get; set; }
        public string idName { get; set; }
        public List<ApplicationsOfTheSystemModel> ApplicationsOfTheSystem { get; set; }        
    }
}