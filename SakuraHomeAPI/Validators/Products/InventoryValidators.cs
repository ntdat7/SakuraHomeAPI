using FluentValidation;
using SakuraHomeAPI.DTOs.Products.Requests;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Validators.Products
{
    /// <summary>
    /// Validator for update stock requests
    /// </summary>
    public class UpdateStockRequestValidator : AbstractValidator<UpdateStockRequestDto>
    {
        public UpdateStockRequestValidator()
        {
            RuleFor(x => x.NewStock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("New stock must be greater than or equal to 0");

            RuleFor(x => x.Action)
                .IsInEnum()
                .WithMessage("Inventory action must be a valid value");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Reason cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Reason));

            RuleFor(x => x.BatchNumber)
                .MaximumLength(50)
                .WithMessage("Batch number cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.BatchNumber));

            RuleFor(x => x.Location)
                .MaximumLength(100)
                .WithMessage("Location cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Location));

            RuleFor(x => x.UnitCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Unit cost must be greater than or equal to 0")
                .When(x => x.UnitCost.HasValue);

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Unit price must be greater than or equal to 0")
                .When(x => x.UnitPrice.HasValue);

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Expiry date must be in the future")
                .When(x => x.ExpiryDate.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(200)
                .WithMessage("Notes cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            // Business logic validations
            RuleFor(x => x)
                .Must(HaveReasonForNegativeAdjustment)
                .WithMessage("Reason is required for stock reduction actions")
                .When(x => IsStockReductionAction(x.Action));
        }

        private bool HaveReasonForNegativeAdjustment(UpdateStockRequestDto request)
        {
            return !string.IsNullOrWhiteSpace(request.Reason);
        }

        private bool IsStockReductionAction(InventoryAction action)
        {
            var reductionActions = new[]
            {
                InventoryAction.Sale,
                InventoryAction.Damage,
                InventoryAction.Lost,
                InventoryAction.Expired,
                InventoryAction.Sample,
                InventoryAction.Adjustment
            };

            return reductionActions.Contains(action);
        }
    }
}