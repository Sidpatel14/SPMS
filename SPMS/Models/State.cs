namespace SPMS.Models
{
    public class State
    {
        public int StateID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CountryISO { get; set; } = string.Empty;

        public virtual Country? Country { get; set; }
    }
}
