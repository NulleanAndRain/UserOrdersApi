namespace Nullean.UserOrdersApi.Entities
{
    public class Response
    {
        public ICollection<Error> Errors { get; set; }
    }

    public class Response<T> : Response
    {
        public T? ResponseBody { get; set; }
    }
}
