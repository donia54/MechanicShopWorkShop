using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Domain.RepairTasks;

public sealed class RepairTask : AuditableEntity
{
    public string? Name { get; private set; }
    public decimal LaborCost { get; private set; }
    public RepairDurationInMinutes EstimatedDurationInMins { get; private set; }

    private readonly List<Part> _parts = [];
    public IReadOnlyList<Part> Parts => _parts.AsReadOnly();
    public decimal TotalCost => LaborCost + _parts.Sum(part => part.Cost * part.Quantity);

    private RepairTask()
    {

    }

    private RepairTask(Guid id, string name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins, IEnumerable<Part> parts)
        : base(id)
    {
        Name = name;
        LaborCost = laborCost;
        EstimatedDurationInMins = estimatedDurationInMins;
        _parts.AddRange(parts);
    }

    public static Result<RepairTask> Create(
        Guid id,
        string? name,
        decimal laborCost,
        RepairDurationInMinutes estimatedDurationInMins,
        IEnumerable<Part>? parts = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RepairTaskErrors.NameRequired;
        }

        if (laborCost <= 0 )
        {
            return RepairTaskErrors.LaborCostInvalid;
        }

        if (!Enum.IsDefined(estimatedDurationInMins))
        {
            return RepairTaskErrors.EstimatedDurationInvalid;
        }

        var safeParts = parts?.ToList() ?? [];

        if (safeParts.GroupBy(part => part.Id).Any(group => group.Count() > 1))
        {
            return RepairTaskErrors.DuplicatePart;
        }

        return new RepairTask(id, name.Trim(), laborCost, estimatedDurationInMins, safeParts);
    }

    public Result<Updated> Update(string? name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RepairTaskErrors.NameRequired;
        }

        if (laborCost <= 0 || laborCost > 10000)
        {
            return RepairTaskErrors.LaborCostInvalid;
        }

        if (!Enum.IsDefined(estimatedDurationInMins))
        {
            return RepairTaskErrors.EstimatedDurationInvalid;
        }

        Name = name.Trim();
        LaborCost = laborCost;
        EstimatedDurationInMins = estimatedDurationInMins;
        return Result.Updated;
    }

    public Result<Updated> AddPart(Part part)
    {
        if (_parts.Any(existingPart => existingPart.Id == part.Id))
        {
            return RepairTaskErrors.DuplicatePart;
        }

        _parts.Add(part);
        return Result.Updated;
    }

    public Result<Updated> RemovePart(Guid partId)
    {
        var existingPart = _parts.FirstOrDefault(part => part.Id == partId);

        if (existingPart is null)
        {
            return RepairTaskErrors.PartNotFound;
        }

        _parts.Remove(existingPart);
        return Result.Updated;
    }

    public Result<Updated> UpdatePart(Guid partId, string? name, decimal cost, int quantity)
    {
        var existingPart = _parts.FirstOrDefault(part => part.Id == partId);

        if (existingPart is null)
        {
            return RepairTaskErrors.PartNotFound;
        }

        var updateResult = existingPart.Update(name, cost, quantity);
        if (updateResult.IsError)
        {
            return updateResult.TopError;
        }

        return Result.Updated;
    }

    public Result<Updated> UpsertParts(IEnumerable<Part>? parts)
    {
        if (parts is null)
        {
            return RepairTaskErrors.PartsRequired;
        }

        var incomingParts = parts.ToList();
        if (incomingParts.GroupBy(part => part.Id).Any(group => group.Count() > 1))
        {
            return RepairTaskErrors.DuplicatePart;
        }

        var incomingPartIds = incomingParts.Select(part => part.Id).ToHashSet();

        _parts.RemoveAll(existingPart => !incomingPartIds.Contains(existingPart.Id));

        foreach (var incomingPart in incomingParts)
        {
            var existingPart = _parts.FirstOrDefault(part => part.Id == incomingPart.Id);

            if (existingPart is null)
            {
                _parts.Add(incomingPart);
                continue;
            }

            var updateResult = existingPart.Update(incomingPart.Name, incomingPart.Cost, incomingPart.Quantity);
            if (updateResult.IsError)
            {
                return updateResult.Errors;
            }
        }

        return Result.Updated;
    }
}
