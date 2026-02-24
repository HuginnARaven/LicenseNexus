using System.Runtime.Serialization;

namespace LicenseNexus.Domain.Enums;

public enum ChargeType
{
    OneTime,
    Recurring,
    UsageBased,
    Refund
}