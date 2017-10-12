/*
 * BraneCloud.Evolution.EC (Evolutionary Computation)
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 *
 * This is an independent conversion from Java to .NET of ...
 *
 * Sean Luke's ECJ project at GMU 
 * (Academic Free License v3.0): 
 * http://www.cs.gmu.edu/~eclab/projects/ecj
 *
 * Radical alteration was required throughout (including structural).
 * The author of ECJ cannot and MUST not be expected to support this fork.
 *
 * If you wish to create yet another fork, please use a different root namespace.
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Configuration.Tests
{
    /// <summary>
    /// These tests are designed to ensure that all type names that might be used
    /// in parameter files (*.param files) can be mapped to valid model types.
    /// These tests MUST be maintained as types and namespaces are added, moved, or renamed!!! 
    /// </summary>
    [TestClass]
    public class TypeNameMapping
    {
        #region Housekeeping

        public TypeNameMapping()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext context { get { return testContextInstance; } }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #endregion // Housekeeping

        [TestMethod]
        public void SearchForAllParameterConstants()
        {
            var sb = new StringBuilder(); // To hold output before writing it somewhere

            var asm = Assembly.GetAssembly(typeof (ECConfigurationAttribute));
            Assert.IsNotNull(asm);
            var types = asm.GetExportedTypes();
            var namespaces = new Dictionary<string, IDictionary<string, IDictionary<string, string>>>();
            foreach (var t in types)
            {
                var dict = new Dictionary<string, string>();
                var foundParam = false;
                var fields = t.GetFields();
                foreach (var f in fields)
                {
                    if (f.IsLiteral && f.Name.StartsWith("P_"))
                    {
                        foundParam = true;
                        dict.Add(f.Name, f.GetRawConstantValue().ToString());
                    }
                }
                if (foundParam)
                {
                    if (String.IsNullOrEmpty(t.Namespace))
                        throw new ApplicationException("Namespace was null or empty!");
                    if (!namespaces.ContainsKey(t.Namespace))
                    {
                        namespaces.Add(t.Namespace, new Dictionary<string, IDictionary<string, string>>());
                    }
                    namespaces[t.Namespace].Add(t.Name, dict);
                }
            }
            //writer.WriteLine("Namespaces: {0}", namespaces.Count);
            foreach (var ns in namespaces.Keys)
            {
                var typeDict = namespaces[ns];
                var typeKeys = typeDict.Keys.ToArray();
                Array.Sort(typeKeys);
                foreach (var typeName in typeKeys)
                {
                    var parmDict = typeDict[typeName];
                    var parmKeys = parmDict.Keys.ToArray();
                    Array.Sort(parmKeys);
                    foreach (var p in parmKeys)
                    {
                        sb = sb.AppendFormat("{0, 50}{1, 50}{2, 50}{3, 50}\n", "", "", p, parmDict[p]);
                    }
                    sb = sb.AppendFormat("{0, 50}{1, 50}\n", "", typeName);
                }
                sb = sb.AppendFormat("{0, 50}\n", ns);
            }
            using (var writer = new StreamWriter("BraneCloud.Evolution.C_ParametersByNamespaceAndType.txt"))
            {
                writer.WriteLine(sb.ToString());
            }
            //context.WriteLine(sb.ToString()); // for debugging or preview (won't format properly at all, but what the heck!)
        }

        [TestMethod]
        public void WhichTypesAreMarkedWithTheECConfigurationAttributeAndWhichAreNot()
        {
            var asm = Assembly.GetAssembly(typeof (ECConfigurationAttribute));
            Assert.IsNotNull(asm);
            TestContext.WriteLine("EC model types in assembly: {0}", asm.FullName);
            var types = asm.GetExportedTypes();
            Assert.IsNotNull(types);
            TestContext.WriteLine("Number of exported types: {0}", types.Length);

            var cfgTypes = from t in types
                           let a =
                               t.GetCustomAttributes(typeof (ECConfigurationAttribute), false)
                               .FirstOrDefault() as ECConfigurationAttribute
                           orderby t.Name
                           select new {t.Name, CanonicalName = (a == null ? "" : a.CanonicalName)};

            var list = cfgTypes.ToList();

            var sepLine = "***************************************";
            var sbc = new StringBuilder(sepLine);
            sbc.AppendLine();
            sbc.AppendLine("Configured Types:");
            sbc.AppendLine(sepLine);

            // Configured Types
            foreach (var t in list.Where(c => !String.IsNullOrEmpty(c.CanonicalName)))
            {
                sbc.AppendFormat("{0, -50} [{1}]", t.Name, t.CanonicalName);
                sbc.AppendLine();
            }
            sbc.AppendLine();

            var sbn = new StringBuilder(sepLine);
            sbn.AppendLine();
            sbn.AppendLine("Non-Configured Types:");
            sbn.AppendLine(sepLine);
            foreach (var t in list.Where(c => String.IsNullOrEmpty(c.CanonicalName)))
            {
                sbn.AppendFormat("{0}", t.Name);
                sbn.AppendLine();
            }
            TestContext.WriteLine(sbc.ToString());
            TestContext.WriteLine(sbn.ToString());
        }

        [TestMethod]
        public void WhatTypesExistInTheModelAssemblyThatAreNotMapped()
        {
            var assembly = typeof(ECTypeMap).Assembly;
            var types = assembly.GetTypes();
            var mappedTypes = ECTypeMap.Values;
            var typesToRemove = new List<Type>();
            TestContext.WriteLine("Number of types found in assembly [{0}] = {1}", assembly.FullName, types.Length.ToString());
            foreach (var type in types)
            {
                if (mappedTypes.Contains(type) || type.FullName.Contains(ECNamespace.Util))
                    typesToRemove.Add(type);
                
                TestContext.WriteLine(type.FullName);
            }
            var typesRemaining = new List<Type>(types);
            foreach (var type in typesToRemove)
            {
                typesRemaining.Remove(type);
            }
            TestContext.WriteLine("");
            TestContext.WriteLine("Types not mapped in EC namespace:");
            foreach (var type in typesRemaining)
            {
                TestContext.WriteLine(type.FullName);
            }
        }


        /// <summary>
        /// This test attempts to retrieve the types found in namespace "BraneCloud.Evolution.EC".
        /// After the test is run, check the "Test Details" to view all types that were discovered.
        /// ECModelTypeMap     - maps names found in "param" files to ACTUAL TYPES.
        /// ECModelTypeNameMap - maps names found in "param" files to ACTUAL TYPE NAMES.
        /// </summary>
        [TestMethod]
        public void ECTypeMapGetsAllMappedTypesFromModelAssembly()
        {
            // For all keys...
            // Take name from     ECModelTypeMap     (in the Model namespace)
            // Get the type from  ECModelTypeMap
            // Get the type from  ECModelTypeNameMap (in the Configuration namespace).
            // Compare the two for equality...
            foreach (var name in ECTypeMap.Keys)
            {
                string typeName;
                var found = ECTypeNameMap.TryGetTypeName(name, out typeName);
                if (!found)
                {
                    TestContext.WriteLine("TypeName for '{0}' could not be mapped to a type name.");
                }

                // we have to use the assembly qualified name here because we are outside the model assembly
                typeName += ", BraneCloud.Evolution.EC"; // Assembly qualified name required.
                var typeFromString = Type.GetType(typeName);
                if (typeFromString != null)
                {
                    TestContext.WriteLine("TypeFromString.FullName = {0}", typeFromString.FullName);
                }
                else
                {
                    TestContext.WriteLine("ERROR! Could not get type = {0}", typeName);
                }
                //Assert.IsNotNull(typeFromString);
                //var typeFromMap = BraneCloud.Evolution.EC.ECModelTypeMap.TryGetType(name);
                //if (typeFromString == null)
                // {
                //    //Assert.IsNotNull(typeFromMap);
                //    TestContext.WriteLine("TypeFromMap.FullName    = {0}", typeFromMap.FullName);
                //    Assert.AreEqual(typeFromMap, typeFromString);
                //}
            }
        }

        /// <summary>
        /// This test attempts to retrieve the types found in namespace "BraneCloud.Evolution.EC".
        /// After the test is run, check the "Test Details" to view all types that were discovered.
        /// ECModelTypeMap     - maps names found in "param" files to ACTUAL TYPES.
        /// ECModelTypeNameMap - maps names found in "param" files to ACTUAL TYPE NAMES.
        /// </summary>
        [TestMethod]
        public void ECTypeMapGetsAllMappedTypesFromKey()
        {
            // For all keys...
            // Take name from     ECModelTypeMap     (in the Model namespace)
            // Get the type from  ECModelTypeMap
            // Get the type from  ECModelTypeNameMap (in the Configuration namespace).
            // Compare the two for equality...

            var failed = false;

            foreach (var name in ECTypeMap.Keys)
            {
                string typeName;
                var found = ECTypeNameMap.TryGetTypeName(name, out typeName);
                if (!found)
                {
                    failed = true;
                    TestContext.WriteLine("TypeName for '{0}' could not be mapped to a type name.");
                    continue;
                }

                // we have to use the assembly qualified name here because we are outside the model assembly
                typeName += ", BraneCloud.Evolution.EC"; // Assembly qualified name required.
                Type typeFromMap; 
                found = ECTypeMap.TryGetType(name, out typeFromMap);
                if (!found)
                {
                    failed = true;
                    TestContext.WriteLine("Type for '{0}' could not be obtained using ECTypeMap.TryGetType().");
                    continue;
                }
                var typeFromString = Type.GetType(typeName);
                if (typeFromString != null)
                {
                    TestContext.WriteLine("TypeFromString.FullName = {0}", typeFromString.FullName);
                }
                else
                {
                    failed = true;
                    TestContext.WriteLine("ERROR! Could not get type = {0}", typeName);
                }
                Assert.IsFalse(failed);
            }
        }

    }
}