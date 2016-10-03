﻿using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using static LanguageExt.Prelude;
using static LanguageExt.TypeClass;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LanguageExt.TypeClasses;

/// <summary>
/// Extension methods for Either
/// </summary>
public static class EitherExtensions
{
    /// <summary>
    /// Append the Right(x) of one option to the Right(y) of another.  If either of the
    /// options are Left then the result is Left
    /// </summary>
    /// <param name="lhs">Left-hand side of the operation</param>
    /// <param name="rhs">Right-hand side of the operation</param>
    /// <returns>lhs + rhs</returns>
    [Pure]
    public static Either<L, R> Append<SEMI, L, R>(this Either<L, R> lhs, Either<L, R> rhs) where SEMI : struct, Semigroup<R> =>
        from x in lhs
        from y in rhs
        select append<SEMI, R>(x, y);

    /// <summary>
    /// Add the bound values of x and y, uses an Add type-class to provide the add
    /// operation for type A.  For example x.Add<TInteger,int>(y)
    /// </summary>
    /// <typeparam name="ADD">Add of A</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>An option with y added to x</returns>
    [Pure]
    public static Either<L, R> Add<ADD, L, R>(this Either<L, R> x, Either<L, R> y) where ADD : struct, Addition<R> =>
        from a in x
        from b in y
        select add<ADD, R>(a, b);

    /// <summary>
    /// Find the difference between the two bound values of x and y, uses a Difference type-class 
    /// to provide the difference operation for type A.  For example x.Difference<TInteger,int>(y)
    /// </summary>
    /// <typeparam name="DIFF">Difference of A</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>An option with the difference between x and y</returns>
    [Pure]
    public static Either<L, R> Difference<DIFF, L, R>(this Either<L, R> x, Either<L, R> y) where DIFF : struct, Difference<R> =>
        from a in x
        from b in y
        select difference<DIFF, R>(a, b);

    /// <summary>
    /// Find the product between the two bound values of x and y, uses a Product type-class 
    /// to provide the product operation for type A.  For example x.Product<TInteger,int>(y)
    /// </summary>
    /// <typeparam name="PROD">Product of A</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>An option with the product of x and y</returns>
    [Pure]
    public static Either<L, R> Product<PROD, L, R>(this Either<L, R> x, Either<L, R> y) where PROD : struct, Product<R> =>
        from a in x
        from b in y
        select product<PROD, R>(a, b);

    /// <summary>
    /// Divide the two bound values of x and y, uses a Divide type-class to provide the divide
    /// operation for type A.  For example x.Divide<TDouble,double>(y)
    /// </summary>
    /// <typeparam name="DIV">Divide of A</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>An option x / y</returns>
    [Pure]
    public static Either<L, R> Divide<DIV, L, R>(this Either<L, R> x, Either<L, R> y) where DIV : struct, Divisible<R> =>
        from a in x
        from b in y
        select divide<DIV, R>(a, b);

    /// Apply y to x
    /// </summary>
    [Pure]
    public static Either<L, B> Apply<L, A, B>(this Either<L, Func<A, B>> x, Either<L, A> y) =>
        x.Apply<Either<L, B>, A, B>(y);

    /// <summary>
    /// Apply y and z to x
    /// </summary>
    [Pure]
    public static Either<L, C> Apply<L, A, B, C>(this Either<L, Func<A, B, C>> x, Either<L, A> y, Either<L, B> z) =>
        x.Apply<Either<L, C>, A, B, C>(y, z);

    /// <summary>
    /// Apply y to x
    /// </summary>
    [Pure]
    public static Either<L, Func<B, C>> Apply<L, A, B, C>(this Either<L, Func<A, Func<B, C>>> x, Either<L, A> y) =>
        x.Apply<Either<L, Func<B, C>>, A, B, C>(y);

    /// <summary>
    /// Apply y to x
    /// </summary>
    [Pure]
    public static Either<L, Func<B, C>> Apply<L, A, B, C>(this Either<L, Func<A, B, C>> x, Either<L, A> y) =>
        x.Apply<Either<L, Func<B, C>>, A, B, C>(y);

    /// <summary>
    /// Apply x, then y, ignoring the result of x
    /// </summary>
    [Pure]
    public static Either<L, B> Action<L, A, B>(this Either<L, A> x, Either<L, B> y) =>
        x.Action<Either<L, B>, A, B>(y);

    /// <summary>
    /// Extracts from a list of 'Either' all the 'Left' elements.
    /// All the 'Left' elements are extracted in order.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>An enumerable of L</returns>
    [Pure]
    public static IEnumerable<L> Lefts<L, R>(this IEnumerable<Either<L, R>> self)
    {
        foreach (var item in self)
        {
            if (item.IsLeft)
            {
                yield return item.LeftValue;
            }
        }
    }

    /// <summary>
    /// Extracts from a list of 'Either' all the 'Right' elements.
    /// All the 'Right' elements are extracted in order.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>An enumerable of L</returns>
    [Pure]
    public static IEnumerable<R> Rights<L, R>(this IEnumerable<Either<L, R>> self)
    {
        foreach (var item in self)
        {
            if (item.IsRight)
            {
                yield return item.RightValue;
            }
        }
    }

    /// <summary>
    /// Partitions a list of 'Either' into two lists.
    /// All the 'Left' elements are extracted, in order, to the first
    /// component of the output.  Similarly the 'Right' elements are extracted
    /// to the second component of the output.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>A tuple containing the an enumerable of L and an enumerable of R</returns>
    [Pure]
    public static Tuple<IEnumerable<L>, IEnumerable<R>> Partition<L, R>(this IEnumerable<Either<L, R>> self) =>
        Tuple(lefts(self), rights(self));

    /// <summary>
    /// Sum of the Either
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <param name="self">Either to count</param>
    /// <returns>0 if Left, or value of Right</returns>
    [Pure]
    public static int Sum<L>(this Either<L, int> self) =>
        self.IsBottom || self.IsLeft
            ? 0
            : self.RightValue;

    /// <summary>
    /// Partial application map
    /// </summary>
    /// <remarks>TODO: Better documentation of this function</remarks>
    [Pure]
    public static Either<L, Func<T2, R>> ParMap<L, T1, T2, R>(this Either<L, T1> self, Func<T1, T2, R> func) =>
        self.Map(curry(func));

    /// <summary>
    /// Partial application map
    /// </summary>
    /// <remarks>TODO: Better documentation of this function</remarks>
    [Pure]
    public static Either<L, Func<T2, Func<T3, R>>> ParMap<L, T1, T2, T3, R>(this Either<L, T1> self, Func<T1, T2, T3, R> func) =>
        self.Map(curry(func));

    [Pure]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Either<L, V> SelectMany<L, T, U, V>(this IEnumerable<T> self,
        Func<T, Either<L, U>> bind,
        Func<T, U, V> project
        )
    {
        var ta = self.Take(1).ToArray();
        if (ta.Length == 0) return Either<L, V>.Bottom;
        var u = bind(ta[0]);
        if (u.IsBottom) return Either<L, V>.Bottom;
        if (u.IsLeft) return Either<L, V>.Left(u.LeftValue);
        return project(ta[0], u.RightValue);
    }

    /// <summary>
    /// Match the two states of the Either and return a promise of a non-null R2.
    /// </summary>
    public static async Task<R2> MatchAsync<L, R, R2>(this Either<L, Task<R>> self, Func<R, R2> Right, Func<L, R2> Left) =>
        self.IsRight
            ? Either<L, R>.CheckNullRightReturn(Right(await self.RightValue))
            : Either<L, R>.CheckNullLeftReturn(Left(self.LeftValue));

    /// <summary>
    /// Match the two states of the Either and return a stream of non-null R2s.
    /// </summary>
    [Pure]
    public static IObservable<R2> MatchObservable<L, R, R2>(this Either<L, IObservable<R>> self, Func<R, R2> Right, Func<L, R2> Left) =>
        self.IsRight
            ? self.RightValue.Select(Right).Select(Either<L, R>.CheckNullRightReturn)
            : Observable.Return(Either<L, R>.CheckNullLeftReturn(Left(self.LeftValue)));

    /// <summary>
    /// Match the two states of the IObservable Either and return a stream of non-null R2s.
    /// </summary>
    [Pure]
    public static IObservable<R2> MatchObservable<L, R, R2>(this IObservable<Either<L, R>> self, Func<R, R2> Right, Func<L, R2> Left) =>
        self.Select(either => match(either, Right, Left));

    public static async Task<Either<L, R2>> MapAsync<L, R, R2>(this Either<L, R> self, Func<R, Task<R2>> map) =>
        self.IsRight
            ? await map(self.RightValue)
            : Left<L, R2>(self.LeftValue);

    public static async Task<Either<L, R2>> MapAsync<L, R, R2>(this Task<Either<L, R>> self, Func<R, Task<R2>> map)
    {
        var val = await self;
        return val.IsRight
            ? await map(val.RightValue)
            : Left<L, R2>(val.LeftValue);
    }

    public static async Task<Either<L, R2>> MapAsync<L, R, R2>(this Task<Either<L, R>> self, Func<R, R2> map)
    {
        var val = await self;
        return val.IsRight
            ? map(val.RightValue)
            : Left<L, R2>(val.LeftValue);
    }

    public static async Task<Either<L, R2>> MapAsync<L, R, R2>(this Either<L, Task<R>> self, Func<R, R2> map) =>
        self.IsRight
            ? map(await self.RightValue)
            : Left<L, R2>(self.LeftValue);

    public static async Task<Either<L, R2>> MapAsync<L, R, R2>(this Either<L, Task<R>> self, Func<R, Task<R2>> map) =>
        self.IsRight
            ? await map(await self.RightValue)
            : Left<L, R2>(self.LeftValue);


    public static async Task<Either<L, R2>> BindAsync<L, R, R2>(this Either<L, R> self, Func<R, Task<Either<L, R2>>> bind) =>
        self.IsRight
            ? await bind(self.RightValue)
            : Left<L, R2>(self.LeftValue);

    public static async Task<Either<L, R2>> BindAsync<L, R, R2>(this Task<Either<L, R>> self, Func<R, Task<Either<L, R2>>> bind)
    {
        var val = await self;
        return val.IsRight
            ? await bind(val.RightValue)
            : Left<L, R2>(val.LeftValue);
    }

    public static async Task<Either<L, R2>> BindAsync<L, R, R2>(this Task<Either<L, R>> self, Func<R, Either<L, R2>> bind)
    {
        var val = await self;
        return val.IsRight
            ? bind(val.RightValue)
            : Left<L, R2>(val.LeftValue);
    }

    public static async Task<Either<L, R2>> BindAsync<L, R, R2>(this Either<L, Task<R>> self, Func<R, Either<L, R2>> bind) =>
        self.IsRight
            ? bind(await self.RightValue)
            : Left<L, R2>(self.LeftValue);

    public static async Task<Either<L, R2>> BindAsync<L, R, R2>(this Either<L, Task<R>> self, Func<R, Task<Either<L, R2>>> bind) =>
        self.IsRight
            ? await bind(await self.RightValue)
            : Left<L, R2>(self.LeftValue);

    public static async Task<Unit> IterAsync<L, R>(this Task<Either<L, R>> self, Action<R> action)
    {
        var val = await self;
        if (val.IsRight) action(val.RightValue);
        return unit;
    }

    public static async Task<Unit> IterAsync<L, R>(this Either<L, Task<R>> self, Action<R> action)
    {
        if (self.IsRight) action(await self.RightValue);
        return unit;
    }

    public static async Task<int> CountAsync<L, R>(this Task<Either<L, R>> self) =>
        (await self).Count();

    public static async Task<int> SumAsync<L>(this Task<Either<L, int>> self) =>
        (await self).Sum();

    public static async Task<int> SumAsync<L>(this Either<L, Task<int>> self) =>
        self.IsRight
            ? await self.RightValue
            : 0;

    public static async Task<S> FoldAsync<L, R, S>(this Task<Either<L, R>> self, S state, Func<S, R, S> folder) =>
        (await self).Fold(state, folder);

    public static async Task<S> FoldAsync<L, R, S>(this Either<L, Task<R>> self, S state, Func<S, R, S> folder) =>
        self.IsRight
            ? folder(state, await self.RightValue)
            : state;

    public static async Task<bool> ForAllAsync<L, R>(this Task<Either<L, R>> self, Func<R, bool> pred) =>
        (await self).ForAll(pred);

    public static async Task<bool> ForAllAsync<L, R>(this Either<L, Task<R>> self, Func<R, bool> pred) =>
        self.IsRight
            ? pred(await self.RightValue)
            : true;

    public static async Task<bool> ExistsAsync<L, R>(this Task<Either<L, R>> self, Func<R, bool> pred) =>
        (await self).Exists(pred);

    public static async Task<bool> ExistsAsync<L, R>(this Either<L, Task<R>> self, Func<R, bool> pred) =>
        self.IsRight
            ? pred(await self.RightValue)
            : false;
}