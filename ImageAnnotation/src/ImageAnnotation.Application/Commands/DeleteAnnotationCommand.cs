using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Application.Commands;

/// <summary>
/// Command for deleting an annotation.
/// </summary>
public class DeleteAnnotationCommand : Command
{
    private readonly ImageAsset _imageAsset;
    private readonly Annotation _annotation;
    private int _index;

    public override string Description => $"Delete {_annotation.GetType().Name}";

    public DeleteAnnotationCommand(ImageAsset imageAsset, Annotation annotation)
    {
        _imageAsset = imageAsset;
        _annotation = annotation;
    }

    public override void Execute()
    {
        _index = _imageAsset.Annotations.IndexOf(_annotation);
        _imageAsset.Annotations.Remove(_annotation);
    }

    public override void Undo()
    {
        if (_index >= 0 && _index <= _imageAsset.Annotations.Count)
        {
            _imageAsset.Annotations.Insert(_index, _annotation);
        }
        else
        {
            _imageAsset.Annotations.Add(_annotation);
        }
    }
}
