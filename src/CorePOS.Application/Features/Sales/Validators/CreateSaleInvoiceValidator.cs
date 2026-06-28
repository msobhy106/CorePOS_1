using FluentValidation;
using CorePOS.Application.Features.Sales.Commands;

namespace CorePOS.Application.Features.Sales.Validators;

public class CreateSaleInvoiceValidator : AbstractValidator<CreateSaleInvoiceCommand>
{
    public CreateSaleInvoiceValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("الفرع مطلوب");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).WithMessage("المخزن مطلوب");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("المستخدم مطلوب");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("يجب إضافة صنف واحد على الأقل");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("الصنف مطلوب");
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر");
            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("السعر لا يمكن أن يكون سالب");
        });

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("نسبة الخصم يجب أن تكون بين 0 و 100");

        RuleFor(x => x.TaxPercent)
            .InclusiveBetween(0, 100).WithMessage("نسبة الضريبة يجب أن تكون بين 0 و 100");

        RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0).WithMessage("المبلغ المدفوع لا يمكن أن يكون سالب");
    }
}
