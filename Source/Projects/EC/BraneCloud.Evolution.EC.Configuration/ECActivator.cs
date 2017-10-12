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
using System.Collections.Generic;
using System.Reflection;

namespace BraneCloud.Evolution.EC.Configuration
{
    /// <summary>
    /// Wraps around the type name mapping for EC to enable activation using System.Activator
    /// </summary>
    public static class ECActivator
    {
        private static readonly List<Assembly> Assemblies = new List<Assembly>();
        private static readonly IDictionary<string, Type> CanonicalNameMap = new Dictionary<string, Type>();
        private static readonly IDictionary<string, Type> FullNameMap = new Dictionary<string, Type>();

        /// <summary>
        /// Register assembly types if they have the ECConfigurationAttribute.
        /// NOTE: Once an exact (fully qualified type is discovered that has already been registered we should probably
        /// just abort immediately (because ALL types will have been registered).
        /// TODO: Test this to make sure there are not any subtle ways this could be foiled.
        /// </summary>
        /// <param name="assembly"></param>
        public static void AddSourceAssembly(Assembly assembly)
        {
            foreach (var t in assembly.GetTypes())
            {
                var attributes = t.GetCustomAttributes(typeof (ECConfigurationAttribute), false) as ECConfigurationAttribute[];
                if (attributes != null && attributes.Length > 0)
                {
                    foreach (var a in attributes)
                    {
                        if (a != null)
                        {
                            var canonicalName = a.CanonicalName;
                            if (!String.IsNullOrEmpty(canonicalName))
                            {
                                if (CanonicalNameMap.ContainsKey(canonicalName))
                                {
                                    // If we're trying to remap the exact same type then we can just skip it...
                                    // NOTE: We could probably just return at this point (since we're using the fullname)
                                    //       because the entire assembly has probably already been mapped.
                                    if (CanonicalNameMap[canonicalName].FullName == t.FullName) continue;
                                    // Otherwise, we have a real conflict!
                                    throw new InvalidOperationException(
                                        String.Format(
                                            "ECActivator: The canonical name '{0}' is already mapped to '{1}'",
                                            canonicalName, t.FullName));
                                }
                                // The type hasn't been registered yet...
                                CanonicalNameMap.Add(canonicalName, t);
                            }
                        }
                    }
                }
                if (!String.IsNullOrEmpty(t.FullName) )
                {
                    if (FullNameMap.ContainsKey(t.FullName))
                    {
                        if (t != FullNameMap[t.FullName])
                            throw new InvalidOperationException(
                                String.Format("ECActivator: The typename '{0}' is already mapped!", t.FullName));
                        continue;
                    }
                    FullNameMap.Add(t.FullName, t);
                }
            }
        }

        public static void AddSourceAssemblies(IEnumerable<Assembly> assemblies)
        {
            Assemblies.AddRange(assemblies);
            foreach (var asm in assemblies)
            {
                AddSourceAssembly(asm);
            }
        }

        public static Type GetType(string name)
        {
            if (Assemblies == null || Assemblies.Count < 1)
                throw new InvalidOperationException("ECActivator: No assemblies have been added. Cannot resolve types.");
            Type type = null;
            CanonicalNameMap.TryGetValue(name, out type);
            if (type != null)
                return type;

            // we can try to get a type by its full name, even if it doesn't have the ECConfigurationAttribute.CononicalName
            FullNameMap.TryGetValue(name, out type);

            return type;
        }

        /// <summary>
        /// This simply defers to <see cref="GetType(string)"/>.
        /// </summary>
        /// <param name="name">The name of the type to search.</param>
        /// <param name="type">The <see cref="Type"/> with the specified name if one is found, otherwise <b>null</b>.</param>
        /// <returns>A boolean value of <b>true</b> if the type is found, otherwise <b>false</b>.</returns>
        public static bool TryGetType(string name, out Type type)
        {
            type = GetType(name);
            if (type != null)
                return true;
            return false;
        }

        /// <summary>
        /// This will try to locate and create the type specified by <paramref name="typeName"/>.
        /// The client is responsible for casting this to the expected type.
        /// </summary>
        public static object CreateInstance(string typeName)
        {
            Type type = null;
            return TryGetType(typeName, out type) ? Activator.CreateInstance(type) : null;
        }
        public static object CreateInstance(string assemblyName, string typeName)
        {
            throw new NotImplementedException("This method has not been implemented.");
        }
    }
}