using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;


namespace Graphic
{
    public class AssemblyGenerator
    {
        public delegate double FunctionDelegate(double x,double y);

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

        private double Eval(double x,double y)
        {
            return (double)EvalMethod.Invoke(null, new object[] { x, y });
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

        // КОСТЫЛЬ ДЛЯ Math.Sin & Math.Cos
        private static string convertMathFunc(string MathFunc)
        {
            string func = MathFunc;
            if (MathFunc.Contains("sin") || MathFunc.Contains("cos"))
            {
                func = MathFunc.Replace("sin", "Math.Sin");
                func = func.Replace("cos", "Math.Cos");
            }
            return func;
        }

        private static string[] GetAssemblyCode(string functionString)
        {
            StringBuilder codeStringBuilder = new StringBuilder();
            codeStringBuilder.Append("using System;");
            codeStringBuilder.AppendFormat("public static class {0}", ClassName);
            codeStringBuilder.Append("{");
            codeStringBuilder.AppendFormat("public static double {0}(double x,double y)", MethodName);
            codeStringBuilder.Append("{");
            // А ВОТ И НАША ФУНКЦИЯ
            codeStringBuilder.AppendFormat("return (double) ({0});", convertMathFunc(functionString));
            codeStringBuilder.Append("}");
            // КОСТЫЛЬ ДЛЯ Math.PI
            codeStringBuilder.Append("public const double Pi = 3.1415926535897931;");
            codeStringBuilder.Append("}");

            return new string[] { codeStringBuilder.ToString() };
        }
    }
}
