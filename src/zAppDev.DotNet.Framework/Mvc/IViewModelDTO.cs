namespace zAppDev.DotNet.Framework.Mvc
{
    public interface IViewModelDTO<T>
    {     
        T Convert();
		object _key { get; }
        object _clientKey { get; set; }
    }
}