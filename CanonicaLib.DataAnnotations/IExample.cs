namespace Zen.CanonicaLib.DataAnnotations
{
    public interface IExample<out T> where T : class
    {
        public string Name { get; }
        public T Example { get; }
    }
}
