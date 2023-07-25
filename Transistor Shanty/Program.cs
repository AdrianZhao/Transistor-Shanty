using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Transistor_Shanty.Models;
using static System.Net.Mime.MediaTypeNames;
var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
var app = builder.Build();
app.MapGet("/laptops", () =>
{
    return Results.Ok(TransistorShanty.Laptops);
});
app.MapGet("/laptops/price/high-low", () =>
{
    HashSet<Laptop> resultLaptop = TransistorShanty.Laptops.OrderByDescending(l => l.Price).ToHashSet();
    return Results.Ok(resultLaptop);
});
app.MapGet("/laptops/price/low-high", () =>
{
    HashSet<Laptop> resultLaptop = TransistorShanty.Laptops.OrderBy(l => l.Price).ToHashSet();
    return Results.Ok(resultLaptop);
});
app.MapGet("/laptops/price", (double? high, double? low) =>
{
    // before = before ?? Int32.MaxValue;
    if (high == null && low == null)
    {
        return Results.BadRequest("At least one value must be provided for before and after parameters.");
    }
    if (high == null)
    {
        high = Int32.MaxValue;
    }
    if (low == null)
    {
        low = Int32.MinValue;
    }
    if (high < low)
    {
        return Results.BadRequest("Cannot requestcourses with an After date greater than a Before date.");
    }
    HashSet<Laptop> laptopsInRange = TransistorShanty.Laptops.Where(c => c.Price <= high && c.Price >= low).ToHashSet();
    return Results.Ok(laptopsInRange.OrderByDescending(l => l.Price).ToHashSet());
});
app.MapGet("/laptops/search", (double? price) =>
{
    try
    {
        if (price == null || price < 0)
        {
            return Results.BadRequest(nameof(price));
        }
        HashSet<Laptop> newLaptop = TransistorShanty.Laptops.Where(l => l.Quantity > 0 && l.Price <= price).ToHashSet();
        Laptop resultLaptop = newLaptop.OrderByDescending(l => l.Price).First();
        return Results.Ok(resultLaptop);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});
app.MapPost("/laptops/{model}", (string model) =>
{
    try
    {
        Laptop newLaptop = TransistorShanty.Laptops.First(l => l.Model.ToLower() == model);
        newLaptop.incrementViewCount();
        return Results.Ok(newLaptop);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});
app.MapPost("/laptops/add", (string model, double price, int year, int quantity, string brand, string type) =>
{
    try
    {
        string brandName = brand.Substring(0, 1).ToUpper() + brand.Substring(1).ToLower();
        string typeName = type.Substring(0, 1).ToUpper() + type.Substring(1).ToLower();
        LaptopBrand? newBrand = TransistorShanty.Brands.FirstOrDefault(b => b.BrandName == brandName);
        LaptopType? newType = TransistorShanty.Types.FirstOrDefault(t => t.Type == typeName);
        int newId = TransistorShanty.Laptops.Max(laptop => laptop.Id) + 1;
        if (newBrand == null)
        {
            return Results.BadRequest(nameof(brand));
        }
        else if (newType == null)
        {
            return Results.BadRequest(nameof(type));
        }
        else
        {
            Laptop addLaptop = new Laptop(newId, model, price, year, quantity, newBrand, newType)
            {
                Id = newId,
                Model = model,
                Price = price,
                Year = year,
                Quantity = quantity,
                LaptopBrand = newBrand,
                LaptopType = newType
            };
            TransistorShanty.Laptops.Add(addLaptop);
            return Results.Ok(addLaptop);
        }
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});
app.MapGet("/types", () =>
{
    Dictionary<string, HashSet<Laptop>> laptopsByType = new Dictionary<string, HashSet<Laptop>>();
    foreach (LaptopType type in TransistorShanty.Types)
    {
        if (!laptopsByType.ContainsKey(type.Type))
        {
            laptopsByType[type.Type] = new HashSet<Laptop>();
        }
        foreach (Laptop laptop in TransistorShanty.Laptops)
        {
            if (laptop.LaptopType.Type == type.Type)
            {
                laptopsByType[type.Type].Add(laptop);
            }
        }
    }
    return Results.Ok(laptopsByType);
    /*
    LaptopType laptopTypeNew = TransistorShanty.Types.First(t => t.Type == "New");
    HashSet<Laptop> laptopNew = new HashSet<Laptop>();
    foreach (Laptop laptop in TransistorShanty.Laptops)
    {
        if (laptop.LaptopType == laptopTypeNew)
        {
            laptopNew.Add(laptop);
        }
    }
    LaptopType laptopTypeRefurbished = TransistorShanty.Types.First(t => t.Type == "Refurbished");
    HashSet<Laptop> laptopRefurbished = new HashSet<Laptop>();
    foreach (Laptop laptop in TransistorShanty.Laptops)
    {
        if (laptop.LaptopType == laptopTypeRefurbished)
        {
            laptopRefurbished.Add(laptop);
        }
    }
    LaptopType laptopTypeRental = TransistorShanty.Types.First(t => t.Type == "Rental");
    HashSet<Laptop> laptopRental = new HashSet<Laptop>();
    foreach (Laptop laptop in TransistorShanty.Laptops)
    {
        if (laptop.LaptopType == laptopTypeRental)
        {
            laptopRental.Add(laptop);
        }
    }
    return Results.Ok(new
    {
        laptopNew, laptopRefurbished, laptopRental
    });
    */
});
app.MapGet("/brands", () =>
{
    Dictionary<string, HashSet<Laptop>> laptopsByBrand = new Dictionary<string, HashSet<Laptop>>();
    foreach (LaptopBrand brand in TransistorShanty.Brands)
    {
        if (!laptopsByBrand.ContainsKey(brand.BrandName))
        {
            laptopsByBrand[brand.BrandName] = new HashSet<Laptop>();
        }
        foreach (Laptop laptop in TransistorShanty.Laptops)
        {
            if (laptop.LaptopBrand.BrandName == brand.BrandName)
            {
                laptopsByBrand[brand.BrandName].Add(laptop);
            }
        }
    }
    return Results.Ok(laptopsByBrand);
    /*
    LaptopBrand dell = TransistorShanty.Brands.First(b => b.BrandName == "Dell");
    HashSet<Laptop> dellLaptop = new HashSet<Laptop>();
    foreach (Laptop laptop in TransistorShanty.Laptops)
    {
        if (laptop.LaptopBrand.BrandName == dell.BrandName)
        {
            dellLaptop.Add(laptop);
        }
    }
    LaptopBrand alienware = TransistorShanty.Brands.First(b => b.BrandName == "Alienware");
    HashSet<Laptop> alienwareLaptop = new HashSet<Laptop>();
    foreach (Laptop laptop in TransistorShanty.Laptops)
    {
        if (laptop.LaptopBrand.BrandName == alienware.BrandName)
        {
            alienwareLaptop.Add(laptop);
        }
    }
    LaptopBrand asusrog = TransistorShanty.Brands.First(b => b.BrandName == "Asusrog");
    HashSet<Laptop> asusrogLaptop = new HashSet<Laptop>();
    foreach (Laptop laptop in TransistorShanty.Laptops)
    {
        if (laptop.LaptopBrand.BrandName == asusrog.BrandName)
        {
            asusrogLaptop.Add(laptop);
        }
    }
    LaptopBrand lenovo = TransistorShanty.Brands.First(b => b.BrandName == "Lenovo");
    HashSet<Laptop> lenovoLaptop = new HashSet<Laptop>();
    foreach (Laptop laptop in TransistorShanty.Laptops)
    {
        if (laptop.LaptopBrand.BrandName == lenovo.BrandName)
        {
            lenovoLaptop.Add(laptop);
        }
    }
    LaptopBrand apple = TransistorShanty.Brands.First(b => b.BrandName == "Apple");
    HashSet<Laptop> appleLaptop = new HashSet<Laptop>();
    foreach (Laptop laptop in TransistorShanty.Laptops)
    {
        if (laptop.LaptopBrand.BrandName == apple.BrandName)
        {
            appleLaptop.Add(laptop);
        }
    }
    return Results.Ok(new
    {
        dellLaptop, alienwareLaptop, asusRogLaptop, lenovoLaptop, appleLaptop
    });
    */
});
app.Run();
static class TransistorShanty
{
    private static int _pkCount = 1;
    public static HashSet<Laptop> Laptops { get; set; } = new HashSet<Laptop>();
    public static HashSet<LaptopBrand> Brands { get; set; } = new HashSet<LaptopBrand>();
    public static HashSet<LaptopType> Types { get; set; } = new HashSet<LaptopType>();
    static TransistorShanty()
    {
        _seedMehtodLaptops();
    }
    public static void CreateLaptop(string model, double price, int year, int quantity, LaptopBrand brand, LaptopType type)
    {
        Laptop newLaptop = new Laptop(_pkCount++, model, price, year, quantity, brand, type);
        Laptops.Add(newLaptop);
    }
    private static void _seedMehtodLaptops()
    {
        LaptopBrand dell = new LaptopBrand(_pkCount++, "Dell");
        Brands.Add(dell);
        LaptopBrand alienware = new LaptopBrand(_pkCount++, "Alienware");
        Brands.Add(alienware);
        LaptopBrand asusrog = new LaptopBrand(_pkCount++, "Asusrog");
        Brands.Add(asusrog);
        LaptopBrand lenovo = new LaptopBrand(_pkCount++, "Lenovo");
        Brands.Add(lenovo);
        LaptopBrand apple = new LaptopBrand(_pkCount++, "Apple");
        Brands.Add(apple);
        LaptopType laptopNew = new LaptopType(_pkCount++, "New");
        Types.Add(laptopNew);
        LaptopType laptopRefurbished = new LaptopType(_pkCount++, "Refurbished");
        Types.Add(laptopRefurbished);
        LaptopType laptopRental = new LaptopType(_pkCount++, "Rental");
        Types.Add(laptopRental);
        CreateLaptop("XPS-13", 1200.00, 2020, 4, dell, laptopNew);
        CreateLaptop("XPS-17", 2200.00, 2023, 1, dell, laptopNew);
        CreateLaptop("Inspiron", 2200.00, 2023, 1, dell, laptopNew);
        CreateLaptop("x14", 1000.00, 2020, 2, alienware, laptopNew);
        CreateLaptop("x15", 800.00, 2018, 1, alienware, laptopRefurbished);
        CreateLaptop("x17", 300.00, 2019, 5, alienware, laptopRental);
        CreateLaptop("Zephyrus", 300.00, 2019, 5, asusrog, laptopRental);
        CreateLaptop("Flow", 2000.00, 2023, 1, asusrog, laptopNew);
        CreateLaptop("Strix", 1500.00, 2023, 0, asusrog, laptopNew);
        CreateLaptop("Legion", 1600.00, 2022, 0, asusrog, laptopNew);
        CreateLaptop("Legion-Pro-5i", 1300.00, 2021, 1, lenovo, laptopRental);
        CreateLaptop("Legion-Slim-5", 3000.00, 2022, 1,lenovo, laptopNew);
        CreateLaptop("Macbook-Pro-14", 4000.00, 2022, 4, apple, laptopRefurbished);
        CreateLaptop("Macbook-Pro-16", 5000.00, 2022, 1, apple, laptopRental);
        CreateLaptop("Macbook-Air-13", 6000.00, 2022, 0, apple, laptopNew);
        CreateLaptop("Macbook-Air-15", 1900.00, 2023, 2, apple, laptopRefurbished);
    }
}