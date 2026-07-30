using System.Text.Json;

namespace PlanningAPI.Models;

public sealed class VoucherWriteRequest
{
    public Dictionary<string, JsonElement> Header { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<VoucherLineWriteRequest> Lines { get; set; } = [];
}

public sealed class VoucherCreateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int VoucherKey { get; set; }
}

public enum VoucherUpdateStatus
{
    Updated,
    NotFound,
    Posted
}

public sealed class VoucherLineWriteRequest
{
    public Dictionary<string, JsonElement> Line { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<Dictionary<string, JsonElement>> Accounts { get; set; } = [];
    public List<Dictionary<string, JsonElement>> LabVendors { get; set; } = [];
}

public sealed class VoucherAggregateDto
{
    public Dictionary<string, object?> Header { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<VoucherLineAggregateDto> Lines { get; set; } = [];
}

public sealed class VoucherLineAggregateDto
{
    public Dictionary<string, object?> Line { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<Dictionary<string, object?>> Accounts { get; set; } = [];
    public List<Dictionary<string, object?>> LabVendors { get; set; } = [];
}
