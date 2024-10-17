﻿using System.Runtime.InteropServices;

namespace Maple.MonoGameAssistant.AndroidCore.JNI.Primitive
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct JFLOAT(float val)
    {
        [MarshalAs(UnmanagedType.R4)]
        readonly float _value = val;
        public static implicit operator JFLOAT(float val) => new(val);
        public static implicit operator float(JFLOAT val) => val._value;

    }

}
