using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Builder = CLMS.Framework.LinqRuntimeTypeBuilder;

#if NETFRAMEWORK
#else
using CLMS.Framework.Services;
using CLMS.Framework.Utilities;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace CLMS.Framework.Tests.LinqRuntimeTypeBuilder
{
    [TestClass]
    public class LinqRuntimeTypeBuilderTest
    {        
        [TestInitialize]
        public void Initialize() 
        {
#if NETFRAMEWORK
#else
            var mock = new Mock<ICacheWrapperService>();

            mock.Setup(foo => foo.Contains("BuiltTypes")).Returns(true);

            var types = new Dictionary<string, Type>();

            mock.Setup(foo => foo.Get<Dictionary<string, Type>>("BuiltTypes")).Returns(types);

            var services = new ServiceCollection();
            services.AddSingleton(ins => mock.Object);

            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
#endif
        }

        [TestCleanup]
        public void Cleanup() 
        {
#if NETFRAMEWORK
#else
            ServiceLocator.SetLocatorProvider(null);
#endif
        }

        [TestMethod]
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
            Expression<Func<int, object>> p = (a) => null;

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

            Expression<Func<int, object>> p = (a) => null;

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
        
            p1 = Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));

            selectField = new Dictionary<string, Type>() {
                { "linqVar", typeof(LinqRuntimeTypeBuilderTest)},
                { "stringVar", typeof(string)}
            };

            Assert.ThrowsException<ApplicationException>(() => Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetDynamicTypesThrow1Test()
        {
            var selectField = new Dictionary<string, Type>() {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            var groupField = new Dictionary<string, Type>();

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

            var selectField = new Dictionary<string, Type>();

            Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicTypesThrow3Test()
        {
            var selectField = new Dictionary<string, Type> {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(null, selectField, typeof(bool));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicTypesThrow4Test()
        {
            var groupField = new Dictionary<string, Type> {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            Builder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, null, typeof(bool));
        }

        [TestMethod]
        public void GetDynamicTypeTest()
        {
            var field = new Dictionary<string, Type> {
                { "intVar", typeof(int)},
                { "stringVar", typeof(string)}
            };

            var p1 = Builder.LinqRuntimeTypeBuilder.GetDynamicType(field, typeof(bool));            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicTypeThrow1Test()
        {
            var p1 = Builder.LinqRuntimeTypeBuilder.GetDynamicType(null, typeof(bool));            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetDynamicTypeThrow2Test()
        {
            var field = new Dictionary<string, Type>();
            var p1 = Builder.LinqRuntimeTypeBuilder.GetDynamicType(field, typeof(bool));            
        }

        [TestMethod]
        public void CreateGroupByAndSelectExpressionsTest() 
        {
            Expression<Func<int, object>> p = (a) => null;

            var groupField = new List<Builder.FieldDefinition<int>>() {
                new Builder.FieldDefinition<int>
                {
                    Name = "Key",
                    Type = typeof(string),
                    Selector = p
                }
            };

            var selectField = new List<Builder.FieldDefinition<IGrouping<object, int>>>() {
                new Builder.FieldDefinition<IGrouping<object, int>>
                {
                    Name = "Key",
                    Type = typeof(string),
                    Selector = groups => groups
                },
                new Builder.FieldDefinition<IGrouping<object, int>>
                {
                    Name = "Name",
                    Type = typeof(string),
                    Selector = groups => groups
                }
            };
            
            var exp = Builder.LinqRuntimeTypeBuilder
                .CreateGroupByAndSelectExpressions(groupField, selectField, out var groupAnonType, out var selectorAnonType);
        }

        [TestMethod]
        public void CombineSelectorsToNewObjectTest()
        {
            Expression<Func<int, object>> p = (a) => null;
            
            var groupField = new List<Builder.FieldDefinition<int>>() {
                new Builder.FieldDefinition<int>
                {
                    Name = "Key",
                    Type = typeof(string),
                    Selector = p
                }
            };
            
            var exp = Builder.LinqRuntimeTypeBuilder
                .CombineSelectorsToNewObject(groupField, out var type);
        }

    }
}