using System.Runtime.Serialization;

namespace LicenseNexus.Domain.Enums;

public enum ChargeType
{
    [EnumMember(Value = "one_time")]
    OneTime,
    
    [EnumMember(Value = "recurring")]
    Recurring,
    
    [EnumMember(Value = "usage_based")]
    UsageBased,
    
    [EnumMember(Value = "refund")]
    Refund
}