using FluentValidation;
using CorePOS.Application.Features.Products.Commands;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Products.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator(IProductRepository repo)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود الصنف مطلوب")
            .MaximumLength(50).WithMessage("كود الصنف لا يتجاوز 50 حرف")
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, null, ct))
            .WithMessage("كود الصنف مستخدم بالفعل");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("اسم الصنف بالعربي مطلوب")
            .MaximumLength(300).WithMessage("اسم الصنف لا يتجاوز 300 حرف");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("القسم مطلوب");

        RuleFor(x => x.BaseUnitId)
            .GreaterThan(0).WithMessage("الوحدة الأساسية مطلوبة");

        RuleFor(x => x.SaleUnitId)
            .GreaterThan(0).WithMessage("وحدة البيع مطلوبة");

        RuleFor(x => x.PurchaseUnitId)
            .GreaterThan(0).WithMessage("وحدة الشراء مطلوبة");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر الشراء لا يمكن أن يكون سالب");

        RuleFor(x => x.SalePrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر البيع لا يمكن أن يكون سالب");

        RuleFor(x => x.TaxPercent)
            .InclusiveBetween(0, 100).WithMessage("نسبة الضريبة يجب أن تكون بين 0 و 100");

        When(x => !string.IsNullOrWhiteSpace(x.Barcode), () =>
            RuleFor(x => x.Barcode!)
                .MustAsync(async (barcode, ct) => !await repo.BarcodeExistsAsync(barcode, null, ct))
                .WithMessage("الباركود مستخدم بالفعل"));
    }
}
