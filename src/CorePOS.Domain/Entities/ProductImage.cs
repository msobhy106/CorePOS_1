using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class ProductImage : BaseEntity
{
    public int    ProductId { get; private set; }
    public string ImagePath { get; private set; } = string.Empty;
    public bool   IsMain    { get; private set; }
    public int    SortOrder { get; private set; }

    public Product? Product { get; private set; }

    protected ProductImage() { }

    public static ProductImage Create(int productId, string imagePath, bool isMain = false, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(imagePath)) throw new ArgumentException("Image path is required.");
        return new ProductImage { ProductId = productId, ImagePath = imagePath.Trim(),
                                  IsMain = isMain, SortOrder = sortOrder };
    }

    public void SetAsMain()    => IsMain = true;
    public void UnsetAsMain()  => IsMain = false;
}
