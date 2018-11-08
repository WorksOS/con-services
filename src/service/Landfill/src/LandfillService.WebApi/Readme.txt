= Application design

== Assumptions

Certain assumptions have been made for the purposes of application design:

- The number of users & projects is on the order of 10^2-10^3
- Users can upload files with several hundred of weight entries
- However, primarily single entries or small batches of entries are made
- Raptor data cannot be retrieved within a reasonable request response cycle
- Raptor API can be down intermittently
- Retrieval of a batch of volumes will always take less than an hour
- A 400 Bad Request response from Raptor is only received when data isn't 
  available for the particular request being made.


== Overview

The service interacts with the Raptor API. It also 
stores data in a MySQL database. 

The Raptor API is used to retrieve daily volume summary information which
is used to calculate density values. 

Simple implementation was favoured over absolute efficiency where the 
low-volume nature of the landfill application permitted it. 


== Authentication & authorisation

The service acts as a proxy between the client and TPaaS with 
regard to authentication and authorisation. 

The user UID is stored in the DB because it's used to 
(a) verify that the user is allowed access to the data in the DB and 
(b) authenticate requests to the Raptor API.

Authentication errors returned by the APIs are propagated to the client,
thus the validity of the sessions is enforced by the APIs rather than by
the service.

Projects are stored in the database as the master data comes in.
The list of projects retrieved from the database is used to display 
the list of available projects to users, and also used to verify that 
users only deal with projects they have permissions for. 

Stale sessions (defined as older than 30 days) are deleted from time to 
time by kicking off a background task (with a 5% chance) from the login 
request handlers.


== Retrieval of volumes

As retrieving data from the Raptor API takes a long time (i.e. longer 
than a reasonable single request cycle), volume retrieval is done in 
background tasks. 

When weight entries are submitted by the client, volume retrieval is 
kicked off in a background task via parallel requests. Parallel requests 
are throttled to prevent overwhelming Raptor and the service.

A background task is created for each client request submitting weights, 
so multiple tasks can be running at a time. 

The service is tolerant to background tasks dying and Raptor API being
intermittently unavailable. It will retry volume retrieval for entries 
where it failed or didn't happen.

Retrying is triggered in two situations: 
- at the end of retrieving a batch of volumes (in case there were 
  short Raptor failures in the process)
- when a user requests project data (useful in case of previous task
  failure)

Only one retry task can be running at a time. The project is "locked" for 
retrying so that additional tasks aren't created. The lock expires within 
an hour so the assumption is that the task always completes before that. 

All projects are unlocked when the service is started as they could be 
left in a locked state when the service terminated.

Note: If a retry task is triggered while a normal volume retrieval
task is running, it can result in redundant requests to Raptor. This is 
a design tradeoff as the probability of it happening is low due to the 
expected pattern of use, while on the other hand it allows tolerance 
to task/service failure. 

The client requests data periodically and is informed about the state of 
volume retrieval (happening/not happening) so that it can choose to 
request data more frequently while new volume data is coming in (and 
update the UI with this new data).


== Time zones

The server and the database time are expected to be set to UTC.

Dates sent by the client are assumed to be in the project time zone.

There is a discrepancy with time zones: the project Master data returns project 
time zones using Windows nomenclature, while MySQL uses IANA nomenclature.

The landfill service resolves this by converting project time zones 
received from the project master data to IANA format, and only dealing with IANA
time zones in the rest of the system. 

NodaTime library is used to deal with time zone manipulation. 

== Deployment

The source contains a file called App.config with URLs for the APIs. 
These should be set by the build script according to the environment 
the service is deployed to.

== Miscellaneous

CORS is enabled for the web service.
 

= LandfillService.Common

Logging releated source code has been copied from the RaptorServices solution. 
The code couldn't be reused due to being located in different source control
systems.

The logging classes have been modified slightly for the requirements of 
landfill service.

