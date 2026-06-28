using FluentValidation;
using CorePOS.Application.Features.Purchases.Commands;

namespace CorePOS.Application.Features.Purchases.Validators;

public class CreatePurchaseInvoiceValidator : AbstractValidator<CreatePurchaseInvoiceCommand>
{
    public CreatePurchaseInvoiceValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage("الفرع مطلوب");
        RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("المخزن مطلوب");
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب إضافة صنف واحد على الأقل");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("الصنف مطلوب");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر");
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0).WithMessage("التكلفة لا يمكن أن تكون سالبة");
        });
    }
}
