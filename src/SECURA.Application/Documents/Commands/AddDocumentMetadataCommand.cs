using FluentValidation;
using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;

namespace SECURA.Application.Documents.Commands;

public sealed record AddDocumentMetadataCommand(
    string RelatedEntityType,
    Guid RelatedEntityId,
    string DocumentType,
    string FileName,
    string StorageRef,
    string? Description = null) : IRequest<Guid>;

public sealed class AddDocumentMetadataCommandValidator
    : AbstractValidator<AddDocumentMetadataCommand>
{
    public AddDocumentMetadataCommandValidator()
    {
        RuleFor(x => x.RelatedEntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RelatedEntityId).NotEmpty();
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.StorageRef).NotEmpty().MaximumLength(1000);
    }
}

public sealed class AddDocumentMetadataCommandHandler
    : IRequestHandler<AddDocumentMetadataCommand, Guid>
{
    private readonly IDocumentRepository _documents;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public AddDocumentMetadataCommandHandler(
        IDocumentRepository documents,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _documents = documents;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AddDocumentMetadataCommand request,
        CancellationToken cancellationToken)
    {
        var doc = DocumentMetadata.Create(
            request.RelatedEntityType,
            request.RelatedEntityId,
            request.DocumentType,
            request.FileName,
            request.StorageRef,
            _currentUser.UserId);

        await _documents.AddAsync(doc, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return doc.Id;
    }
}
