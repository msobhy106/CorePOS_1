namespace CorePOS.Application.Features.Categories.DTOs;

public class CategoryDto
{
    public int     Id        { get; set; }
    public string  Code      { get; set; } = string.Empty;
    public string  NameAr    { get; set; } = string.Empty;
    public string? NameEn    { get; set; }
    public int?    ParentId  { get; set; }
    public string? ParentName{ get; set; }
    public int     Level     { get; set; }
    public int     SortOrder { get; set; }
    public bool    IsActive  { get; set; }
    public List<CategoryDto> Children { get; set; } = [];
}
