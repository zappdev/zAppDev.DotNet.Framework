using CLMS.Framework.LinqRuntimeTypeBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CLMS.Framework.Tests.Data
{
    [TestClass]
    public class LinqRuntimeTypeBuilderTest
    {
        [TestMethod()]
        public void SanitizeCSharpIdentifierTest()
        {
            var id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("true");
            Assert.AreEqual(id, "True");

            id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("_test");
            Assert.AreEqual(id, "_Test");

            id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("if");
            Assert.AreEqual(id, "If");

            id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("Name");
            Assert.AreEqual(id, "Name");

            id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("1Name");
            Assert.AreEqual(id, "_1Name");
        }

        [TestMethod]
        public void CSharpNameTest()
        {
            var id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.CSharpName(typeof(LinqRuntimeTypeBuilderTest));
            Assert.AreEqual(id, "LinqRuntimeTypeBuilderTest");

            id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.CSharpName(typeof(LinqRuntimeTypeBuilderTest), false);
            Assert.AreEqual(id, "LinqRuntimeTypeBuilderTest");

            id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.CSharpName(typeof(List<int>), false);
            Assert.AreEqual(id, "ListInt32");

            id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.CSharpName(null, false);
            Assert.AreEqual(id, "");

            id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.CSharpName(typeof(List<int>), true);
            Assert.AreEqual(id, "List<Int32>");
        }

        [TestMethod]
        public void FieldDefinitionTest()
        {
            Expression<System.Func<int, object>> p = (a) => null;

            var test = new FieldDefinition<int>
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

            var param = new ParameterReplaceVisitor(p1, p1);

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

            var p1 = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));

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

            LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));
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

            LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));
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

            LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));
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

            LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.GetDynamicTypes(groupField, selectField, typeof(bool));
        }
    }
}
