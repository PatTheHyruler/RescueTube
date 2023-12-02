namespace Utils.Pagination.Contracts;

public class SortOptions
{
    public SortOptions()
    {
    }

    public SortOptions(string name, SortOptions previous) :
        this(name, name == previous.Name ? previous.Descending : null)
    {
    }

    public SortOptions(string name, bool? existingDescending)
    {
        Name = name;
        if (existingDescending != null)
        {
            Descending = !existingDescending;
        }
    }

    public string? Name { get; set; }
    public bool? Descending { get; set; }
}