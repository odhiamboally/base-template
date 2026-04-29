namespace BT.Application.Configuration;
public record PaginationSetting
{
    public int PageSize { get; init; } = 50;
    public int MaxPageSize { get; init; } = 100;
    public bool IsLastPage { get; init; }
    public bool IsFirstPage { get; init; }

    public int? PreviousPage { get; init; } = 0;
    public int? CurrentPage { get; init; } = 0;
    public int? NextPage { get; init; } = 0;

    public string? Cursor { get; init; } = "0";
    public string? PreviousCursor { get; init; }
    public string? NextCursor { get; init; }
    public string? LastCursor { get; init; }


}
