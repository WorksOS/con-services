using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Utilities.Interfaces;
using System.Linq;
using VSS.VisionLink.Raptor.Utilities;
using System.Collections.Generic;
using System.Reflection;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Utility.Tests
{
    public class TestClassBase : MemoryStream
    {
        public TestClassBase()
        {
        }
    }

    public class TestClass : TestClassBase
    {
        public TestClass()
        {
        }
    }

    [TestClass]
    public class TypesHelperTests
    {
        /// <summary> 
        /// Note: Only descendent classes within the assembly defining the base class will be included in the result
        /// </summary>
        [TestMethod]
        public void Test_TypesHelper_FindAllDerivedTypes()
        {
            List<Type> types = TypesHelper.FindAllDerivedTypes<TestClassBase>();

            Assert.IsTrue(types.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass not noted as a descendent of MemoryStream");

            List<Type> types2 = TypesHelper.FindAllDerivedTypes<List<Object>>();

            Assert.IsFalse(types2.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass incorrectly noted as a descendent of List<>");
        }

        /// <summary> 
        /// Note: Only descendent classes within the assembly defining the base class will be included in the result
        /// </summary>
        [TestMethod]
        public void Test_TypesHelper_FindAllDerivedTypes_Fail()
        {
            List<Type> types = TypesHelper.FindAllDerivedTypes<MemoryStream>();

            Assert.IsFalse(types.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass noted as a descendent of MemoryStream (defined in different assembly)");
        }

        [TestMethod]
        public void Test_TypesHelper_FindAllDerivedTypesInThisAssembly()
        {
            List<Type> types = TypesHelper.FindAllDerivedTypes<MemoryStream>(Assembly.GetExecutingAssembly());

            Assert.IsTrue(types.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass not noted as a descendent of MemoryStream");

            List<Type> types2 = TypesHelper.FindAllDerivedTypes<List<Object>>(Assembly.GetExecutingAssembly());

            Assert.IsFalse(types2.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass incorrectly noted as a descendent of List<>");
        }
    }
}