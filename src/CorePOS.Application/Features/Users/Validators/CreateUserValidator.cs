using FluentValidation;
using CorePOS.Application.Features.Users.Commands;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Users.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator(IUserRepository repo)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب")
            .MinimumLength(3).WithMessage("اسم المستخدم لا يقل عن 3 أحرف")
            .MaximumLength(100).WithMessage("اسم المستخدم لا يتجاوز 100 حرف")
            .MustAsync(async (u, ct) => !await repo.UsernameExistsAsync(u, null, ct))
            .WithMessage("اسم المستخدم مستخدم بالفعل");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(6).WithMessage("كلمة المرور لا تقل عن 6 أحرف");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("الدور الوظيفي مطلوب");
    }
}
