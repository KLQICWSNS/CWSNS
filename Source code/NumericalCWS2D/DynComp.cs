using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Numerics;
using System.Windows.Forms;

namespace NumericalCWS2D
{
    static class DynComp
    {
        internal static Func<double, double, double, double, Complex> dyn = null;
        internal static bool Compile(string code, string[] refdll, bool VB, out string errors)
        {
            var cp = new CompilerParameters();
            cp.CompilerOptions = "/target:library /optimize";
            cp.GenerateInMemory = true;
            cp.GenerateExecutable = false;
            cp.IncludeDebugInformation = false;
            string dll;
            foreach(string i in refdll)
            {
                dll = i.Trim(' ');
                if (dll != "") cp.ReferencedAssemblies.Add(dll);
            }
            var provider = CodeDomProvider.CreateProvider(VB ? "VB" : "C#");
            var cres = provider.CompileAssemblyFromSource(cp, code);
            var errorc = cres.Errors;
            if (errorc.Count != 0)
            {
                string errs = "Compilation failed.";
                foreach (CompilerError i in errorc)
                {
                    if (i.IsWarning)
                    {
                        errs = errs + $"\r\nWarning {i.ErrorNumber} at Line {i.Line}, Column {i.Column}: {i.ErrorText}";
                    }
                    else
                    {
                        errs = errs + $"\r\nError {i.ErrorNumber} at Line {i.Line}, Column {i.Column}: {i.ErrorText}";
                    }
                }
                errors = errs;
                return false;
            }
            var asm = cres.CompiledAssembly;
            Type d1;
            try
            {
                d1 = asm.CreateInstance("CustomWavefunction").GetType();
            }
            catch
            {
                errors = "The class name \"CustomWavefunction\" has been removed, erroneously changed, or its instance cannot be created.";
                return false;
            }
            try
            {
                dyn = (Func<double, double, double, double, Complex>)d1.GetMethod("Wavefunction").CreateDelegate(typeof(Func<double, double, double, double, Complex>));
            }
            catch
            {
                errors = "The method \"Wavefunction\" has been removed or erroneously changed.";
                return false;
            }
            if (dyn == null) throw new Exception();
            errors = null;
            return true;
        }

    }
}
