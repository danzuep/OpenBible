using System.Collections;
using Bible.Backend.Models;

public class UsxContentIterator : IEnumerable<IUsxBase>, IEnumerator<IUsxBase>
{
    private readonly Stack<IEnumerator> _stack = new();
    private IUsxBase _current;

    public UsxContentIterator(UsxScriptureBook book)
    {
        _current = book;
        if (book.Content != null)
        {
            _stack.Push(book.Content.GetEnumerator());
        }
    }

    public IUsxBase Current => _current;

    object IEnumerator.Current => _current;

    public void Dispose()
    {
        // Nothing to dispose
    }

    public IEnumerator<IUsxBase> GetEnumerator() => this;

    public bool MoveNext()
    {
        while (_stack.Any())
        {
            var enumerator = _stack.Peek();
            if (!enumerator.MoveNext())
            {
                _stack.Pop();
                continue;
            }

            switch (enumerator.Current)
            {
                case IUsxBase usxBase:
                    _current = usxBase;
                    if (usxBase is UsxContent combination && combination.Content != null)
                    {
                        _stack.Push(combination.Content.GetEnumerator());
                    }
                    return true;
                default:
                    continue;
            }
        }
        return false;
    }

    public void Reset()
    {
        _stack.Clear();
        _current = null;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}