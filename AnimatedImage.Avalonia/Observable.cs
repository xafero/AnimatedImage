using System;
using System.Collections.Generic;

namespace AnimatedImage.Avalonia
{
    internal class Observable<T> : IObservable<T>, IDisposable
    {
        private List<IObserver<T>> _observers = new();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new SubscribeDisposable<T>(_observers, observer);
        }

        public void OnNext(T value)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(value);
            }
        }

        public void Dispose()
        {
            _observers.Clear();
        }

        class SubscribeDisposable<U> : IDisposable
        {
            private List<IObserver<U>> _observers;
            private IObserver<U> _observer;

            public SubscribeDisposable(List<IObserver<U>> observers, IObserver<U> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                _observers.Remove(_observer);
            }
        }
    }
}
