using System.Reflection;
using System.Runtime.InteropServices;

#if NET40
[assembly: SecurityRules(SecurityRuleSet.Level1)]
#endif

[assembly: AssemblyCompany("Mary Kay Inc.")]
[assembly: AssemblyProduct("{BuildNumber}")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("Local v0.0")]

[assembly: AssemblyCopyrightAttribute("Copyright © Mary Kay 2013")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif