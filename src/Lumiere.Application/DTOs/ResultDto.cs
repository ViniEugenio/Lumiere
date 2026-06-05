namespace Lumiere.Application.DTOs;

public sealed class ResultDto<T>
{
    private readonly List<string> _errors = [];

    public IReadOnlyCollection<string> Errors => _errors.AsReadOnly();
    public T? Data { get; private set; }
    public bool IsSuccess => _errors.Count == 0;
    public bool IsFailure => _errors.Count > 0;

    public void SetData(T data) => Data = data;
    public void AddError(string error) => _errors.Add(error);
    public void AddErrors(IEnumerable<string> errors) => _errors.AddRange(errors);
}
