namespace Dsw2026Tpi.CrossCutting.Models;

public record ErrorResponse(string errorCode, string message)
{
    public ICollection<ErrorDetail> Details { get; } = [];
    public void AddDetail(string field, string issue)
    {
        Details.Add(new ErrorDetail(field, issue));
    }
    public void AddDetail(IEnumerable<(string, string)> details)
    {
        foreach (var detail in details)
        {
            AddDetail(detail.Item1, detail.Item2);
        }
    }
}

public record ErrorDetail(string field, string issue);