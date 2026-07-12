namespace Necroperator.Extensions
{
    public class Disposable : IDisposable
    {
        private readonly Action disposeAction;

        private Disposable(Action disposeAction)
        {
            this.disposeAction = disposeAction;
        }

        public static IDisposable Create(Action disposeAction)
        {
            return new Disposable(disposeAction);
        }

        public void Dispose()
        {
            disposeAction();
        }
    }
}
