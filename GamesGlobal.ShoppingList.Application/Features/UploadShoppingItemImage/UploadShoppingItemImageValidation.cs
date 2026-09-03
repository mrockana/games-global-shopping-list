using System;
using System.Collections.Generic;
using FluentValidation;
using GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;

namespace GamesGlobal.ShoppingList.Application.Features.UploadShoppingItemImage;

public sealed class UploadShoppingItemImageValidation : AbstractValidator<UploadShoppingItemImageCommandRequest>
{
    private static readonly char[] PathSeparators = ['/', '\\'];

    public UploadShoppingItemImageValidation(FileObjectStoreOptions fileObjectStoreOptions)
    {
        HashSet<string> allowedContentTypes = new(fileObjectStoreOptions.AllowedContentTypes, StringComparer.OrdinalIgnoreCase);

        RuleFor(r => r.UserCode)
            .NotEmpty().WithMessage($"{nameof(UploadShoppingItemImageCommandRequest.UserCode)} is required");

        RuleFor(r => r.ShoppingItemId)
            .GreaterThan(0).WithMessage($"{nameof(UploadShoppingItemImageCommandRequest.ShoppingItemId)} must be greater than zero");

        RuleFor(r => r.Content)
            .NotNull().WithMessage($"{nameof(UploadShoppingItemImageCommandRequest.Content)} is required");

        RuleFor(r => r.Length)
            .GreaterThan(0).WithMessage("The uploaded file is empty")
            .LessThanOrEqualTo(fileObjectStoreOptions.MaxImageSizeInBytes)
            .WithMessage($"The uploaded file exceeds the maximum allowed size of {fileObjectStoreOptions.MaxImageSizeInBytes.ToString()} bytes");

        RuleFor(r => r.ContentType)
            .NotNull().NotEmpty().WithMessage($"{nameof(UploadShoppingItemImageCommandRequest.ContentType)} is required")
            .Must(allowedContentTypes.Contains)
            .WithMessage($"Only the following content types are allowed: {string.Join(", ", fileObjectStoreOptions.AllowedContentTypes)}");

        RuleFor(r => r.FileName)
            .NotNull().NotEmpty().WithMessage($"{nameof(UploadShoppingItemImageCommandRequest.FileName)} is required")
            .MaximumLength(fileObjectStoreOptions.MaxObjectNameLength)
            .Must(fileName => !string.IsNullOrWhiteSpace(fileName) && fileName.IndexOfAny(PathSeparators) < 0 && !fileName.Contains("..", StringComparison.Ordinal))
            .WithMessage($"{nameof(UploadShoppingItemImageCommandRequest.FileName)} must not contain path segments");
    }
}
