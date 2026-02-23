using System.Reflection;
var asm = typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly;
var qcc = asm.GetType("Microsoft.EntityFrameworkCore.Query.QueryCompilationContext");
var fields = qcc.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(f => f.Name.Contains("Parameter") || f.Name.Contains("ueryContext"));
foreach (var f in fields) Console.WriteLine(f.FieldType.Name + " " + f.Name + " " + (f.IsStatic ? "static" : "instance"));
Console.WriteLine();
var props = qcc.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(p => p.Name.Contains("Parameter") || p.Name.Contains("ueryContext"));
foreach (var p in props) Console.WriteLine(p.PropertyType.Name + " " + p.Name);
