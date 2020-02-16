
This README describes the process required to generate the XSD document for distribution to external integrators from types in the VSS.Nighthawk.DataOut.Interfaces.Models namespace.

1.) Run the Visual Studio x64 Win64 Command Prompt (2012)

2.) Change to the "\Server\CommonLibrary\VSS.DataOut.Interfaces\Xsd" directory

3.) Run the following command:

xsd.exe ..\bin\Debug\VSS.Nighthawk.DataOut.Interfaces.dll /type:VSS.Nighthawk.DataOut.Interfaces.Models.*

The above command will generate two (2) XSD documents, schema0.xsd and schema1.xsd as follows:

* schema0.xsd will contain definition for types in VSS.Nighthawk.DataOut.Interfaces.Models

* schema1.xsd will contain definition for System.Guid

4.) Overwrite the contents of DataOutCommandApi.xsd with those of schema0.xsd

5.) Delete definitions and references to OutOfBandTestMessage from the new DataOutCommandApi.xsd contents

6.) Copy the definition of simpleType name="guid" from schema1.xsd

7.) Paste the definition of simpleType name="guid" over <xs:import namespace="http://microsoft.com/wsdl/types/" /> in the new DataOutCommandApi.xsd contents

8.) In the new DataOutCommandApi.xsd contents

* Replace ALL occurances of http://microsoft.com/wsdl/types/ namespace and guid references, e.g.: xmlns:q2="http://microsoft.com/wsdl/types/" type="q1:guid"
* With the following string: type="guid"

9.) Update <xs:schema ... > with: targetNamespace="http://www.myvisionlink.com/TelematicsData/Outbound" xmlns="http://www.myvisionlink.com/TelematicsData/Outbound"

10.) Delete the temporary schema0.xsd and schema1.xsd files


NOTE: If you wish to verify that an integrator will be able to successfully generate types from the built XSD, you can run the following command from the above command prompt:

xsd.exe DataOutCommandApi.xsd /c

This command will generate a .NET class file (DataOutCommandApi.cs) from the XSD file (DataOutCommandApi.xsd).

If successful, XSD.EXE will output something similar to the following text: "Writing file '\Server\CommonLibrary\VSS.DataOut.Interfaces\Xsd\DataOutCommandApi.cs'."