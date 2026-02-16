namespace LicenseNexus.Infrastructure.Data.RedisEntities;

public class ClassificationModel
{
    public string TypeName { get; set; } = string.Empty;
    public string UnitMeasureName { get; set; } = string.Empty;
        
    public VendorModel Vendor { get; set; } = new();
    public GroupModel Group { get; set; } = new();
}

public class VendorModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

public class GroupModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}