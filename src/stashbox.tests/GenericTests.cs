﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Stashbox.Tests
{
    [TestClass]
    public class GenericTests
    {
        [TestMethod]
        public void GenericTests_Resolve()
        {
            var container = new StashboxContainer();

            container.RegisterType(typeof(ITest1<,>), typeof(Test1<,>));
            var inst = container.Resolve<ITest1<int, string>>();

            Assert.IsNotNull(inst);
            Assert.IsInstanceOfType(inst, typeof(Test1<int, string>));
        }

        public interface ITest1<I, K>
        {
            I IProp { get; }
            K KProp { get; }
        }

        public class Test1<I, K> : ITest1<I, K>
        {
            public I IProp { get; }
            public K KProp { get; }
        }
    }
}