namespace ImageAnnotation.Application.Commands;

/// <summary>
/// Base class for undoable commands.
/// </summary>
public abstract class Command
{
    /// <summary>
    /// Gets the command description.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Executes the command.
    /// </summary>
    public abstract void Execute();

    /// <summary>
    /// Undoes the command.
    /// </summary>
    public abstract void Undo();
}
