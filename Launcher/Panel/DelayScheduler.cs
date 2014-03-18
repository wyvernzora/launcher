// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
// Launcher.Panel/DelayScheduler.cs
// --------------------------------------------------------------------------------
// Copyright (c) 2014, Jieni Luchijinzhou a.k.a Aragorn Wyvernzora
// 
// This file is a part of Launcher.Panel.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
// of the Software, and to permit persons to whom the Software is furnished to do 
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Timers;
using System.Windows.Threading;

namespace Launcher.Panel
{
    /// <summary>
    ///     Delayed event response.
    /// </summary>
    public class DelayScheduler
    {
        private readonly Dispatcher dispatcher;
        private Timer timer;

        /// <summary>
        ///     Constructor.
        ///     Specifies a dispatcher to run the handler on.
        /// </summary>
        /// <param name="dispatcher">
        ///     Dispatcher that owns the event-raising object,
        ///     null if handler can be invoked on the timer thread.
        /// </param>
        public DelayScheduler(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        /// <summary>
        ///     Schedules an event response to run after a delay.
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="handler"></param>
        public void Schedule(TimeSpan delay, Action handler)
        {
            Cancel();

            timer = new Timer(delay.TotalMilliseconds)
            {
                AutoReset = false
            };
            timer.Elapsed += (@s, e) =>
                dispatcher.Invoke(DispatcherPriority.Input, handler);

            timer.Start();
        }

        /// <summary>
        ///     Cancels the scheduled event response.
        /// </summary>
        public void Cancel()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}