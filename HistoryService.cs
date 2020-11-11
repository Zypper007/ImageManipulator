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
        List<T> UndoStack;
        List<T> RedoStack;
        private bool _lastCanRedo = false;
        private bool _lastCanUndo = false;
        int Size;

        public delegate void CanPropertyChanging(BoolArgs args);
        public event CanPropertyChanging CanUndoEvent;
        public event CanPropertyChanging CanRedoEvent;

        public bool CanRedo 
        { 
            get
            {
                if (RedoStack.Count > 0)
                    return true;
                return false;
            }
        }

        public bool CanUndo
        {
            get
            {
                if (UndoStack.Count > 0)
                    return true;
                return false;
            }
        }


        public HistoryService(T CurrentState, int HistoryLength)
        {
            this.CurrentState = CurrentState;
            Size = HistoryLength;
            UndoStack = new List<T>();
            RedoStack = new List<T>();
        }

        public void NewState(T entity)
        {
            Set(UndoRedo.UNDO, CurrentState);
            CurrentState = entity;
        }

        public T Undo()
        {
            var idx = UndoStack.Count - 1;
            var entity = UndoStack[idx];
            UndoStack.RemoveAt(idx);

            Set(UndoRedo.REDO, CurrentState);
            CurrentState = entity;

            OnPropertyChangeCanUndo();
            return entity;
        }

        public T Redo()
        {
            var idx = RedoStack.Count - 1;
            var entity = RedoStack[RedoStack.Count - 1];
            RedoStack.RemoveAt(idx);

            Set(UndoRedo.UNDO, CurrentState);
            CurrentState = entity;

            OnPropertyChangeCanRedo();
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
                if (UndoStack.Count + 1 >= Size)
                    UndoStack.RemoveAt(0);
                UndoStack.Add(entity);
                OnPropertyChangeCanUndo();
            }
            else
            {
                if (RedoStack.Count + 1 >= Size)
                    RedoStack.RemoveAt(0);
                RedoStack.Add(entity);
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
