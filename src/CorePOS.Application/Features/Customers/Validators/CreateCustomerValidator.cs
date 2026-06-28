using FluentValidation;
using CorePOS.Application.Features.Customers.Commands;

namespace CorePOS.Application.Features.Customers.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم العميل مطلوب")
            .MaximumLength(300).WithMessage("الاسم لا يتجاوز 300 حرف");

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("رقم الهاتف لا يتجاوز 50 رقم");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("الحد الائتماني لا يمكن أن يكون سالب");
    }
}
