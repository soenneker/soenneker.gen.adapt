using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class ReflectionEmitter
{
    public static void EmitReflectionAdapter(StringBuilder sb, string targetNamespace)
    {
        sb.AppendLine("#nullable enable");
        sb.Append("namespace ").AppendLine(targetNamespace);
        sb.AppendLine("{");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Collections.Concurrent;");
        sb.AppendLine("using System.Collections.ObjectModel;");
        sb.AppendLine("using System.Reflection;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("public static partial class GenAdapt");
        sb.AppendLine("{");
        sb.AppendLine("""
            private static class __RuntimeMappingCache
            {
                internal static readonly ConcurrentDictionary<(Type Source, Type Destination), Func<object, object>> Mappers = new();
            }

            /// <summary>Maps a runtime source type, preferring any available generated mapping and otherwise using a cached reflection mapper.</summary>
            public static TDest AdaptViaReflection<TDest>(this object source)
            {
                ArgumentNullException.ThrowIfNull(source);
                return (TDest)__AdaptRuntime(source, typeof(TDest), true);
            }

            /// <summary>Maps a source whose concrete type is unavailable at compile time.</summary>
            [Obsolete("No compile-time mapping was available for this call. The runtime fallback may use reflection.", DiagnosticId = "SGA005")]
            public static TDest Adapt<TDest>(this object source) => AdaptViaReflection<TDest>(source);

            private static object __AdaptRuntime(object source, Type destination, bool copy = false)
            {
                destination = Nullable.GetUnderlyingType(destination) ?? destination;
                if (!copy && destination.IsInstanceOfType(source)) return source;
                return __RuntimeMappingCache.Mappers.GetOrAdd((source.GetType(), destination), static pair => __BuildRuntimeMapper(pair.Source, pair.Destination))(source);
            }

            // Removed by the compiler when there are no generated pairs to dispatch.
            static partial void __TryGetGeneratedMapper(Type source, Type destination, ref Func<object, object>? mapper);

            private static Func<object, object> __BuildRuntimeMapper(Type source, Type destination)
            {
                Func<object, object>? generated = null;
                __TryGetGeneratedMapper(source, destination, ref generated);
                if (generated is not null) return generated;
                if (destination == typeof(string)) return static value => value.ToString()!;
                if (destination.IsEnum)
                    return source == typeof(string) ? value => Enum.Parse(destination, (string)value) : value => Enum.ToObject(destination, value);
                if (typeof(IConvertible).IsAssignableFrom(source) && typeof(IConvertible).IsAssignableFrom(destination))
                    return value => Convert.ChangeType(value, destination, System.Globalization.CultureInfo.InvariantCulture);

                Type? sourceDictionary = __FindGenericInterface(source, typeof(IDictionary<,>)) ?? __FindGenericInterface(source, typeof(IReadOnlyDictionary<,>));
                Type? destinationDictionary = __FindGenericInterface(destination, typeof(IDictionary<,>)) ?? __FindGenericInterface(destination, typeof(IReadOnlyDictionary<,>));
                if (sourceDictionary is not null && destinationDictionary is not null)
                {
                    Type[] sourceArguments = sourceDictionary.GetGenericArguments();
                    Type[] destinationArguments = destinationDictionary.GetGenericArguments();
                    return (Func<object, object>)typeof(GenAdapt).GetMethod(nameof(__CreateDictionaryMapper), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(sourceArguments[0], sourceArguments[1], destinationArguments[0], destinationArguments[1]).Invoke(null, new object[] { destination })!;
                }

                if (source != typeof(string) && typeof(IEnumerable).IsAssignableFrom(source))
                {
                    Type? element = destination.IsArray ? destination.GetElementType() : __FindGenericInterface(destination, typeof(IEnumerable<>))?.GetGenericArguments()[0];
                    if (element is not null)
                        return (Func<object, object>)typeof(GenAdapt).GetMethod(nameof(__CreateCollectionMapper), BindingFlags.NonPublic | BindingFlags.Static)!
                            .MakeGenericMethod(element).Invoke(null, new object[] { destination })!;
                }

                var assignments = new List<(PropertyInfo Source, PropertyInfo Destination, bool Direct, bool Nullable)>();
                foreach (PropertyInfo property in destination.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.SetMethod?.IsPublic != true || property.GetIndexParameters().Length != 0) continue;
                    PropertyInfo? input = source.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (input?.GetMethod?.IsPublic != true || input.GetIndexParameters().Length != 0) continue;
                    bool direct = property.PropertyType.IsAssignableFrom(input.PropertyType) ||
                        Nullable.GetUnderlyingType(property.PropertyType) == input.PropertyType ||
                        Nullable.GetUnderlyingType(input.PropertyType) == property.PropertyType;
                    if (!direct && !__CanMapRuntime(input.PropertyType, property.PropertyType)) continue;
                    bool nullable = !property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) is not null;
                    assignments.Add((input, property, direct, nullable));
                }
                var plan = assignments.ToArray();
                return value =>
                {
                    // Keep one box for a struct destination until every setter has run.
                    object target = Activator.CreateInstance(destination)!;
                    foreach (var assignment in plan)
                    {
                        object? propertyValue = assignment.Source.GetValue(value);
                        if (propertyValue is null)
                        {
                            if (assignment.Nullable)
                                assignment.Destination.SetValue(target, null);
                            continue;
                        }
                        assignment.Destination.SetValue(target, assignment.Direct ? propertyValue : __AdaptRuntime(propertyValue, assignment.Destination.PropertyType));
                    }
                    return target;
                };
            }

            private static bool __CanMapRuntime(Type source, Type destination)
            {
                source = Nullable.GetUnderlyingType(source) ?? source;
                destination = Nullable.GetUnderlyingType(destination) ?? destination;
                if (source == destination || destination.IsAssignableFrom(source)) return true;
                if (typeof(IEnumerable).IsAssignableFrom(source) && typeof(IEnumerable).IsAssignableFrom(destination) && source != typeof(string)) return true;
                return source != typeof(string) && destination != typeof(string) && !source.IsPrimitive && !destination.IsPrimitive &&
                    !source.IsEnum && !destination.IsEnum;
            }

            private static Type? __FindGenericInterface(Type type, Type definition)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == definition) return type;
                foreach (Type candidate in type.GetInterfaces())
                    if (candidate.IsGenericType && candidate.GetGenericTypeDefinition() == definition) return candidate;
                return null;
            }

            private static Func<object, object> __CreateCollectionMapper<TElement>(Type destination)
            {
                // Resolve the shape once. Element loops use typed storage and never
                // invoke MakeGenericMethod or MethodInfo.Invoke.
                Func<List<TElement>, object> finish;
                if (destination == typeof(TElement[])) finish = static list => list.ToArray();
                else if (destination.IsAssignableFrom(typeof(List<TElement>))) finish = static list => list;
                else if (destination == typeof(HashSet<TElement>) || destination == typeof(ISet<TElement>) || destination == typeof(IReadOnlySet<TElement>)) finish = static list => new HashSet<TElement>(list);
                else if (destination == typeof(SortedSet<TElement>)) finish = static list => new SortedSet<TElement>(list);
                else if (destination == typeof(Queue<TElement>)) finish = static list => new Queue<TElement>(list);
                else if (destination == typeof(Stack<TElement>)) finish = static list => new Stack<TElement>(list);
                else if (destination == typeof(LinkedList<TElement>)) finish = static list => new LinkedList<TElement>(list);
                else if (destination == typeof(Collection<TElement>)) finish = static list => new Collection<TElement>(list);
                else if (destination == typeof(ObservableCollection<TElement>)) finish = static list => new ObservableCollection<TElement>(list);
                else finish = list => Activator.CreateInstance(destination, list)!;
                return value =>
                {
                    var values = (IEnumerable)value;
                    int count = value is ICollection collection ? collection.Count : 0;
                    if (destination == typeof(TElement[]) && value is ICollection)
                    {
                        var array = count == 0 ? Array.Empty<TElement>() : new TElement[count];
                        int index = 0;
                        foreach (object? item in values)
                            array[index++] = item is null ? default! : (TElement)__AdaptRuntime(item, typeof(TElement));
                        return array;
                    }
                    var result = new List<TElement>(count);
                    foreach (object? item in values)
                        result.Add(item is null ? default! : (TElement)__AdaptRuntime(item, typeof(TElement)));
                    return finish(result);
                };
            }

            private static Func<object, object> __CreateDictionaryMapper<TSourceKey, TSourceValue, TKey, TValue>(Type destination) where TKey : notnull
            {
                return value =>
                {
                    int count = value is ICollection collection ? collection.Count : 0;
                    var result = new Dictionary<TKey, TValue>(count);
                    foreach (var item in (IEnumerable<KeyValuePair<TSourceKey, TSourceValue>>)value)
                    {
                        TKey key = (TKey)__AdaptRuntime(item.Key!, typeof(TKey));
                        TValue mapped = item.Value is null ? default! : (TValue)__AdaptRuntime(item.Value, typeof(TValue));
                        result[key] = mapped;
                    }
                    if (destination.IsInstanceOfType(result)) return result;
                    return Activator.CreateInstance(destination, result)!;
                };
            }
            """);
        sb.AppendLine("}");
        sb.AppendLine("}");
    }
}
