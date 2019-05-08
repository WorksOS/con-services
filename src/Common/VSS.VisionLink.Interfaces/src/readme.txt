This solution contains interfaces for the kafka messages, 
      which are built into the VSS.Interfaces.netcore nuget package. 
This solution is considered obsolete, 
      as other VSS groups use VSS-Messaging to consume MasterData from Kafka queues.
However, Merino uses our own MasterData consumers which require the Events.MasterData interfaces and models. 
      Also various Merino services which write kafka message e.g. ProjectSvc, need to include this also.

To build the package
   1) update the version in VSS.Visionlink.Interfaces.Core.csproj
   2) build the release instance of VSS.Visionlink.Interfaces.Core.csproj (to check it compiles) 
   3) run package.bat, which will deploy the new VSS.Interfaces.netcore to the vss package server. Note: this address may be out of date?