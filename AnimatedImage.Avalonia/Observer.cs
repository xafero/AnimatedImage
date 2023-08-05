using Avalonia;
using System;

namespace AnimatedImage.Avalonia
{
    internal class Observer<T> : IObserver<T>
    {
        private Action<T> _onnext;

        public Observer(Action<T> onnext)
        {
            _onnext = onnext;
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        public void OnNext(T value) => _onnext(value);
    }

    internal static class Observer
    {
        public static Observer<AvaloniaPropertyChangedEventArgs<T>> Create<T>(Action<AvaloniaObject, T> onnext)
            => new Observer<AvaloniaPropertyChangedEventArgs<T>>(e => onnext(e.Sender, e.NewValue.Value));
    }
}
