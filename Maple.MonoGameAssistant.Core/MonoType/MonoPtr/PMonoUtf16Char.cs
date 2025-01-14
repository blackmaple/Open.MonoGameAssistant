﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Maple.MonoGameAssistant.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct PMonoUtf16Char(nint ptr)
    {
        [MarshalAs(UnmanagedType.SysInt)]
        readonly nint _ptr = ptr;
        public static implicit operator nint(PMonoUtf16Char ptr) => ptr._ptr;
        public static implicit operator PMonoUtf16Char(nint ptr) => new(ptr);
        public unsafe static implicit operator PMonoUtf16Char(char* ptr) => new nint(ptr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Valid() => _ptr != nint.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string? ToString()
        {
            if (false == Valid())
            {
                return default;
            }
            return new string((char*)_ptr);

        }
    }





}
