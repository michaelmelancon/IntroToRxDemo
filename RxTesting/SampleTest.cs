using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using DictionarySuggestDemo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace RxTesting
{
    [TestClass]
    public class SampleTest
    {
        Mock<IMyModel> _myModelMock;
        TestSchedulerProvider _schedulerProvider;
        MyViewModel _viewModel;

        [TestInitialize]
        public void SetUp()
        {
            _myModelMock = new Mock<IMyModel>();
            _schedulerProvider = new TestSchedulerProvider();
            _viewModel = new MyViewModel(_myModelMock.Object, _schedulerProvider);
        }

        [TestMethod]
        public void Should_add_to_Prices_when_Model_publishes_price()
        {
            decimal expected = 1.23m;
            var priceStream = new Subject<decimal>();
            _myModelMock.Setup(svc => svc.PriceStream(It.IsAny<string>())).Returns(priceStream);
            _viewModel.Show("SomeSymbol");
            //Schedule the OnNext
            _schedulerProvider.ThreadPool.Schedule(() => priceStream.OnNext(expected));
            Assert.AreEqual(0, _viewModel.Prices.Count);
            //Execute the OnNext action
            _schedulerProvider.ThreadPool.AdvanceBy(1);
            Assert.AreEqual(0, _viewModel.Prices.Count);
            //Execute the OnNext handler
            _schedulerProvider.Dispatcher.AdvanceBy(1);
            Assert.AreEqual(1, _viewModel.Prices.Count);
            Assert.AreEqual(expected, _viewModel.Prices.First());
        }

        [TestMethod]
        public void Should_disconnect_if_no_prices_for_10_seconds()
        {
            var timeoutPeriod = TimeSpan.FromSeconds(10);
            var priceStream = Observable.Never<decimal>();
            _myModelMock.Setup(svc => svc.PriceStream(It.IsAny<string>())).Returns(priceStream);
            _viewModel.Show("SomeSymbol");
            _schedulerProvider.ThreadPool.AdvanceBy(timeoutPeriod.Ticks - 1);
            Assert.IsTrue(_viewModel.IsConnected);
            _schedulerProvider.ThreadPool.AdvanceBy(timeoutPeriod.Ticks);
            Assert.IsFalse(_viewModel.IsConnected);
        }
    }

    public class MyViewModel : IMyViewModel
    {
        private readonly IMyModel _myModel;
        private readonly ObservableCollection<decimal> _prices;
        private readonly ISchedulerProvider _schedulers;
        public MyViewModel(IMyModel myModel, ISchedulerProvider schedulers)
        {
            _schedulers = schedulers;
            _myModel = myModel;
            _prices = new ObservableCollection<decimal>();
        }

        public void Show(string symbol)
        {
            //TODO: resource mgt, exception handling etc...
            _myModel.PriceStream(symbol)
                .SubscribeOn(_schedulers.ThreadPool)
                .ObserveOn(_schedulers.Dispatcher)
                .Timeout(TimeSpan.FromSeconds(10), _schedulers.ThreadPool)
                .Subscribe(
                    Prices.Add,
                    ex =>
                    {
                        if (ex is TimeoutException)
                            IsConnected = false;
                    });
            IsConnected = true;
        }

        public ObservableCollection<decimal> Prices
        {
            get { return _prices; }
        }

        public bool IsConnected { get; private set; }
    }

    public interface IMyViewModel
    {
        bool IsConnected { get; }
        ObservableCollection<decimal> Prices { get; }
        void Show(string symbol);
    }

    public interface IMyModel
    {
        IObservable<decimal> PriceStream(string symbol);
    }
}
