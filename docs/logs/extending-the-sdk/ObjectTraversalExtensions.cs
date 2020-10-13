// <copyright file="ObjectTraversalExtensions.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

internal static class ObjectTraversalExtensions
{
    // TODO: consider weak-ref or a cap to avoid unbounded memory
    private static ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();

    public static IEnumerable<KeyValuePair<string, object>> Travel(this object obj)
    {
        var type = obj.GetType();
        var name = type.AssemblyQualifiedName; // TODO: evaluate how expensive this is, and if there is a better identifier (e.g. mangled name)

        // TODO: access public fields

        // TODO: loop detection (probably not)

        PropertyInfo[] properties;
        if (cache.TryGetValue(name, out var tmp))
        {
            properties = (PropertyInfo[])tmp;
        }
        else
        {
            properties = type.GetProperties();
            cache[name] = properties; // TODO: revisit thread-safety
        }

        for (int i = 0; i < properties.Length; i++)
        {
            var propertyInfo = properties[i];
            yield return new KeyValuePair<string, object>(propertyInfo.Name, propertyInfo.GetValue(obj));
        }
    }
}
