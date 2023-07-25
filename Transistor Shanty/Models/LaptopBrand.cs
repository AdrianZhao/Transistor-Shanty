namespace Transistor_Shanty.Models
{
    public class LaptopBrand
    {
        public int Id { get; set; }
        private string _brandName;
        public string BrandName { get { return _brandName; } set 
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _brandName = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            } 
        }
        HashSet<LaptopType> laptopTypes = new HashSet<LaptopType>();
        public LaptopBrand(int id, string brandName) 
        {
            Id = id;
            BrandName = brandName;
        }
    }
}
