using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageManipulator
{
    class HistoryService<T>
    {
        T CurrentState;
        T[] UndoStack;
        T[] RedoStack;
        private bool _lastCanRedo = false;
        private bool _lastCanUndo = false;
        private int Size;
        private int _stateCount = 0;
        private int _undoCount = 0;
        private int _redoCount = 0;
        private int StateCount
        {
            get => _stateCount;
            set
            {
                if (value >= Size)
                    _stateCount = value - Size;
                else _stateCount = value;
            }
        }

       
        public int UndoCount
        {
            get => _undoCount;
            private set
            {
                _undoCount = value;
                OnPropertyChangeCanUndo();
            }
        }
        public int RedoCount 
        { 
            get => _redoCount;
            private set
            {
                _redoCount = value;
                OnPropertyChangeCanRedo();
            }
        }

        public delegate void CanPropertyChanging(BoolArgs args);
        public event CanPropertyChanging CanUndoEvent;
        public event CanPropertyChanging CanRedoEvent;

        public bool CanRedo 
        { 
            get
            {
                if (RedoCount > 0)
                    return true;
                return false;
            }
        }

        public bool CanUndo
        {
            get
            {
                if (UndoCount > 0)
                    return true;
                return false;
            }
        }


        public HistoryService(T CurrentState, int HistoryLength)
        {
            this.CurrentState = CurrentState;
            Size = HistoryLength;
            UndoStack = new T[HistoryLength];
            RedoStack = new T[HistoryLength];
        }

        public void NewState(T entity)
        {
            StateCount++;
            Set(UndoRedo.UNDO, CurrentState);
            CurrentState = entity;
        }

        public T Undo()
        {
            UndoCount--;
            var entity = UndoStack[UndoCount];

            Set(UndoRedo.REDO, CurrentState);
            CurrentState = entity;

            return entity;
        }

        public T Redo()
        {
            RedoCount--;
            var entity = RedoStack[RedoCount];

            Set(UndoRedo.UNDO, CurrentState);
            CurrentState = entity;

            return entity;
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

        private void Set(UndoRedo undoRedo, T entity)
        {
            if(undoRedo == UndoRedo.UNDO)
            {
                if (UndoCount >= Size)
                    UndoStack[StateCount] = entity;
                else
                {
                    UndoStack[UndoCount] = entity;
                    UndoCount++;
                }
                OnPropertyChangeCanUndo();
            }
            else
            {
                RedoStack[RedoCount] = entity;
                RedoCount++;
                OnPropertyChangeCanRedo();
            }
                
        }

        private enum UndoRedo
        {
            UNDO,
            REDO
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
