using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Observable.Aliases;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Threading;

namespace InteractiveExample
{
    static class Program
    {
        [STAThread]
        public static void Main()
        {
            var p = new Person();
            var sub1 = p.GetNameChanges()
                .Subscribe(name => Debug.WriteLine("SUB1 - " + name));

            SetName(p, "John");
            SetName(p, "Kent");

            #region Filter

            var sub2 = p.GetNameChanges()
                .Do(name => Debug.WriteLine("I'm about to decide whether to filter out " + name))
                .Filter(name => name != "Johnny")
                .Subscribe(name => Debug.WriteLine("SUB2 - " + name));

            SetName(p, "Johnny");
            SetName(p, "Brad");

            sub2.Dispose();

            SetName(p, "Leroy");

            #endregion

            #region distinct

            //var sub3 = p.GetNameChanges()
            //    .Distinct()
            //    .Subscribe(name => Debug.WriteLine("SUB3 - " + name));

            //var sub4 = p.GetNameChanges()
            //    .DistinctUntilChanged()
            //    .Subscribe(name => Debug.WriteLine("SUB4 - " + name));

            //SetName(p, "Frank");

            //SetName(p, "Frank");

            //SetName(p, "Randy");

            //SetName(p, "Frank");

            //sub3.Dispose();
            //sub4.Dispose();

            #endregion

            sub1.Dispose();

            #region Mapping

            //var sub5 = p.GetAgeChanges()
            //    .Subscribe(age => Debug.WriteLine(p.Name + " is " + age));

            //SetBirthDate(p, new DateTime(1977, 3, 8));
            //sub5.Dispose();
            #endregion

            #region Timer & Zip
            var line1 = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1.9)).Map(_ => Faker.Name.GetFirstName()).Take(5);
            var line2 = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2.4)).Map(_ => Faker.Name.GetFirstName()).Take(5);
            var line3 = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)).Map(_ => Faker.Name.GetFirstName()).Take(5);

            
            var sub6 = line1.Zip(line2, (l1, l2) => Tuple.Create(l1, l2))
                .Zip(line3, (team, l3) => Tuple.Create(team.Item1, team.Item2, l3))
                .Subscribe(team => 
                    Debug.WriteLine(string.Format("{4}We've got Team {0}!{4}  Here comes {1} {2} and {3}!", 
                                    Faker.Company.GetCatchPhrase(), team.Item1, team.Item2, team.Item3, Environment.NewLine)));
            Thread.Sleep(30000);
            sub6.Dispose();
            #endregion

        }

        public static void SetName(Person p, string name)
        {
            Debug.WriteLine("");
            Debug.WriteLine("Changing name to " + name);
            p.Name = name;
        }

        public static void SetBirthDate(Person p, DateTime birthDate)
        {
            Debug.WriteLine("");
            Debug.WriteLine("Changing birth date to " + birthDate.ToShortDateString());
            p.BirthDate = birthDate;
        }
    }
}
