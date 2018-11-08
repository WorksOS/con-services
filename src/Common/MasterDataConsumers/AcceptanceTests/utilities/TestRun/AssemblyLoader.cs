using System;
using System.Linq;
using System.Runtime.Loader;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.IO;

namespace TestRun
{
    public class AssemblyLoader : AssemblyLoadContext
    {
        public string FolderPath { get; set;}
        public AssemblyLoader(string folderPath)
        {
            FolderPath = folderPath;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            try
            {
                var deps = DependencyContext.Default;
                var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();  //
                if (res.Count > 0)
                {
                    return Assembly.Load(new AssemblyName(res.First().Name));
                }
                var apiApplicationFileInfo = new FileInfo($"{FolderPath}{Path.DirectorySeparatorChar}{assemblyName.Name}.dll");

                //Console.WriteLine("Load DLL: " + apiApplicationFileInfo.FullName);
                if (!File.Exists(apiApplicationFileInfo.FullName))
                    { return Assembly.Load(assemblyName);}
                var asl = new AssemblyLoader(apiApplicationFileInfo.DirectoryName);
                return asl.LoadFromAssemblyPath(apiApplicationFileInfo.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return null;
            }
        }
    }
}