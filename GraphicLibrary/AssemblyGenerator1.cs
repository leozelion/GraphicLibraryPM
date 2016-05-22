using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace DiscreteDataCompressor
{
    class AssemblyGenerator
    {
        public delegate double FunctionDelegate(double x);

        private const string ClassName = "Evaluator";
        private const string MethodName = "GetValue";

        private readonly string functionString;
        private MethodInfo methodInfo;

        public AssemblyGenerator(string functionString)
        {
            if (String.IsNullOrEmpty(functionString))
            {
                throw new ArgumentNullException("functionString");
            }
            this.functionString = functionString;
        }

        public FunctionDelegate Function
        {
            get
            {
                return Eval;
            }
        }

        private double Eval(double x)
        {
            return (double)EvalMethod.Invoke(null, new object[] { x });
        }


        private MethodInfo EvalMethod
        {
            get
            {
                if (methodInfo == null)
                {
                    methodInfo = BuildAssembly(functionString).GetType(ClassName).GetMethod(MethodName);
                }
                return methodInfo;
            }
        }

        private static Assembly BuildAssembly(string functionString)
        {
            CodeDomProvider provider = new CSharpCodeProvider();
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.GenerateInMemory = true;

            CompilerResults compilerResults = provider.CompileAssemblyFromSource(compilerParameters, GetAssemblyCode(functionString));
            if (compilerResults.Errors.HasErrors)
            {
                StringBuilder errorStringBuilder = new StringBuilder("Unable to compile assembly.\r\n");
                foreach (CompilerError compilerError in compilerResults.Errors)
                {
                    errorStringBuilder.AppendLine(compilerError.ErrorText);
                }
                throw new ArgumentException(errorStringBuilder.ToString());
            }

            return compilerResults.CompiledAssembly;
        }

        private static string[] GetAssemblyCode(string functionString)
        {
            StringBuilder codeStringBuilder = new StringBuilder();
            codeStringBuilder.Append("using System;");
            codeStringBuilder.AppendFormat("public static class {0}", ClassName);
            codeStringBuilder.Append("{");
            codeStringBuilder.AppendFormat("public static double {0}(double x)", MethodName);
            codeStringBuilder.Append("{");
            codeStringBuilder.AppendFormat("return (double) ({0});", functionString);
            codeStringBuilder.Append("}");
            codeStringBuilder.Append("}");

            return new string[] { codeStringBuilder.ToString() };
        }
    }
}
