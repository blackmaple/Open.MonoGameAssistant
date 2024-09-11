﻿using System.Diagnostics.CodeAnalysis;

namespace Maple.MonoGameAssistant.UITask
{
    internal class UITaskState_Func<T_CONTEXT, T_RETURN>(T_CONTEXT gameContext, Func<T_CONTEXT, T_RETURN> func)
        : UITaskState<T_CONTEXT>(gameContext)
        where T_CONTEXT : class
    {
        public Func<T_CONTEXT, T_RETURN> Func { get; } = func;

        public T_RETURN? ReturnValue { set; get; }

        public bool TryGetValue([MaybeNullWhen(false)] out T_RETURN val)
        {
            val = ReturnValue;
            return this.ExecSuccess;
        }

        protected override void ExecuteImp()
        {
            this.ReturnValue = this.Func.Invoke(this.Context);
        }
    }

}
