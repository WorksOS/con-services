using System;
using System.IO;
using VSS.TRex.Common.Utilities.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using VSS.TRex.Common.Utilities;
using Xunit;

namespace VSS.TRex.Tests.Utility
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

        public class TypesHelperTests
    {
        /// <summary> 
        /// Note: Only descendent classes within the assembly defining the base class will be included in the result
        /// </summary>
        [Fact]
        public void Test_TypesHelper_FindAllDerivedTypes()
        {
            List<Type> types = TypesHelper.FindAllDerivedTypes<TestClassBase>();

            Assert.True(types.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass not noted as a descendent of MemoryStream");

            List<Type> types2 = TypesHelper.FindAllDerivedTypes<List<object>>();

            Assert.False(types2.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass incorrectly noted as a descendent of List<>");
        }

        /// <summary> 
        /// Note: Only descendent classes within the assembly defining the base class will be included in the result
        /// </summary>
        [Fact]
        public void Test_TypesHelper_FindAllDerivedTypes_Fail()
        {
            List<Type> types = TypesHelper.FindAllDerivedTypes<MemoryStream>();

            Assert.False(types.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass noted as a descendent of MemoryStream (defined in different assembly)");
        }

        [Fact]
        public void Test_TypesHelper_FindAllDerivedTypesInThisAssembly()
        {
            List<Type> types = TypesHelper.FindAllDerivedTypes<MemoryStream>(Assembly.GetExecutingAssembly());

            Assert.True(types.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass not noted as a descendent of MemoryStream");

            List<Type> types2 = TypesHelper.FindAllDerivedTypes<List<object>>(Assembly.GetExecutingAssembly());

            Assert.False(types2.Any(x => x.IsAssignableFrom(typeof(TestClass))), "testClass incorrectly noted as a descendent of List<>");
        }
    }
}
