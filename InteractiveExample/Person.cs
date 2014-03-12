using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Observable.Aliases;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveExample
{
    public class Person
    {
        private string _name;
        private DateTime _birthDate;
        private Subject<DateTime> _birthDateChanges;

        public Person()
        {
            _birthDateChanges = new Subject<DateTime>();
        }

        public event EventHandler<NameChangedEventArgs> NameChanged;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                if (NameChanged != null)
                    NameChanged(this, new NameChangedEventArgs(_name));
            }
        }

        public DateTime BirthDate
        {
            get
            {
                return _birthDate;
            }
            set
            {
                _birthDate = value;
                _birthDateChanges.OnNext(value);
            }
        }

        public IObservable<string> GetNameChanges()
        {
            return Observable.FromEventPattern<NameChangedEventArgs>(this, "NameChanged")
                .Map(ep => ep.EventArgs.Value);
        }

        public IObservable<DateTime> GetBirthDateChanges()
        {
            return _birthDateChanges.AsObservable();
        }

        public IObservable<int> GetAgeChanges()
        {
            return GetBirthDateChanges().Map(birthDate => 
                ((int)(DateTime.Now - birthDate).TotalDays) / 365);
        }
    }

    public class NameChangedEventArgs : EventArgs
    {
        public NameChangedEventArgs(string value)
	    {
            Value = value;
        }

        public string Value {get;private set;}
    }

}
