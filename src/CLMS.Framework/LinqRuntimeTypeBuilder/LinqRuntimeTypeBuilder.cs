using log4net;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

#if NETFRAMEWORK
using Microsoft.CSharp;
#else
using CLMS.Framework.Services;
using CLMS.Framework.Utilities;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
#endif

namespace CLMS.Framework.LinqRuntimeTypeBuilder
{
    public static class LinqRuntimeTypeBuilder
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly AssemblyName AssemblyName = new AssemblyName() { Name = "DynamicLinqTypes" };
        private static readonly ModuleBuilder ModuleBuilder = null;
        private static Dictionary<string, Type> BuiltTypes
        {
            get
            {
                #if NETFRAMEWORK
                    if (CLMS.AppDev.Cache.CacheManager.Current.Contains("BuiltTypes"))
                        return CLMS.AppDev.Cache.CacheManager.Current.Get<Dictionary<string, Type>>("BuiltTypes");
                    var builtTypes = new Dictionary<string, Type>();
                    CLMS.AppDev.Cache.CacheManager.Current.Set("BuiltTypes", builtTypes);
                    return builtTypes;
                #else
                    var cacheManager = ServiceLocator.Current.GetInstance<ICacheWrapperService>();
                    if (cacheManager.Contains("BuiltTypes")) return cacheManager.Get<Dictionary<string, Type>>("BuiltTypes");
                    var builtTypes = new Dictionary<string, Type>();
                    cacheManager.Set("BuiltTypes", builtTypes);
                    return builtTypes;
                #endif
            }
        }

		/*
		Introduced a _monitorLock object reflecting the BuildTypes Dictionary for Locking Purposes, to avoid the SynchronizationLockException. Check the reason here: 
		
		https://docs.microsoft.com/en-us/dotnet/api/system.threading.monitor?redirectedfrom=MSDN&view=netframework-4.7.2
		Each task throws a SynchronizationLockException exception because the nTasks 
		variable is boxed before the call to the Monitor.Enter method in each task. 
		In other words, each method call is passed a separate variable that is independent of the others.[...]
		*/
		private static object _monitorLock = BuiltTypes;		
		
        static LinqRuntimeTypeBuilder()
        {
            ModuleBuilder = AssemblyBuilder
                .DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run)
                .DefineDynamicModule(AssemblyName.Name);
        }

        private static string GetTypeKey(Dictionary<string, Type> fields, Type type, Dictionary<string, Type> selectFields = null)
        {
            //TODO: optimize the type caching -- if fields are simply reordered, that doesn't mean that they're actually different types, so this needs to be smarter
            var key = CSharpName(type, false) + "_";
            foreach (var field in fields)
                key += SanitizeCSharpIdentifier(field.Key) + "_" + CSharpName(field.Value, false) + "_";

            if(selectFields != null)
                foreach (var field in selectFields)
                    key += SanitizeCSharpIdentifier(field.Key) + "_" + CSharpName(field.Value, false) + "_";

            return key;
        }
        public static string SanitizeCSharpIdentifier(string name, string replacementChar = "")
        {
            var className = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
            var isValid = Microsoft.CSharp.CSharpCodeProvider.CreateProvider("C#").IsValidIdentifier(className);
            if (!isValid)
            {
                var regex = new System.Text.RegularExpressions.Regex(@"\W");
                className = regex.Replace(className, replacementChar);
                if (!char.IsLetter(className, 0))
                {
                    className = className.Insert(0, "_");
                }
            }
            return className.Replace(" ", replacementChar);
        }

        public static string CSharpName(this Type type, bool forDeclaration = true)
        {
            if (type == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var name = type.Name;
            if (!type.IsGenericType) return name;
            sb.Append(name.Substring(0, name.IndexOf('`')));
            sb.Append("<");
            sb.Append(string.Join(", ", type.GetGenericArguments()
                                            .Select(t => t.CSharpName())));
            sb.Append(">");

            return forDeclaration ? sb.ToString() : sb.ToString().Replace("<", "").Replace(">", "").Replace(",", "").Replace(" ", "");
        }

        public static Tuple<Type, Type> GetDynamicTypes(Dictionary<string, Type> groupFields, Dictionary<string, Type> selectFields, Type resultSetType)
        {
            if (null == groupFields)
                throw new ArgumentNullException(nameof(groupFields));
            if (0 == groupFields.Count)
                throw new ArgumentOutOfRangeException(nameof(groupFields), "groupFields must have at least 1 field definition");

            if (null == selectFields)
                throw new ArgumentNullException(nameof(selectFields));
            if (0 == selectFields.Count)
                throw new ArgumentOutOfRangeException(nameof(selectFields), "selectFields must have at least 1 field definition");

            Monitor.Enter(_monitorLock);			
            try
            {
                var groupClassName = GetTypeKey(groupFields, resultSetType);
                var selectClassName = GetTypeKey(selectFields, resultSetType, groupFields);
				//This is to ensure that the built groupby and selector expressions reside in the same
				//DummyAssembly in the dictionary. The Dictionary value should be changed to Tuple<Type, Type>
				//and store the groupby and selectors together
                groupClassName += selectClassName;

                if (BuiltTypes.ContainsKey(groupClassName) && BuiltTypes.ContainsKey(selectClassName))
                    return new Tuple<Type, Type>(BuiltTypes[groupClassName], BuiltTypes[selectClassName]);

                var code = $@"

            using System;
            using System.Diagnostics;
			using System.Linq;
			using System.Linq.Expressions;
			using System.Collections.Generic;

            namespace DummyAssembly {{
                [Serializable]
                public class {groupClassName}
                {{
                    {
                        string.Join("\r\n", groupFields.Select(a => "public " + CSharpName(a.Value) + " " + SanitizeCSharpIdentifier(a.Key) + ";").ToArray())
                    }

                    public {groupClassName}({ string.Join(", ", groupFields.Select(a=> CSharpName(a.Value) + " _" + SanitizeCSharpIdentifier(a.Key)).ToArray()) })
                    {{
                        {
                            string.Join("\r\n", groupFields.Select(a => SanitizeCSharpIdentifier(a.Key) + " = _" + SanitizeCSharpIdentifier(a.Key) + ";").ToArray())
                        }
                    }}

                    public override bool Equals(object obj)
                    {{
                        var o = ({groupClassName})obj;
                        return o != null && ({ string.Join(" && ", groupFields.Select(a => $"((this.{SanitizeCSharpIdentifier(a.Key)} != null && this.{SanitizeCSharpIdentifier(a.Key)}.Equals(o.{SanitizeCSharpIdentifier(a.Key)})) || (this.{SanitizeCSharpIdentifier(a.Key)} == null && o.{SanitizeCSharpIdentifier(a.Key)} == null))").ToArray()) });
                    }}

                    public override int GetHashCode()
                    {{
                        if ({string.Join("||", groupFields.Select(a => SanitizeCSharpIdentifier(a.Key) + " == null ").ToArray())})  {{ return 0; }}
                        int hash = 13;
                        {
                            string.Join("\r\n", groupFields.Select(a => "hash = (hash * 7) + " + SanitizeCSharpIdentifier(a.Key) + ".GetHashCode();").ToArray())
                        }
                        return hash;
                    }}
                }}

                [Serializable]
                public class {selectClassName}
                {{
                    public {groupClassName} Key;
                    {
                        string.Join("\r\n", selectFields.Where(a => SanitizeCSharpIdentifier(a.Key) != "Key").Select(a => "public " + CSharpName(a.Value) + " " + SanitizeCSharpIdentifier(a.Key) + ";").ToArray())
                    }

                    public {selectClassName}({groupClassName} _Key, { string.Join(", ", selectFields.Where(a => SanitizeCSharpIdentifier(a.Key) != "Key").Select(a => CSharpName(a.Value) + " _" + SanitizeCSharpIdentifier(a.Key)).ToArray()) })
                    {{
                        {
                            string.Join("\r\n", selectFields.Select(a => SanitizeCSharpIdentifier(a.Key) + " = _" + SanitizeCSharpIdentifier(a.Key) + ";").ToArray())
                        }
                    }}

                    public override bool Equals(object obj)
                    {{
                        var o = ({selectClassName})obj;
                        return o != null && ({ string.Join(" && ", selectFields.Select(a => $"((this.{SanitizeCSharpIdentifier(a.Key)} != null && this.{SanitizeCSharpIdentifier(a.Key)}.Equals(o.{SanitizeCSharpIdentifier(a.Key)})) || (this.{SanitizeCSharpIdentifier(a.Key)} == null && o.{SanitizeCSharpIdentifier(a.Key)} == null ))").ToArray()) });
                    }}

                    public override int GetHashCode()
                    {{
                        if ({string.Join("||", selectFields.Select(a => SanitizeCSharpIdentifier(a.Key) + " == null ").ToArray())})  {{ return 0; }}
                        int hash = 13;
                        {
                            string.Join("\r\n", selectFields.Select(a => "hash = (hash * 7) + " + SanitizeCSharpIdentifier(a.Key) + ".GetHashCode();").ToArray())
                        }
                        return hash;
                    }}
                }}
            }}
";
                var dummyAssembly = BuildAssembly(code);
                BuiltTypes[groupClassName] = dummyAssembly.GetType("DummyAssembly." + groupClassName);
                BuiltTypes[selectClassName] = dummyAssembly.GetType("DummyAssembly." + selectClassName);

                selectFields["Key"] = BuiltTypes[groupClassName];

                return new Tuple<Type, Type>(BuiltTypes[groupClassName], BuiltTypes[selectClassName]);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Debug.WriteLine(ex);
                throw;
            }
            finally
            {
                Monitor.Exit(_monitorLock);
            }
        }

        public static Type GetDynamicType(Dictionary<string, Type> fields, Type type)
        {
            if (null == fields)
                throw new ArgumentNullException(nameof(fields));
            if (0 == fields.Count)
                throw new ArgumentOutOfRangeException(nameof(fields), "fields must have at least 1 field definition");

            try
            {
                Monitor.Enter(BuiltTypes);
                var className = GetTypeKey(fields, type);

                if (BuiltTypes.ContainsKey(className))
                    return BuiltTypes[className];

                var code = $@"

            using System;
            using System.Diagnostics;
			using System.Linq;
			using System.Linq.Expressions;
			using System.Collections.Generic;

            namespace DummyAssembly {{
                [Serializable]
                public class {className}
                {{
                    {
                        string.Join("\r\n", fields.Select(a => "public " + CSharpName(a.Value) + " " + a.Key + ";").ToArray())
                    }

                    public {className}({ string.Join(", ", fields.Select(a=> CSharpName(a.Value) + " _" + a.Key).ToArray()) })
                    {{
                        {
                            string.Join("\r\n", fields.Select(a => a.Key + " = _" + a.Key + ";").ToArray())
                        }
                    }}

                    public override bool Equals(object obj)
                    {{
                        var o = ({className})obj;
                        return o != null && ({ string.Join(" && ", fields.Select(a => $"((this.{a.Key} != null && this.{a.Key}.Equals(o.{a.Key})) || (this.{a.Key} == null && o.{a.Key} == null))").ToArray()) });
                    }}

                    public override int GetHashCode()
                    {{
                        int hash = 13;
                        {
                            string.Join("\r\n", fields.Select(a => "hash = (hash * 7) + " + a.Key + ".GetHashCode();").ToArray())
                        }
                        return hash;
                    }}
                }}

            }}
";
                var dummyAssembly = BuildAssembly(code);
                var matchClassType = dummyAssembly.GetType("DummyAssembly." + className);
                BuiltTypes[className] = matchClassType;

                return BuiltTypes[className];
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Debug.WriteLine(ex);
            }
            finally
            {
                Monitor.Exit(BuiltTypes);
            }

            return null;
        }

        private static Assembly BuildAssembly(string codeToCompile)
        {
#if NETSTANDARD           
            var syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);

            var assemblyName = Path.GetRandomFileName();

            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(BinaryExpression).GetTypeInfo().Assembly.Location), 
            };

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var errors = new StringBuilder("Compiler Errors :\r\n");
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (var error in failures)
                    {
                        errors.AppendFormat("Error {0}: {1}\n",
                            error.Id, error.GetMessage());
                    }

                    throw new ApplicationException(errors.ToString());
                }

                ms.Seek(0, SeekOrigin.Begin);
                return AssemblyLoadContext.Default.LoadFromStream(ms);
            }
#else
            var provider = new CSharpCodeProvider();
            var compilerParameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };
            compilerParameters.ReferencedAssemblies.Add("System.Core.Dll");

            var results = provider.CompileAssemblyFromSource(compilerParameters, codeToCompile);
            if (!results.Errors.HasErrors) return results.CompiledAssembly;

            var errors = new StringBuilder("Compiler Errors :\r\n");
            foreach (CompilerError error in results.Errors)
            {
                errors.AppendFormat("Line {0},{1}\t: {2}\n",
                    error.Line, error.Column, error.ErrorText);
            }

            throw new ApplicationException(errors.ToString());
#endif
        }

        private static Type GetNullableType(Type type)
        {
            if (type == null)
            {
                return null;
            }

            var isStruct = type.IsValueType && !type.IsEnum;
            var isNullable = Nullable.GetUnderlyingType(type) != null;

            var nullableType = !isNullable && isStruct
                                ? typeof(Nullable<>).MakeGenericType(type)
                                : type;

            return nullableType;
        }

        public static Tuple<Expression<Func<T, object>>, Expression<Func<IGrouping<object, T>, object>>> CreateGroupByAndSelectExpressions<T>(List<FieldDefinition<T>> groups, List<FieldDefinition<IGrouping<object, T>>> selectors,
            out Type groupAnonType, out Type selectorAnonType)
        {
            var groupFields = groups.ToDictionary(@group => @group.Name, @group => GetNullableType(@group.Type));
            var selectorFields = selectors.ToDictionary(selector => selector.Name, selector => GetNullableType(selector.Type));

            var types = GetDynamicTypes(groupFields, selectorFields, typeof(T));
            groupAnonType = types.Item1;
            selectorAnonType = types.Item2;

            selectors.First(a => a.Name == "Key").Type = groupAnonType;

            var groupLamda = CombineSelectorsToNewObject(groups, groupAnonType);
            var selectLamda = CombineSelectorsToNewObject(selectors, selectorAnonType);

            return new Tuple<Expression<Func<T, object>>, Expression<Func<IGrouping<object, T>, object>>>(groupLamda, selectLamda);
        }

        public static Expression<Func<T, object>> CombineSelectorsToNewObject<T>(List<FieldDefinition<T>> selectors, Type createdType)
        {
            var param = Expression.Parameter(typeof(T), "x");

            var arguments = new List<Expression>();
            foreach (var selector in selectors)
            {
                var replace = new ParameterReplaceVisitor(selector.Selector.Parameters[0], param);
                var fieldName = SanitizeCSharpIdentifier(selector.Name);
                var property = createdType.GetField(fieldName);
                if (property == null)
                {
                    throw new ApplicationException($"Could not find field {fieldName} for type {createdType}.");
                }
                var replacedParamExpression = replace.Visit(selector.Selector.Body);
                if (replacedParamExpression == null)
                {
                    throw new ApplicationException($"Could Visit and Convert expression: {selector.Selector}");
                }

                replacedParamExpression = Expression.Convert(replacedParamExpression, GetNullableType(selector.Type));
                arguments.Add(replacedParamExpression);
            }

            var ctor = createdType.GetConstructor(selectors.Select(a => a.Type).ToArray());
            if (ctor == null)
            {
                throw new ApplicationException(
                    $"Could not get ctor for created type: {createdType}, with args: {string.Join<Type>(",", selectors.Select(a => a.Type).ToArray())}");
            }

            var ctorExpr = Expression.New(ctor, arguments.ToArray());
            return Expression.Lambda<Func<T, object>>(ctorExpr, param);
        }

        public static Expression<Func<T, object>> CombineSelectorsToNewObject<T>(List<FieldDefinition<T>> groups,
            out Type type)
        {
            var fieldsForGroup = groups.ToDictionary(@group => @group.Name, @group => GetNullableType(@group.Type));

            var createdType = GetDynamicType(fieldsForGroup, typeof(T));
            type = createdType;

            var param = Expression.Parameter(typeof(T), "x");

            var arguments = new List<Expression>();
            foreach (var @group in groups)
            {
                var replace = new ParameterReplaceVisitor(@group.Selector.Parameters[0], param);
                var fieldName = SanitizeCSharpIdentifier(@group.Name);
                var property = createdType.GetField(@group.Name);
                if (property == null)
                {
                    throw new ApplicationException(string.Format("Could not find field {0} for type {1}.", fieldName,
                        createdType));
                }
                var replacedParamExpression = replace.Visit(@group.Selector.Body);
                if (replacedParamExpression == null)
                {
                    throw new ApplicationException(string.Format("Could Visit and Convert expression: {0}",
                        @group.Selector));
                }

                replacedParamExpression = Expression.Convert(replacedParamExpression, GetNullableType(@group.Type));
                arguments.Add(replacedParamExpression);
            }

            var ctor = createdType.GetConstructor(fieldsForGroup.Select(a => a.Value).ToArray());
            if (ctor == null)
            {
                throw new ApplicationException(string.Format("Could not get ctor for created type: {0}, with args: {1}",
                    createdType,
                    string.Join<Type>(",", fieldsForGroup.Select(a => a.Value).ToArray())
                    ));
            }

            var ctorExpr = Expression.New(ctor, arguments.ToArray());
            return Expression.Lambda<Func<T, object>>(ctorExpr, param);
        }        
    }

    public class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from, _to;
        public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
        {
            this._from = from;
            this._to = to;
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : base.VisitParameter(node);
        }
    }

    public class FieldDefinition<T>
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public Expression<Func<T, object>> Selector { get; set; }
    }
}
