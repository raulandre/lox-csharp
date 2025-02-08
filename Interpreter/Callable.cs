namespace Interpreter;

public interface ICallable
{
    object Call(Interpreter interpreter, List<object> args);
    int Arity();
}
