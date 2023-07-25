namespace Transistor_Shanty.Models
{
    public class LaptopType
    {
        public int Id { get; set; }
        private string _type;
        public string Type { get { return _type; } set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _type = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            } 
        }
        public LaptopType(int id, string type) 
        {
            Id = id;
            Type = type;
        }
    }
}
