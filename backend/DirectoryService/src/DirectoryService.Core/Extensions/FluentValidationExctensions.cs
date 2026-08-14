using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel;
using FluentValidation;
using FluentValidation.Results;

namespace DirectoryService.Core.Extensions;

public static class FluentValidationExctensions
{
    public static IRuleBuilderOptionsConditions<T, TValue> MustBeValueObject<T, TValue, TVo>(
        this IRuleBuilder<T, TValue> ruleBuilder,
        Func<TValue, Result<TVo, Error>> factory)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            var result = factory(value);

            if (result.IsFailure)
                context.AddFailure(new ValidationFailure(context.PropertyPath, result.Error.Message)
                {
                    ErrorCode = result.Error.Code,
                    CustomState = result.Error
                });
        });
    }

    public static IRuleBuilderOptionsConditions<T, TValue> MustBeValueObject<T, TValue, TVo>(
        this IRuleBuilder<T, TValue> ruleBuilder,
        Func<TValue, Result<TVo, Failure>> factory)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            var result = factory(value);

            if (result.IsFailure)
                foreach (var error in result.Error)
                {
                    context.AddFailure(new ValidationFailure(context.PropertyPath, error.Message)
                        { ErrorCode = error.Code, CustomState = error});
                }
        });
    }

    public static IRuleBuilderOptionsConditions<T, TValue> MustSatisfy<T, TValue>(
        this IRuleBuilder<T, TValue> ruleBuilder,
        Func<TValue, Error?> factory)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            var result = factory(value);

            if (result is not null)
                context.AddFailure(new ValidationFailure(context.PropertyPath, result.Message)
                {
                    ErrorCode = result.Code,
                    CustomState = result
                });

        });
    }
}