using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CLMS.Framework.Service;
using CLMS.Framework.Utilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Builder = CLMS.Framework.LinqRuntimeTypeBuilder;

namespace CLMS.Framework.Test.LinqRuntimeTypeBuilder
{
    [TestClass]
    public class LinqRuntimeTypeBuilderTest
    {
        public LinqRuntimeTypeBuilderTest()
        {
            var services = new ServiceCollection();

            services.AddDistributedMemoryCache();

            var provider = services.BuildServiceProvider();
            
            services.AddSingleton<ICacheWrapperService>(new CacheWrapperService(provider.GetService<IDistributedCache>(), null));

            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
        }
        
        [TestMethod()]
        public void SanitizeCSharpIdentifierTest()
        {
            var id = Builder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("true");
            Assert.AreEqual(id, "True");

            id = Builder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("_test");
            Assert.AreEqual(id, "_Test");

            id = Builder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("if");
            Assert.AreEqual(id, "If");

            id = Builder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("Name");
            Assert.AreEqual(id, "Name");

            id = Builder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("1Name");
            Assert.AreEqual(id, "_1Name");
        }

        [TestMethod]
        public void CSharpNameTest()
        {
            var id = Builder.LinqRuntimeTypeBuilder.CSharpName(typeof(LinqRuntimeTypeBuilderTest));
            Assert.AreEqual(id, "LinqRuntimeTypeBuilderTest");

            id = Builder.LinqRuntimeTypeBuilder.CSharpName(typeof(LinqRuntimeTypeBuilderTest), false);
            Assert.AreEqual(id, "LinqRuntimeTypeBuilderTest");

            id = Builder.LinqRuntimeTypeBuilder.CSharpName(typeof(List<int>), false);
            Assert.AreEqual(id, "ListInt32");

            id = Builder.LinqRuntimeTypeBuilder.CSharpName(null, false);
            Assert.AreEqual(id, "");

            id = Builder.LinqRuntimeTypeBuilder.CSharpName(typeof(List<int>), true);
            Assert.AreEqual(id, "List<Int32>");
        }

        [TestMethod]
        public void FieldDefinitionTest()
        {
            Expression<System.Func<int, object>> p = (a) => null;

            var test = new Builder.FieldDefinition<int>
            {
                Name = "Name",
                Type = typeof(string),
                Selector = p
            };

            Assert.AreEqual("Name", test.Name);
            Assert.AreEqual(typeof(string), test.Type);
            Assert.AreEqual(p, test.Selector);
        }

        [TestMethod]
        public void ParameterReplaceVisitorTest()
        {
            var p1 = Expression.Parameter(typeof(int));

            var param = new Builder.ParameterReplaceVisitor(p1, p1);

            Expression<System.Func<int, object>> p = (a) => null;

            param.Visit(p);
            param.Visit(p1);

            Assert.AreNotEqual(param, null);
        }

        [TestMethod]
        public void GetDynamicTypesTest()
        {
            var groupField = new Dictionary<string, Type>() {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            var selectField = new Dictionary<string, Type>() {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            var p1 = Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetDynamicTypesThrow1Test()
        {
            var selectField = new Dictionary<string, Type>() {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            var groupField = new Dictionary<string, Type>()
            {
            };

            Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetDynamicTypesThrow2Test()
        {
            var groupField = new Dictionary<string, Type>() {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            var selectField = new Dictionary<string, Type>()
            {
            };

            Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicTypesThrow3Test()
        {
            var selectField = new Dictionary<string, Type>() {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            Dictionary<string, Type> groupField = null;

            Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicTypesThrow4Test()
        {
            var groupField = new Dictionary<string, Type>() {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            Dictionary<string, Type> selectField = null;

            Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));
        }
    }
}
