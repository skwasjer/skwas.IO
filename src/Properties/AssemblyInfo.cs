using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

#if COVERAGE
[assembly: SecurityRules(SecurityRuleSet.Level1)] 
#else
[assembly: AllowPartiallyTrustedCallers]
#endif
[assembly: System.CLSCompliant(true)]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("skwas.IO")]
[assembly: AssemblyDescription("Library with IO classes.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("skwas")]
[assembly: AssemblyProduct("skwas.IO")]
[assembly: AssemblyCopyright("© 2007-2015 skwas")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("f63428dc-36f6-4a6a-88de-b880761c6a87")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("4.0.*")]
