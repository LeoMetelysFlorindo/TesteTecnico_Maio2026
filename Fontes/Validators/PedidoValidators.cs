
using FluentValidation;
using PedioApi.DTOs;

namespace PedioApi.Validators;

public class CreateProductValidator : AbstractValidator<CreatePedidoDto>
{
    public CreateProductValidator()
    {
        // Name is required and limited to 200 characters
        RuleFor(x => x.ClienteNome)
            .NotEmpty().WithMessage("Nome do Cliente is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");  
    }
}

public class UpdateProductValidator : AbstractValidator<UpdatePedidoDto>
{
    public UpdateProductValidator()
    {
        // Conditional validation: only if value is provided
        RuleFor(x => x.ClienteNome)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.ClienteNome));      
        
    }
}

