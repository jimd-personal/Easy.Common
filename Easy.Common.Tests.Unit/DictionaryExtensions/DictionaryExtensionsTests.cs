﻿namespace Easy.Common.Tests.Unit.DictionaryExtensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using Easy.Common.Extensions;
    using Easy.Common.Interfaces;
    using Easy.Common.Tests.Unit.EasyDictionary;
    using NUnit.Framework;
    using Shouldly;
    using HashHelper = Easy.Common.HashHelper;

    [TestFixture]
    internal sealed class DictionaryExtensionsTests
    {
        [Test]
        public void When_getting_or_adding_value()
        {
            var oldValue = new Stopwatch();

            var someDic = new Dictionary<int, Stopwatch> { { 1, oldValue } };

            var newValue = new Stopwatch();

            oldValue.ShouldNotBe(newValue);

            someDic.GetOrAdd(1, () => newValue).ShouldBe(oldValue);
            someDic.GetOrAdd(2, () => newValue).ShouldBe(newValue);

            someDic.Remove(2);

            someDic.GetOrAdd(1, newValue).ShouldBe(oldValue);
            someDic.GetOrAdd(2, newValue).ShouldBe(newValue);
        }

        [Test]
        public void When_getting_value_or_default()
        {
            var someDic = new Dictionary<int, string> { { 1, "A" } };

            string value;
            someDic.TryGetValue(1, out value).ShouldBeTrue();
            value.ShouldBe("A");

            someDic.GetOrDefault(1).ShouldBe("A");

            someDic.GetOrDefault(4).ShouldBeNull();

            someDic.GetOrDefault(4, "not-there").ShouldBe("not-there");
            someDic.GetOrDefault(1, "not-there").ShouldBe("A");
        }

        [Test]
        public void When_converting_nameValueCollection_to_dictionary()
        {
            var someNameValueCollection = new NameValueCollection { { "1", "A" }, { "2", "B" } };

            var dictionary = someNameValueCollection.ToDictionary();

            dictionary.ShouldNotBeNull();
            dictionary.Count.ShouldBe(someNameValueCollection.Count);
            dictionary["1"].ShouldBe("A");
            dictionary["2"].ShouldBe("B");
        }

        [Test]
        public void When_converting_dictionary_to_concurrentDictionary()
        {
            var someDic = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };
            var concurrentDic = someDic.ToConcurrentDictionary();

            concurrentDic.ShouldNotBeNull();
            concurrentDic.ShouldBeOfType<ConcurrentDictionary<string, int>>();
            concurrentDic.Count.ShouldBe(someDic.Count);
            concurrentDic["A"].ShouldBe(1);
            concurrentDic["B"].ShouldBe(2);

            concurrentDic.ContainsKey("A").ShouldBeTrue();
            concurrentDic.ContainsKey("a").ShouldBeFalse();
        }

        [Test]
        public void When_converting_dictionary_to_concurrentDictionary_with_comparer()
        {
            var someDic = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };
            var concurrentDic = someDic.ToConcurrentDictionary(StringComparer.OrdinalIgnoreCase);

            concurrentDic.ShouldNotBeNull();
            concurrentDic.ShouldBeOfType<ConcurrentDictionary<string, int>>();
            concurrentDic.Count.ShouldBe(someDic.Count);
            concurrentDic["A"].ShouldBe(1);
            concurrentDic["B"].ShouldBe(2);

            concurrentDic.ContainsKey("A").ShouldBeTrue();
            concurrentDic.ContainsKey("a").ShouldBeTrue();
        }

        [Test]
        public void When_comparing_dictionaries()
        {
            IDictionary<string, int> left = new Dictionary<string, int>();
            IDictionary<string, int> right = new Dictionary<string, int>();

            DictionaryExtensions.Equals(left, right).ShouldBeTrue();

            left.Add("A", 1);
            DictionaryExtensions.Equals(left, right).ShouldBeFalse();

            right.Add("A", 1);
            DictionaryExtensions.Equals(left, right).ShouldBeTrue();

            left["A"] = 2;
            DictionaryExtensions.Equals(left, right).ShouldBeFalse();

            right["A"] = 2;
            right["B"] = 3;
            DictionaryExtensions.Equals(left, right).ShouldBeFalse();

            left["B"] = 3;
            DictionaryExtensions.Equals(left, right).ShouldBeTrue();

            DictionaryExtensions.Equals(((IDictionary<string, int>) null), right).ShouldBeFalse();

            left = right;
            DictionaryExtensions.Equals(left, null).ShouldBeFalse();
            
            DictionaryExtensions.Equals(((IDictionary<string, int>) null), null).ShouldBeTrue();
        }

        [Test]
        public void When_comparing_dictionaries_with_comparer()
        {
            IDictionary<int, string> left = new Dictionary<int, string>();
            IDictionary<int, string> right = new Dictionary<int, string>();

            var comparer = StringComparer.OrdinalIgnoreCase;

            DictionaryExtensions.Equals(left, right, comparer).ShouldBeTrue();

            left.Add(1, "A");
            DictionaryExtensions.Equals(left, right, comparer).ShouldBeFalse();

            right.Add(1, "A");
            DictionaryExtensions.Equals(left, right, comparer).ShouldBeTrue();

            left[2] = "A";
            DictionaryExtensions.Equals(left, right, comparer).ShouldBeFalse();

            right[2] = "A";
            right[3] = "B";
            DictionaryExtensions.Equals(left, right, comparer).ShouldBeFalse();

            left[3] = "B";
            DictionaryExtensions.Equals(left, right, comparer).ShouldBeTrue();

            left[3] = "b";
            DictionaryExtensions.Equals(left, right, comparer).ShouldBeTrue();

            DictionaryExtensions.Equals(((IDictionary<int, string>)null), right, comparer).ShouldBeFalse();

            left = right;
            DictionaryExtensions.Equals(left, null, comparer).ShouldBeFalse();

            DictionaryExtensions.Equals(((IDictionary<int, string>)null), null, comparer).ShouldBeTrue();
        }

        [Test]
        public void When_comparing_easy_dictionaries()
        {
            IReadOnlyDictionary<string, Person> left = new EasyDictionary<string, Person>(p => p.Id);
            IReadOnlyDictionary<string, Person> right = new EasyDictionary<string, Person>(p => p.Id);

            DictionaryExtensions.Equals(left, right).ShouldBeTrue();

            ((IEasyDictionary<string, Person>)left).Add(new Person("A", 1));
            DictionaryExtensions.Equals(left, right).ShouldBeFalse();

            ((IEasyDictionary<string, Person>)right).Add(new Person("A", 1));
            DictionaryExtensions.Equals(left, right).ShouldBeTrue();
            
            DictionaryExtensions.Equals(((IDictionary<int, string>)null), null).ShouldBeTrue();
        }

        private sealed class Person : Equatable<Person>
        {
            public Person(string id, int age)
            {
                Id = id;
                Age = age;
            }

            public string Id { get; }
            public int Age { get; }

            public override int GetHashCode() => HashHelper.GetHashCode(Id, Age);
        }
    }
}