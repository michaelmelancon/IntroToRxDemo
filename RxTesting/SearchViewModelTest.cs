using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using DictionarySuggestDemo;
using DictionarySuggestDemo.Models;
using DictionarySuggestDemo.ViewModels;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Collections.Generic;

namespace RxTesting
{
    [TestClass]
    public class SearchViewModelTest
    {

        [TestMethod]
        public void Should_search_after_pause_in_text_changes()
        {
            var schedulers = new TestSchedulerProvider();
            var serviceMock = new Mock<ISuggestService>();
            serviceMock.Setup(service => service.GetSuggestions(It.IsAny<string>())).Returns(Task.FromResult(new[] { "Reactive", "Reactive Extensions" }));

            
            var viewModel = new SearchViewModel(serviceMock.Object, schedulers);


            Assert.IsNull(viewModel.SearchText);
            Assert.IsNull(viewModel.Suggestions);
            Assert.IsFalse(viewModel.HasSuggestions);
            viewModel.SearchText = "reactive";
            schedulers.Default.AdvanceTo(500);

            Assert.IsNull(viewModel.Suggestions);
            schedulers.Default.AdvanceBy(1000); 
            serviceMock.Verify(service => service.GetSuggestions(It.IsAny<string>()));

            // not finished :(

            Assert.IsTrue(viewModel.HasSuggestions);            
        }
    }

    public class TestSchedulerProvider : ISchedulerProvider
    {
        public TestSchedulerProvider()
        {
            Default = new TestScheduler();
            Dispatcher = new TestScheduler();
            ThreadPool = new TestScheduler();
        }

        public TestScheduler Default
        {
            get; private set;
        }

        public TestScheduler Dispatcher
        {
            get; private set;
        }

        public TestScheduler ThreadPool
        {
            get;
            private set;
        }



        IScheduler ISchedulerProvider.Default
        {
            get { return Default; }
        }

        IScheduler ISchedulerProvider.Dispatcher
        {
            get { return Dispatcher; }
        }

        IScheduler ISchedulerProvider.ThreadPool
        {
            get { return ThreadPool; }
        }

    }

}
