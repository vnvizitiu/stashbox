﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stashbox.Infrastructure;

namespace Stashbox.Tests
{
    [TestClass]
    public class ReMapTests
    {
        [TestMethod]
        public void ReMapTests_SingleResolve()
        {
            IStashboxContainer container = new StashboxContainer();
            container.RegisterType<ITest1, Test1>();

            var test1 = container.Resolve<ITest1>();

            Assert.IsNotNull(test1);
            Assert.IsInstanceOfType(test1, typeof(Test1));

            container.ReMap<ITest1, Test11>();

            var test11 = container.Resolve<ITest1>();

            Assert.IsNotNull(test11);
            Assert.IsInstanceOfType(test11, typeof(Test11));
        }

        [TestMethod]
        public void ReMapTests_DependencyResolve()
        {
            IStashboxContainer container = new StashboxContainer();
            container.RegisterType<ITest1, Test1>();
            container.RegisterType<ITest2, Test2>();

            var test2 = container.Resolve<ITest2>();

            Assert.IsNotNull(test2.Test1);
            Assert.IsInstanceOfType(test2.Test1, typeof(Test1));

            container.ReMap<ITest1, Test11>();

            var test22 = container.Resolve<ITest2>();

            Assert.IsNotNull(test22.Test1);
            Assert.IsInstanceOfType(test22.Test1, typeof(Test11));
        }

        public interface ITest1 { }

        public interface ITest2
        {
            ITest1 Test1 { get; }
        }

        public class Test1 : ITest1
        { }

        public class Test11 : ITest1
        { }

        public class Test2 : ITest2
        {
            public ITest1 Test1 { get; }

            public Test2(ITest1 test1)
            {
                this.Test1 = test1;
            }
        }
    }
}
