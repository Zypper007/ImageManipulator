using System;

namespace ImageManipulator
{
    class HistoryService<T>
    {
        private bool _lastCanRedo = false;
        private bool _lastCanUndo = false;

        T[] _Stack;

        // HistoryPointer pokazuje gdzie ma wstawiać nowe elementy
        private int _historyPointer = 0;
        private int HistoryPointer
        {
            get => _historyPointer;
            set
            {
                if (value >= _Stack.Length)
                    _historyPointer = value - _Stack.Length;
                else if (value < 0)
                    _historyPointer = value + _Stack.Length;
                else _historyPointer = value;
            }
        }

        public T CurrentState { get; private set; }

        // Zwraca liczbę możliwych cofnięć
        // nie może być wyższy niż rozmiar tablicy
        private int _undoCount = 0;
        public int UndoCount
        {
            get => _undoCount;
            set
            {
                if (value > _Stack.Length)
                    _undoCount = _Stack.Length;
                else _undoCount = value;
            }
        }

        // Zwraca liczbę możliwych przejść dalej
        public int RedoCount { get; private set; } = 0;

        public bool CanUndo => (UndoCount > 0) ? true : false;
        public bool CanRedo => (RedoCount > 0) ? true : false;
        
        public delegate void CanPropertyChanging(BoolArgs args);
        public event CanPropertyChanging CanUndoEvent;
        public event CanPropertyChanging CanRedoEvent;


        public HistoryService(T CurrentState, int HistoryLength)
        {
            this.CurrentState = CurrentState;
            _Stack = new T[HistoryLength];
        }

        public void NewState(T entity)
        {
            _Stack[HistoryPointer] = CurrentState;
            HistoryPointer++;
            UndoCount++;
            RedoCount = 0;
            CurrentState = entity;
            OnPropertyChangeCanUndo();
            OnPropertyChangeCanRedo();
        }

        public T Undo()
        {
            if (CanUndo)
            {
                _Stack[HistoryPointer] = CurrentState;
                HistoryPointer--;
                UndoCount--;
                RedoCount++;
                var entity = _Stack[HistoryPointer];
                CurrentState = entity;
                OnPropertyChangeCanUndo();
                OnPropertyChangeCanRedo();
                return CurrentState;
            }
            throw new Exception("Can't Undo");
        }

        public T Redo()
        {
            if(CanRedo)
            {
                HistoryPointer++;
                var entity = _Stack[HistoryPointer];
                RedoCount--;
                UndoCount++;
                CurrentState = entity;
                OnPropertyChangeCanUndo();
                OnPropertyChangeCanRedo();
                return CurrentState;
            }
            throw new Exception("Can't Redo");
        }

        private void OnPropertyChangeCanUndo()
        {
            var canUndoNow = CanUndo;
            if(_lastCanUndo != canUndoNow)
            {
                CanUndoEvent?.Invoke(new BoolArgs(canUndoNow));
                _lastCanUndo = canUndoNow;
            }

        }

        private void OnPropertyChangeCanRedo() 
        {
            var canRedoNow = CanRedo;
            if (_lastCanRedo != canRedoNow)
            {
                CanRedoEvent?.Invoke(new BoolArgs(canRedoNow));
                _lastCanRedo = canRedoNow;
            }
        }
    }

    class BoolArgs
    {
        public bool value { get; }
        public BoolArgs(bool value)
        {
            this.value = value;
        }
    }
}
