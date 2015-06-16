= Application design

== Assumptions

Certain assumptions have been made for the purposes of application design:

- The number of users & projects is in the hundreds
- Users can upload files with several hundred of weight entries
- Raptor data cannot be retrieved within a reasonable request response cycle
- The Foreman API is assumed to be available - the landfill service cannot
  function without it
- Raptor API can be down intermittently
- A 400 Bad Request response from Raptor is only received when data isn't 
  available for the particular request being made


== Authentication & authorisation

- sessions
- keys
- project lists


== Retrieval of volumes

- background tasks
- retries
- failure modes

== Time zones

Dates sent by the client are assumed to be in the project time zone.

There is a discrepancy with time zones: the Foreman API returns project 
time zones using Windows nomenclature, while MySQL uses IANA nomenclature.

The landfill services resolves this by converting project time zones 
received from the Foreman API to IANA format, and only dealing with IANA
time zones in the rest of the system. 

NodaTime library is used to deal with time zone manipulation. 

Note that time zone conversions can be ambiguous in some (rare) situations, 
and NodaTime "best effort" conversion will be applied in those situations. 
However, as the system deals with full days, +/- 1 hour time discrepancies
around midnight are not expected to be an issue in practice.


== Miscellaneous

- CORS is enabled for the web service
- 


= LandfillService.Common

Logging releated source code has been copied from the RaptorServices solution. 
The code couldn't be reused due to being located in different source control
systems.

The logging classes have been modified slightly for the requirements of 
LandfillService.

