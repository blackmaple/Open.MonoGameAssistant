﻿using Maple.MonoGameAssistant.Common;
using Maple.MonoGameAssistant.HotKey.Abstractions;
using Maple.MonoGameAssistant.WinApi;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Maple.MonoGameAssistant.HotKey
{
    internal sealed class UnmanagedThreadMessage : NotifyThreadMessage
    {
        int ThreadId { set; get; }
        public UnmanagedThreadMessage(IWinMsgNotifyService winMsgNotifyService) : base(winMsgNotifyService)
        {
            var taskThreadMessageLoop = new Task(ThreadMessageLoop, CancellationToken.None, TaskCreationOptions.LongRunning);
            taskThreadMessageLoop.Start();
        }



        void ThreadMessageLoop()
        {
            using (Logger.Running())
            {

                this.ThreadId = WindowsRuntime.GetCurrentThreadId();

                WindowsRuntime.MSG msg = default;
                while (!PostedQuit)
                {

                    if (WindowsRuntime.PeekMessage(ref msg, WindowsRuntime.MessageThreadHandle, 0, 0, WindowsRuntime.PM_NOREMOVE))
                    {
                        if (PostedQuit)
                        {
                            break;
                        }
                        WindowsRuntime.GetMessage(ref msg, WindowsRuntime.MessageThreadHandle, 0, 0);
                        if (PostedQuit)
                        {
                            break;
                        }
                        this.NotifyChannel.Writer.TryWrite(new WinMsgNotifyDTO() { Msg = msg.message, WParam = msg.wParam, LParam = msg.lParam });
                    }
                    else
                    {
                        if (PostedQuit)
                        {
                            break;
                        }
                        //WaitMessage 可能会失败 所以不判断返回值
                        WindowsRuntime.WaitMessage();
                        if (PostedQuit)
                        {
                            break;
                        }
                    }
                }
                this.NotifyChannel.Writer.Complete();

            }
        }

        public sealed override bool TrySendThreadMsg(EnumWindowMessage msg, nint wParam, nint lParam)
        {
            return !this.PostedQuit && this.ThreadId != 0 && WindowsRuntime.PostThreadMessage(this.ThreadId, msg, wParam, lParam);
        }


 
        

    }
}