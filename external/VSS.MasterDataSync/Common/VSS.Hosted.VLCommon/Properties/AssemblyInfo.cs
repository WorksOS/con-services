using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

[assembly: AssemblyTitle( "VLCommon" )]

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTests")]


//Maintain the SOAP namespace from the classes moved from datatypes into TrimTracMessages
[assembly: ContractNamespace("http://schemas.datacontract.org/2004/07/Trimble.Construction.DataTypes", ClrNamespace = "VSS.Hosted.VLCommon.TrimTracMessages")]
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
