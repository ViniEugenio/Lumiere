namespace Lumiere.Application.DTOs.Results;

public class ResultDto
{

    public List<string> Errors { get; private set; } = [];
    public object Data { get; private set; } = default;

    public List<string> GetErrors()
    {
        return Errors;
    }

    public object GetData()
    {
        return Data;
    }

    public void SetData(object data)
    {
        Data = data;
    }

    public bool IsValid()
    {
        return Errors.Count == 0;
    }

    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public void AddErrors(List<string> errors)
    {
        Errors.AddRange(errors);
    }

}
