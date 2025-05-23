using System;

namespace TCad.Util;

public interface IServiceProvider<T>
{
    T Get();

    void Override(Func<T> func);
}

public class ServiceProvider<T> : IServiceProvider<T>
{
    Func<T> _creator;


    public ServiceProvider(Func<T> creator)
    {
        _creator = creator;
    }

    public T Get()
    {
        return _creator();
    }

    public void Override(Func<T> creator)
    {
        _creator = creator;
    }
}

public class SingleServiceProvider<T> : IServiceProvider<T> where T : class
{
    private T _value;

    public SingleServiceProvider(Func<T> creator)
    {
        Console.WriteLine($"SingleServiceProvider<{typeof(T)}> constructor");
        _value = creator();
    }

    public T Get()
    {
        return _value;
    }

    public void Override(Func<T> creator)
    {
        _value = creator();
    }
}

public class LateSingleServiceProvider<T> : IServiceProvider<T> where T : class
{
    Func<T> _creator;

    T _value;

    public LateSingleServiceProvider(Func<T> creator)
    {
        _creator = creator;
    }

    public T Get()
    {
        if (_value == null)
        {
            _value = _creator();
        }

        return _value;
    }

    public void Override(Func<T> creator)
    {
        _creator = creator;
    }
}
