namespace CanonicaLib.DataAnnotations
{
    public interface IExample<T> where T : class
    {
        public string Name { get; }
        public T Example { get; }
    }
}
