using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Fudie.Tests.Common;

public static class AsyncQueryable
{
    public static IQueryable<T> Of<T>(params T[] items)
        => new TestAsyncEnumerable<T>(items.AsQueryable());

    public static IQueryable<T> Of<T>(IEnumerable<T> items)
        => new TestAsyncEnumerable<T>(items.AsQueryable());

    public static IQueryable<T> Empty<T>()
        => new TestAsyncEnumerable<T>(Enumerable.Empty<T>().AsQueryable());
}

internal class TestAsyncEnumerable<T>(IQueryable<T> source)
    : EnumerableQuery<T>(source), IAsyncEnumerable<T>, IQueryable<T>
{
    IQueryProvider IQueryable.Provider
        => new TestAsyncQueryProvider<T>(source.Provider);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default)
        => new TestAsyncEnumerator<T>(source.GetEnumerator());
}

internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
}

internal class TestAsyncQueryProvider<T>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
        => new TestAsyncEnumerable<T>(inner.CreateQuery<T>(expression));

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncEnumerable<TElement>(inner.CreateQuery<TElement>(expression));

    public object? Execute(Expression expression)
        => inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression)
        => inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken ct = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var result = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!
            .MakeGenericMethod(resultType)
            .Invoke(inner, [expression]);

        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [result])!;
    }
}
