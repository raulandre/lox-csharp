namespace Lox.Extensions;

public static class TupleExtensions
{
    public static double Sum(this (double, double) tuple)
        => tuple.Item1 + tuple.Item2;

    public static double Subtract(this (double, double) tuple)
        => tuple.Item1 - tuple.Item2;

    public static double Multiply(this (double, double) tuple)
        => tuple.Item1 * tuple.Item2;

    public static double Divide(this (double, double) tuple)
        => tuple.Item1 / tuple.Item2;

    public static T Apply<T>(this (double, double) tuple, Func<double, double, T> func)
        => func(tuple.Item1, tuple.Item2);
}