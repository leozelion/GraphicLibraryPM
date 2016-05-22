using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using System.Windows.Forms;

namespace DiscreteDataCompressor
{
    class CodeGenerator
    {


        private static Assembly BuildAssembly()
        {
            CodeDomProvider provider = new CSharpCodeProvider();
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.GenerateInMemory = true;

            CompilerResults compilerResults = provider.CompileAssemblyFromSource(compilerParameters, GetAssemblyCode());
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

        private static string[] GetAssemblyCode()
        {
            //число для границ цикла
            int p = DataInput.i0 / Environment.ProcessorCount;
            StringBuilder codeStringBuilder = new StringBuilder();
            codeStringBuilder.Append("using System;");
            codeStringBuilder.Append("using System.Windows.Forms;");
            codeStringBuilder.AppendFormat("public static class {0}");
            codeStringBuilder.Append("{");
            codeStringBuilder.AppendFormat("public static double {0}(double x)");
            codeStringBuilder.Append("{");
            codeStringBuilder.AppendFormat(@"MessageBox.Show("");");
            codeStringBuilder.Append("}");
            codeStringBuilder.Append("}");
            /*codeStringBuilder.Append(@"using System;
                                         using System.Collections.Generic;
                                         using System.Linq;
                                         using System.Text;
                                         using System.IO;
                                         using System.Threading;
                                         using System.Threading.Tasks;
                                         using System.Windows.Forms;

                                        namespace DiscreteDataCompressor
                                        {
                                            class Comp
                                            {");

                codeStringBuilder.Append(@"
              for (int i = 0; i < Environment.ProcessorCount; i++)
              {
                  MessageBox.Show("");
              }");
                //codeStringBuilder.AppendFormat("() => { for (i = 0; i <= 500; i++)
                //codeStringBuilder.AppendFormat("return (double) ({0});", Cores);
                 () => { for (i = 0; i <= 500; i++)
                     {
                         sum = 0.0;
                         y0 = DataInput.x[DataInput.ix[i]];

                         for (j = 0; j <= N - 1; j++)
                         {
                             x0 = DataInput.x[j];
                             if (j != N - 1)
                             {
                                 x1 = DataInput.x[j + 1];
                             }
                             else x1 = x0;

                             if (j != N - 1)
                             {
                                 sum += DataInput.inputDataFlow[j] * DataInput.biorSystemOnSpline(true, x0, x1, y0);
                             }
                             else
                                 sum += DataInput.inputDataFlow[j] * DataInput.biorSystemOnSpline(false, x0, x1, y0);

                         }//j

                         DataInput.a[i] = sum;
                         //fa.WriteLine(DataInput.a[i]);


                     }//i
                 },
                 () =>
                 {
                     for (i = 501; i <= i0 - 1; i++)
                     {
                         sum = 0.0;
                         y0 = DataInput.x[DataInput.ix[i]];

                         for (j = 0; j <= N - 1; j++)
                         {
                             x0 = DataInput.x[j];
                             if (j != N - 1)
                             {
                                 x1 = DataInput.x[j + 1];
                             }
                             else x1 = x0;

                             if (j != N - 1)
                             {
                                 sum += DataInput.inputDataFlow[j] * DataInput.biorSystemOnSpline(true, x0, x1, y0);
                             }
                             else
                                 sum += DataInput.inputDataFlow[j] * DataInput.biorSystemOnSpline(false, x0, x1, y0);

                         }//j

                         DataInput.a[i] = sum;
                         //fa.WriteLine(DataInput.a[i]);


                     }//i
                 }
              
                 );
             

                 */


            return new string[] { codeStringBuilder.ToString() };
        }

    }


}