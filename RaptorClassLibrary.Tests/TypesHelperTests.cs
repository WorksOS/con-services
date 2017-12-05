﻿using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Utilities.Interfaces;
using System.Linq;
using VSS.VisionLink.Raptor.Utilities;
using System.Collections.Generic;
using System.Reflection;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    public class testClassBase : MemoryStream
    {
        public testClassBase()
        {
        }
    }

    public class testClass : testClassBase
    {
        public testClass()
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
            List<Type> types = TypesHelper.FindAllDerivedTypes<testClassBase>();

            Assert.IsTrue(types.Any(x => x.IsAssignableFrom(typeof(testClass))), "testClass not noted as a descendent of MemoryStream");

            List<Type> types2 = TypesHelper.FindAllDerivedTypes<List<Object>>();

            Assert.IsFalse(types2.Any(x => x.IsAssignableFrom(typeof(testClass))), "testClass incorrectly noted as a descendent of List<>");
        }

        /// <summary> 
        /// Note: Only descendent classes within the assembly defining the base class will be included in the result
        /// </summary>
        [TestMethod]
        public void Test_TypesHelper_FindAllDerivedTypes_Fail()
        {
            List<Type> types = TypesHelper.FindAllDerivedTypes<MemoryStream>();

            Assert.IsFalse(types.Any(x => x.IsAssignableFrom(typeof(testClass))), "testClass noted as a descendent of MemoryStream (defined in different assembly)");
        }

        [TestMethod]
        public void Test_TypesHelper_FindAllDerivedTypesInThisAssembly()
        {
            List<Type> types = TypesHelper.FindAllDerivedTypes<MemoryStream>(Assembly.GetExecutingAssembly());

            Assert.IsTrue(types.Any(x => x.IsAssignableFrom(typeof(testClass))), "testClass not noted as a descendent of MemoryStream");

            List<Type> types2 = TypesHelper.FindAllDerivedTypes<List<Object>>(Assembly.GetExecutingAssembly());

            Assert.IsFalse(types2.Any(x => x.IsAssignableFrom(typeof(testClass))), "testClass incorrectly noted as a descendent of List<>");
        }
    }
}