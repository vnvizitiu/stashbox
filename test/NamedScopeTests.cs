﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stashbox.Exceptions;
using Stashbox.Lifetime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stashbox.Tests
{
    [TestClass]
    public class NamedScopeTests
    {
        [TestMethod]
        public void NamedScope_Simple_Resolve_Prefer_Named()
        {
            var inst = new StashboxContainer()
                .Register<ITest, Test11>()
                .Register<ITest, Test>(config => config.InNamedScope("A"))
                .Register<ITest, Test1>()
                .BeginScope("A")
                .Resolve<ITest>();

            Assert.IsInstanceOfType(inst, typeof(Test));
        }

        [TestMethod]
        public void NamedScope_Dependency_Resolve_Prefer_Named()
        {
            var inst = new StashboxContainer()
                .Register<Test2>()
                .Register<ITest, Test11>()
                .Register<ITest, Test>(config => config.InNamedScope("A"))
                .Register<ITest, Test1>()
                .BeginScope("A")
                .Resolve<Test2>();

            Assert.IsInstanceOfType(inst.Test, typeof(Test));
        }

        [TestMethod]
        public void NamedScope_Simple_Resolve_Wrapper_Prefer_Named()
        {
            var scope = new StashboxContainer()
                .Register<ITest, Test11>()
                .Register<ITest, Test>(config => config.InNamedScope("A"))
                .Register<ITest, Test1>()
                .BeginScope("A");

            var func = scope.Resolve<Func<ITest>>();
            var lazy = scope.Resolve<Lazy<ITest>>();
            var tuple = scope.Resolve<Tuple<ITest>>();
            var enumerable = scope.Resolve<IEnumerable<ITest>>();
            var all = scope.ResolveAll<ITest>();

            Assert.IsInstanceOfType(func(), typeof(Test));
            Assert.IsInstanceOfType(lazy.Value, typeof(Test));
            Assert.IsInstanceOfType(tuple.Item1, typeof(Test));
            Assert.IsInstanceOfType(enumerable.Last(), typeof(Test));
            Assert.IsInstanceOfType(all.First(), typeof(Test));
        }

        [TestMethod]
        public void NamedScope_Dependency_Resolve_Wrapper_Prefer_Named()
        {
            var inst = new StashboxContainer()
                .Register<Test3>()
                .Register<ITest, Test11>()
                .Register<ITest, Test>(config => config.InNamedScope("A"))
                .Register<ITest, Test1>()
                .BeginScope("A")
                .Resolve<Test3>();

            Assert.IsInstanceOfType(inst.Func(), typeof(Test));
            Assert.IsInstanceOfType(inst.Lazy.Value, typeof(Test));
            Assert.IsInstanceOfType(inst.Enumerable.Last(), typeof(Test));
            Assert.IsInstanceOfType(inst.Tuple.Item1, typeof(Test));
        }

        [TestMethod]
        public void NamedScope_Simple_Resolve_Prefer_Named_Last()
        {
            var inst = new StashboxContainer()
                .Register<ITest, Test11>(config => config.InNamedScope("A"))
                .Register<ITest, Test>()
                .Register<ITest, Test1>(config => config.InNamedScope("A"))
                .BeginScope("A")
                .Resolve<ITest>();

            Assert.IsInstanceOfType(inst, typeof(Test1));
        }

        [TestMethod]
        public void NamedScope_Simple_Resolve_Gets_Same_Within_Scope()
        {
            var scope = new StashboxContainer()
                .Register<ITest, Test>()
                .Register<ITest, Test1>(config => config.InNamedScope("A"))
                .BeginScope("A");

            var a = scope.Resolve<ITest>();
            var b = scope.Resolve<ITest>();

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void NamedScope_Simple_Resolve_Gets_Named_Within_Scope()
        {
            var scope = new StashboxContainer()
                .Register<ITest, Test11>(config => config.InNamedScope("A"))
                .Register<ITest, Test>(config => config.InNamedScope("A").WithName("T"))
                .Register<ITest, Test1>(config => config.InNamedScope("A"))
                .BeginScope("A");

            var a = scope.Resolve<ITest>("T");
            var b = scope.Resolve<ITest>("T");
            var c = scope.Resolve<ITest>();

            Assert.AreSame(a, b);
            Assert.AreNotSame(a, c);
            Assert.IsInstanceOfType(a, typeof(Test));
        }

        [TestMethod]
        public void NamedScope_Simple_Resolve_Gets_Named_When_Scoped_Doesnt_Exist()
        {
            var scope = new StashboxContainer()
                .Register<ITest, Test11>()
                .Register<ITest, Test>(config => config.WithName("T"))
                .Register<ITest, Test1>()
                .BeginScope("A");

            var a = scope.Resolve<ITest>("T");
            var b = scope.Resolve<ITest>("T");
            var c = scope.Resolve<ITest>();

            Assert.IsInstanceOfType(c, typeof(Test1));
            Assert.IsInstanceOfType(b, typeof(Test));
            Assert.IsInstanceOfType(a, typeof(Test));
        }

        [TestMethod]
        public void NamedScope_Dependency_Resolve_Wrapper_Gets_Same_Within_Scope()
        {
            var inst = new StashboxContainer()
                .Register<Test3>()
                .Register<ITest, Test>(config => config.InNamedScope("A"))
                .Register<ITest, Test1>()
                .BeginScope("A")
                .Resolve<Test3>();

            Assert.AreSame(inst.Func(), inst.Lazy.Value);
            Assert.AreSame(inst.Lazy.Value, inst.Enumerable.Last());
            Assert.AreSame(inst.Enumerable.Last(), inst.Tuple.Item1);
        }

        [TestMethod]
        public void NamedScope_Simple_Resolve_Get_Last_If_Scoped_Doesnt_Exist()
        {
            var inst = new StashboxContainer()
                .Register<ITest, Test>()
                .Register<ITest, Test1>()
                .BeginScope("A")
                .Resolve<ITest>();

            Assert.IsInstanceOfType(inst, typeof(Test1));
        }

        [TestMethod]
        public void NamedScope_Dependency_Get_Last_If_Scoped_Doesnt_Exist()
        {
            var inst = new StashboxContainer()
                .Register<Test3>()
                .Register<ITest, Test>()
                .Register<ITest, Test1>()
                .BeginScope("A")
                .Resolve<Test3>();

            Assert.IsInstanceOfType(inst.Func(), typeof(Test1));
            Assert.IsInstanceOfType(inst.Lazy.Value, typeof(Test1));
            Assert.IsInstanceOfType(inst.Enumerable.Last(), typeof(Test1));
            Assert.IsInstanceOfType(inst.Tuple.Item1, typeof(Test1));
        }

        [TestMethod]
        public void NamedScope_Defines_Scope_Prefer_Named()
        {
            var inst = new StashboxContainer()
                .Register<Test3>(config => config.DefinesScope("A"))
                .Register<ITest, Test11>()
                .Register<ITest, Test>(config => config.InNamedScope("A"))
                .Register<ITest, Test1>()
                .Resolve<Test3>();

            Assert.IsInstanceOfType(inst.Func(), typeof(Test));
            Assert.IsInstanceOfType(inst.Lazy.Value, typeof(Test));
            Assert.IsInstanceOfType(inst.Enumerable.Last(), typeof(Test));
            Assert.IsInstanceOfType(inst.Tuple.Item1, typeof(Test));

            Assert.AreSame(inst.Func(), inst.Lazy.Value);
            Assert.AreSame(inst.Lazy.Value, inst.Enumerable.Last());
            Assert.AreSame(inst.Enumerable.Last(), inst.Tuple.Item1);
        }

        [TestMethod]
        public void NamedScope_Preserve_Instance_Through_Nested_Scopes()
        {
            var container = new StashboxContainer()
                .Register<ITest, Test>(config => config.InNamedScope("A"));

            using (var s1 = container.BeginScope("A"))
            {
                var i1 = s1.Resolve<ITest>();
                using (var s2 = s1.BeginScope("C"))
                {
                    var i2 = s2.Resolve<ITest>();

                    Assert.AreSame(i2, i1);
                }
            }
        }

        [TestMethod]
        public void NamedScope_Dispose_Instance_Through_Nested_Scopes()
        {
            var container = new StashboxContainer()
                .Register<ITest, Test12>(config => config.InNamedScope("A"));

            ITest i1;
            using (var s1 = container.BeginScope("A"))
            {
                i1 = s1.Resolve<ITest>();
                using (var s2 = s1.BeginScope())
                {
                    var i2 = s2.Resolve<ITest>();

                    Assert.AreSame(i2, i1);
                }

                Assert.IsFalse(((Test12)i1).Disposed);
            }

            Assert.IsTrue(((Test12)i1).Disposed);
        }

        [TestMethod]
        public void NamedScope_Dispose_Instance_Defines_Named_Scope()
        {
            var container = new StashboxContainer()
                .Register<ITest, Test12>(config => config.InNamedScope("A"))
                .Register<Test2>(config => config.DefinesScope("A"));

            Test2 i1;
            using (var s1 = container.BeginScope())
            {
                i1 = s1.Resolve<Test2>();
            }

            Assert.IsTrue(((Test12)i1.Test).Disposed);
        }

        [TestMethod]
        public void NamedScope_Lifetime_Check()
        {
            var inst = new StashboxContainer()
                .Register<ITest, Test>(config => config.InNamedScope("A"))
                .ContainerContext.RegistrationRepository.GetRegistrationMappings().First(reg => reg.Key == typeof(ITest));

            Assert.IsInstanceOfType(inst.Value.RegistrationContext.Lifetime, typeof(NamedScopeLifetime));
        }

        [TestMethod]
        [ExpectedException(typeof(ResolutionFailedException))]
        public void NamedScope_Throws_ResolutionFailedException_Without_Scope()
        {
            var container = new StashboxContainer()
                .Register<ITest, Test>(config => config.InNamedScope("A"));

            container.Resolve<ITest>();
        }

        [TestMethod]
        public void NamedScope_WithNull()
        {
            var container = new StashboxContainer()
                .Register<Test2>(config => config.InNamedScope("A"));

            Assert.IsNull(container.BeginScope("A").Resolve<Test2>(nullResultAllowed: true));
        }

        [TestMethod]
        public void NamedScope_ChildContainer_Chain()
        {
            var container = new StashboxContainer()
                .Register<Test2>(config => config.DefinesScope("B").InNamedScope("A"));

            var child = container.CreateChildContainer()
                .Register<ITest, Test1>(config => config.InNamedScope("B"))
                .Register<Test4>(config => config.DefinesScope("A"));

            var inst = child.Resolve<Test4>();

            Assert.IsNotNull(inst.Test);
            Assert.IsNotNull(inst.Test.Test);
        }

        [TestMethod]
        public void NamedScope_ChildContainer()
        {
            var container = new StashboxContainer()
                .Register<Test2>(config => config.DefinesScope("A"))
                .Register<ITest, Test>(config => config.InNamedScope("A"));

            var child = container.CreateChildContainer()
                .Register<ITest, Test1>(config => config.InNamedScope("A"));

            var inst = child.Resolve<Test2>();

            Assert.IsInstanceOfType(inst.Test, typeof(Test1));
        }

        [TestMethod]
        public void NamedScope_Chain()
        {
            var container = new StashboxContainer()
                .Register<Test2>(config => config.DefinesScope("B").InNamedScope("A"))
                .Register<ITest, Test1>(config => config.InNamedScope("B"))
                .Register<Test4>(config => config.DefinesScope("A"));

            var inst = container.Resolve<Test4>();

            Assert.IsNotNull(inst.Test);
            Assert.IsNotNull(inst.Test.Test);
        }

        [TestMethod]
        public void NamedScope_ChildContainer_Chain_Reverse()
        {
            var container = new StashboxContainer()
                .Register<Test4>(config => config.DefinesScope("A"))
                .Register<ITest, Test1>(config => config.InNamedScope("B"));

            var child = container.CreateChildContainer()
                .Register<Test2>(config => config.DefinesScope("B").InNamedScope("A"));

            var inst = child.Resolve<Test4>();

            Assert.IsNotNull(inst.Test);
            Assert.IsNotNull(inst.Test.Test);
        }

        interface ITest
        { }

        class Test : ITest
        { }

        class Test1 : ITest
        { }

        class Test11 : ITest
        { }

        class Test12 : ITest, IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                if (this.Disposed)
                    throw new ObjectDisposedException("");

                this.Disposed = true;
            }
        }

        class Test2
        {
            public Test2(ITest test)
            {
                Test = test;
            }

            public ITest Test { get; }
        }

        class Test4
        {
            public Test4(Test2 test)
            {
                Test = test;
            }

            public Test2 Test { get; }
        }

        class Test3
        {
            public Test3(Func<ITest> func, Lazy<ITest> lazy, IEnumerable<ITest> enumerable, Tuple<ITest> tuple)
            {
                Func = func;
                Lazy = lazy;
                Enumerable = enumerable;
                Tuple = tuple;
            }

            public Func<ITest> Func { get; }
            public Lazy<ITest> Lazy { get; }
            public IEnumerable<ITest> Enumerable { get; }
            public Tuple<ITest> Tuple { get; }
        }
    }
}
