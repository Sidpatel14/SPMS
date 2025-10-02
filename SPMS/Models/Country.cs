namespace SPMS.Models
{
    public class Country
    {
        public int CountryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ISO { get; set; } = string.Empty;

        public virtual ICollection<State>? States { get; set; }
    }
}
