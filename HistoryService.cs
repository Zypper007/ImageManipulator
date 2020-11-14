using System;

namespace ImageManipulator
{
    class HistoryService<T>
    {
        private bool _lastCanRedo = false;
        private bool _lastCanUndo = false;

        T[] _Stack;

        private int _stackPointer = 0;
        private int StackPointer
        {
            get => _stackPointer;
            set
            {
                if (value >= _Stack.Length)
                    _stackPointer = value - _Stack.Length;
                else _stackPointer = value;
            }
        }

        private int _stateCounter = 0;
        private int StateCounter
        {
            get => _stateCounter;
            set
            {
                if (value > _Stack.Length)
                    _stateCounter = _Stack.Length;
                else _stateCounter = value;
            }
        }


        public T CurrentState { get; private set; }
        public int UndoCount => StackPointer;
        public int RedoCount => StateCounter - StackPointer;
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
            _Stack[StackPointer] = CurrentState;
            StateCounter++;
            StackPointer++;
            CurrentState = entity;
            OnPropertyChangeCanUndo();
        }

        public T Undo()
        {
            if (CanUndo)
            {
                _Stack[StackPointer] = CurrentState;
                StackPointer--;
                var entity = _Stack[StackPointer];
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
                StackPointer++;
                var entity = _Stack[StackPointer];
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
