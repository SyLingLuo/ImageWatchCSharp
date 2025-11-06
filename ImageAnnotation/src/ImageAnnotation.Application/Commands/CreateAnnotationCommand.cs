using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Application.Commands;

/// <summary>
/// Command for creating a new annotation.
/// </summary>
public class CreateAnnotationCommand : Command
{
    private readonly ImageAsset _imageAsset;
    private readonly Annotation _annotation;

    public override string Description => $"Create {_annotation.GetType().Name}";

    public CreateAnnotationCommand(ImageAsset imageAsset, Annotation annotation)
    {
        _imageAsset = imageAsset;
        _annotation = annotation;
    }

    public override void Execute()
    {
        _imageAsset.Annotations.Add(_annotation);
    }

    public override void Undo()
    {
        _imageAsset.Annotations.Remove(_annotation);
    }
}
