namespace Lumiere.Application.Interfaces;

public interface IResultDto
{
    bool Succeeded { get; }
    void AddErrors(IEnumerable<string> errors);
}
