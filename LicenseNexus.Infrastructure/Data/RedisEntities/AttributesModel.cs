namespace LicenseNexus.Infrastructure.Data.RedisEntities;

public class AttributesModel
{
    public int QuantityMin { get; set; }
    public int QuantityMax { get; set; }
    public bool IsPromo { get; set; }
    public bool IsTop { get; set; }
    public bool IsNew { get; set; }
    public string? Logo { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Author { get; set; }
}