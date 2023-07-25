namespace Transistor_Shanty.Models
{
    public class Laptop
    {
        // primary key
        public int Id { get; set; }
        private string _model;
        public string Model { get { return _model; } set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _model = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }
        private double _price;
        public double Price { get { return _price; } set 
            { 
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                else
                {
                    _price = value;
                }
            } 
        }
        private int _year;
        public int Year { get { return _year; } set
            {
                if ((value < 2014) || (value > 2024))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                else
                {
                    _year = value;
                }
            }
        }
        private int _quantity;
        public int Quantity { get { return _quantity; } set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                else
                {
                    _quantity = value;
                }
            }
        }
        public LaptopBrand LaptopBrand { get; set; }
        public LaptopType LaptopType { get; set; }
        private int _viewTimes = 0;
        public int ViewTimes { get { return _viewTimes; } }
        public void incrementViewCount()
        {
            _viewTimes++;
        }
        // relational property
        public HashSet<LaptopBrand> brands = new HashSet<LaptopBrand>();
        public HashSet<LaptopType> types = new HashSet<LaptopType>();
        public Laptop(int id, string model, double price, int year, int quantity, LaptopBrand laptopBrand, LaptopType laptopType)
        {
            Id = id; 
            Model = model; 
            Price = price; 
            Year = year; 
            Quantity = quantity;
            LaptopBrand = laptopBrand;
            LaptopType = laptopType;
        }
    }
}
