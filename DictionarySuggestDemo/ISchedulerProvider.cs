using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.PlatformServices;
using System.Text;
using System.Threading.Tasks;

namespace DictionarySuggestDemo
{
    public interface ISchedulerProvider
    {
        IScheduler Default { get; }
        IScheduler Dispatcher { get; }
        IScheduler ThreadPool { get; }
    }

    public class DefaultSchedulerProvider : ISchedulerProvider
    {    
        public IScheduler Default
        {
	        get { return Scheduler.Default; }
        }

        public IScheduler Dispatcher
        {
	        get {return DispatcherScheduler.Current ; }
        }

        public IScheduler ThreadPool
        {
            get { return Scheduler.ThreadPool; }
        }
    }
}
