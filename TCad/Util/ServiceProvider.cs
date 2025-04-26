using System;

public interface IServiceProvider<T>
{
    T Instance { get; }

    void Override(Func<T> func);
}

public class ServiceProvider<T> : IServiceProvider<T>
{
    Func<T> _creator;

    public T Instance
    {
        get => _creator();
    }

    public ServiceProvider(Func<T> creator)
    {
        _creator = creator;
    }

    public void Override(Func<T> creator)
    {
        _creator = creator;
    }
}

public class SingleServiceProvider<T> : IServiceProvider<T> where T : class
{
    public T Instance
    {
        get;
        private set;
    }

    public SingleServiceProvider(Func<T> creator)
    {
        Console.WriteLine($"SingleServiceProvider<{typeof(T)}> constructor");

        Instance = creator();
    }

    public void Override(Func<T> creator)
    {
        Instance = creator();
    }
}

public class LateSingleServiceProvider<T> : IServiceProvider<T> where T : class
{
    Func<T> _creator;

    T _value;

    public T Instance
    {
        get
        {
            if (_value == null)
            {
                _value = _creator();
            }
            return _value;
        }
    }

    public LateSingleServiceProvider(Func<T> creator)
    {
        _creator = creator;
    }

    public void Override(Func<T> creator)
    {
        _creator = creator;
    }
}
