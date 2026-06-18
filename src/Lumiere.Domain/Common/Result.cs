namespace Lumiere.Domain.Common
{
    public sealed class Result<T>
    {

        public bool Succeeded => Errors.Count == 0;
        public T? Data { get; private set; }
        public List<string> Errors { get; private set; } = [];

        public void SetData(T data)
        {
            Data = data;
        }

        public void SetErrors(List<string> errors)
        {
            Errors.AddRange(errors);
        }

        public void SetError(string error)
        {
            Errors.Add(error);
        }

    }
}
